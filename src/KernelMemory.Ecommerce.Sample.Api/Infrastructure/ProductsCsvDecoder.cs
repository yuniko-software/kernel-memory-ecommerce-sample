using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.KernelMemory.DataFormats;
using Microsoft.KernelMemory.Pipeline;

namespace KernelMemory.Ecommerce.Sample.Api.Infrastructure;

public class ProductsCsvDecoder : IContentDecoder
{
    public bool SupportsMimeType(string mimeType)
    {
        return mimeType != null && mimeType.StartsWith(MimeTypes.CSVData, StringComparison.OrdinalIgnoreCase);
    }

    public Task<FileContent> DecodeAsync(string filename, CancellationToken cancellationToken = default)
    {
        using var stream = File.OpenRead(filename);
        return DecodeAsync(stream, cancellationToken);
    }

    public Task<FileContent> DecodeAsync(BinaryData data, CancellationToken cancellationToken = default)
    {
        using var stream = data.ToStream();
        return DecodeAsync(stream, cancellationToken);
    }

    // WIP
    public async Task<FileContent> DecodeAsync(Stream data, CancellationToken cancellationToken = default)
    {
        var result = new FileContent(MimeTypes.PlainText);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "|",
            HasHeaderRecord = true,
        };

        using var reader = new StreamReader(data);
        using var csv = new CsvReader(reader, csvConfig);

        var records = await csv
            .GetRecordsAsync<ProductCsvModel>(cancellationToken)
            .ToArrayAsync(cancellationToken);

        for (int i = 0; i < records.Length; i++)
        {
            result.Sections.Add(new FileSection(i, records[i].ToString(), true));
        }

        return result;
    }
}
