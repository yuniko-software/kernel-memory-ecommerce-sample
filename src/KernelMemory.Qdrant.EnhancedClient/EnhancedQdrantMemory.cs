using System.Collections;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Google.Protobuf.Collections;
using Grpc.Core;
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

public sealed class EnhancedQdrantMemory : IMemoryDb, IMemoryDbUpsertBatch, IDisposable
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

    public async Task CreateIndexAsync(string index, int vectorSize, CancellationToken cancellationToken = default)
    {
        index = NormalizeIndexName(index);

        ulong vectorSizeUlong = ConvertIntToUlongStrict(vectorSize);

        try
        {
            await _client.CreateCollectionAsync(
                collectionName: index,
                vectorsConfig: new VectorParams
                {
                    Size = vectorSizeUlong,
                    Distance = Distance.Cosine
                },
                // sparseVectorsConfig: new SparseVectorConfig(), // TODO uncomment to enable sparse vectors
                cancellationToken: cancellationToken);

        }
        catch (RpcException ex)
        {
            // Do not throw if collection already exists, it is expected.
            if (ex.StatusCode == Grpc.Core.StatusCode.AlreadyExists)
            {
                _log.LogDebug("Collection {Index} already exists", index);
                return;
            }

            throw;
        }
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
        return _client.DeleteCollectionAsync(index, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<string>> GetIndexesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> collectionNames = await _client.ListCollectionsAsync(cancellationToken);
        return collectionNames;
    }

    public async IAsyncEnumerable<MemoryRecord> GetListAsync(
        string index,
        ICollection<MemoryFilter>? filters = null,
        int limit = 1,
        bool withEmbeddings = false,
        CancellationToken cancellationToken = default)
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

        List<RetrievedPoint> results;
        try
        {
            var scrollResponse = await _client.ScrollAsync(
                collectionName: index,
                filter: new Filter
                {
                    // TODO
                },
                offset: 0,
                limit: (uint)limit,
                vectorsSelector: new WithVectorsSelector { Enable = withEmbeddings },
                payloadSelector: new WithPayloadSelector { Enable = true },
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (IndexNotFoundException e)
        {
            _log.LogWarning(e, "Index not found");
            // Nothing to return
            yield break;
        }

        foreach (var point in results)
        {
            yield return point.ToMemoryRecord();
        }
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

    public async Task<string> UpsertAsync(
        string index,
        MemoryRecord record,
        CancellationToken cancellationToken = default)
    {
        var result = UpsertBatchAsync(index, [record], cancellationToken);
        var id = await result.SingleAsync(cancellationToken).ConfigureAwait(false);
        return id;
    }

    public async IAsyncEnumerable<string> UpsertBatchAsync(
        string index,
        IEnumerable<MemoryRecord> records,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        index = NormalizeIndexName(index);

        // Call ToList to avoid multiple enumerations
        // (CA1851: Possible multiple enumerations of 'IEnumerable' collection.
        // Consider using an implementation that avoids multiple enumerations).
        var localRecords = records.ToList();

        var qdrantPoints = new List<PointStruct>();
        foreach (var record in localRecords)
        {
            PointStruct qdrantPoint;
            Guid pointId;

            if (string.IsNullOrEmpty(record.Id))
            {
                pointId = Guid.NewGuid();
                record.Id = pointId.ToString("N");

                _log.LogTrace(
                    "Generate new Qdrant point with record ID {RecordId}",
                    record.Id);
            }
            else
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
                    pointId = Guid.NewGuid();
                    _log.LogTrace(
                        "No record with ID {RecordId} found, generated a new point ID {PointId}",
                        record.Id,
                        pointId);
                }
                else
                {
                    pointId = new Guid(existingPoint.Id.Uuid);
                    _log.LogTrace("Point ID {PointId} found, updating...", pointId);
                }
            }

            // TODO [denbell] move code of building PointStruct to a separate method
            MapField<string, Value> payload = BuildPayload(record);

            Vector vector = new Vector()
            {
                // Indices = new SparseIndices(), // TODO [denbell]
                //VectorsCount = (uint)1, // TODO [denbell]
            };
            vector.Data.AddRange(record.Vector.Data.ToArray());

            qdrantPoint = new PointStruct
            {
                Id = pointId,
                Vectors = new Vectors
                {
                    Vector = vector,
                },
            };
            qdrantPoint.Payload.Add(payload);

            qdrantPoints.Add(qdrantPoint);
        }

        UpdateResult updateResult = await _client
            .UpsertAsync(index, qdrantPoints, wait: true, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _log.LogTrace("Result of upsert is {UpdateResultStatus}", updateResult.Status);

        foreach (var record in localRecords)
        {
            yield return record.Id;
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    #region private ================================================================================

    // Note: "_" is allowed in Qdrant, but we normalize it to "-" for consistency with other DBs
    private static readonly Regex s_replaceIndexNameCharsRegex = new(@"[\s|\\|/|.|_|:]", RegexOptions.Compiled);
    private const string ValidSeparator = "-";

    private static MapField<string, Value> BuildPayload(MemoryRecord record)
    {
        var payload = new MapField<string, Value>();
        foreach (var kv in record.Payload)
        {
            Value qValue = null;
            if (kv.Value is null)
            {
                qValue = null;
            }
            else if (kv.Value is string str)
            {
                qValue = str;
            }
            else if (kv.Value is int _int)
            {
                qValue = _int;
            }
            else if (kv.Value is long _long)
            {
                qValue = _long;
            }
            else if (kv.Value is bool _bool)
            {
                qValue = _bool;
            }
#pragma warning disable IDE0045 // Convert to conditional expression
            else if (kv.Value is double _double)
            {
                qValue = _double;
            }
            else if (kv.Value is string[] strArr)
            {
                qValue = strArr;
            }
            else
            {
                throw new NotImplementedException(
                    $"There is no handling for payload value of type {kv.Value.GetType().FullName}");
            }
#pragma warning restore IDE0045 // Convert to conditional expression

            if (qValue is not null)
            {
                payload.Add(kv.Key, qValue);
            }
        }

        return payload;
    }

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
