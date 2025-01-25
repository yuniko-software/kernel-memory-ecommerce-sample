using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.MemoryStorage;
using Microsoft.ML;
using Microsoft.ML.Data;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace KernelMemory.Qdrant.EnhancedClient;

public sealed class EnhancedQdrantMemory : IMemoryDb, IDisposable
{
    private readonly ITextEmbeddingGenerator _embeddingGenerator;
    private readonly QdrantClient _client;
    private readonly ILogger<EnhancedQdrantMemory> _log;

    public EnhancedQdrantMemory(
        ITextEmbeddingGenerator embeddingGenerator,
        ILogger<EnhancedQdrantMemory> log)
    {
        _embeddingGenerator = embeddingGenerator;
        _client = new QdrantClient("kernelmemory.ecommerce.sample.qdrant");
        _log = log;
    }

    public Task CreateIndexAsync(string index, int vectorSize, CancellationToken cancellationToken = default)
    {
        index = NormalizeIndexName(index);

        ulong vectorSizeUlong = ConvertIntToUlongStrict(vectorSize);

        return _client.CreateCollectionAsync(
            collectionName: index,
            vectorsConfig: new VectorParams
            {
                Size = vectorSizeUlong,
                Distance = Distance.Cosine
            },
            sparseVectorsConfig: new SparseVectorConfig(),
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(
        string index,
        MemoryRecord record,
        CancellationToken cancellationToken = default)
    {
        index = NormalizeIndexName(index);

        try
        {
            ScrollResponse scrollResponse = await _client
                .ScrollAsync(
                    collectionName: index,
                    filter: MatchText("id", record.Id),
                    limit: 1,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            RetrievedPoint existingPoint = scrollResponse.Result.FirstOrDefault();

            if (existingPoint == null)
            {
                _log.LogTrace("No record with ID {Id} found, nothing to delete", record.Id);
                return;
            }

            _log.LogTrace("Point ID {Id} found, deleting...", existingPoint.Id);
            UpdateResult updateResult = await _client
                .DeleteAsync(
                    collectionName: index,
                    id: existingPoint.Id.Num,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            _log.LogTrace("DeleteAsync status is {Status}", updateResult.Status);
        }
        catch (IndexNotFoundException e)
        {
            _log.LogInformation(e, "Index not found, nothing to delete");
        }
    }

    public Task DeleteIndexAsync(string index, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetIndexesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<MemoryRecord> GetListAsync(string index, ICollection<MemoryFilter>? filters = null, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<(MemoryRecord, double)> GetSimilarListAsync(
        string index,
        string text,
        ICollection<MemoryFilter>? filters = null,
        double minRelevance = 0,
        int limit = 1,
        bool withEmbeddings = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        index = NormalizeIndexName(index);
        if (limit <= 0)
        { limit = int.MaxValue; }

        // Remove empty filters
        filters = filters?.Where(f => !f.IsEmpty()).ToList();

        var requiredTags = new List<IEnumerable<string>>();
        if (filters is { Count: > 0 })
        {
            requiredTags.AddRange(filters.Select(filter => filter.GetFilters().Select(x => $"{x.Key}{Constants.ReservedEqualsChar}{x.Value}")));
        }

        Embedding textEmbedding = await _embeddingGenerator.GenerateEmbeddingAsync(text, cancellationToken).ConfigureAwait(false);
        float[] sparseVector = GetSparseVector(text);

        var results = await _client.QueryAsync(
            collectionName: index,
            prefetch: new List<PrefetchQuery> {
                new() {
                    Query = sparseVector.Select((value, index) => (value, (uint)index)).ToArray(),
                    Using = "sparse",
                    Limit = 20
                },
                new() {
                    Query = textEmbedding.Data.ToArray(),
                    Using = "dense",
                    Limit = 20
                }
            },
            query: Fusion.Rrf,
            cancellationToken: cancellationToken);
        
        // TODO this line is here just to stub the compilation error
        yield break;

        throw new NotImplementedException();
    }

    public Task<string> UpsertAsync(string index, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    #region private ================================================================================

    // Note: "_" is allowed in Qdrant, but we normalize it to "-" for consistency with other DBs
    private static readonly Regex s_replaceIndexNameCharsRegex = new(@"[\s|\\|/|.|_|:]", RegexOptions.Compiled);
    private const string ValidSeparator = "-";

    private static string NormalizeIndexName(string index)
    {
        ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(index, nameof(index), "The index name is empty");
        index = s_replaceIndexNameCharsRegex.Replace(index.Trim().ToLowerInvariant(), ValidSeparator);

        return index.Trim();
    }

    private static ulong ConvertIntToUlongStrict(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Tried to convert negative value to ulong. Only non-negative values are expected here.");
        }

        return (ulong)value;
    }

    #endregion

    public class TextData
    {
        public string Text { get; set; }
    }

    public class TransformedData
    {
        [ColumnName("Features")]
        public float[] Features { get; set; }
    }

    public float[] GetSparseVector(string text)
    {
        var context = new MLContext();

        var data = new List<TextData> { new TextData { Text = text } };
        var dataView = context.Data.LoadFromEnumerable(data);

        var pipeline = context.Transforms.Text.FeaturizeText("Features", nameof(TextData.Text));
        var transformedData = pipeline.Fit(dataView).Transform(dataView);

        var featureColumn = transformedData.GetColumn<float[]>("Features").FirstOrDefault();
        return featureColumn;
    }
}
