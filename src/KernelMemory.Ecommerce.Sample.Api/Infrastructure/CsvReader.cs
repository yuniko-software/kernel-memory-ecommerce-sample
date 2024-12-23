using System.Globalization;
using CsvHelper.Configuration;
using KernelMemory.Ecommerce.Sample.Api.Application.CsvReader;
using KernelMemory.Ecommerce.Sample.Api.Domain;

namespace KernelMemory.Ecommerce.Sample.Api.Infrastructure;

public class CsvReader<TRecord> : ICsvReader<TRecord>
{
    private readonly CsvConfiguration _config;

    public CsvReader()
    {
        _config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "|",
            HasHeaderRecord = true,
        };
    }

    public async Task<Result<IReadOnlyCollection<TRecord>>> ReadRecordsAsync(
        Stream stream, CancellationToken cancellationToken = default)
    {
        try
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvHelper.CsvReader(reader, _config);
            var records = await csv
                .GetRecordsAsync<TRecord>(cancellationToken)
                .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyCollection<TRecord>>(records);
        }
        catch (Exception ex)
        {
            var error = CsvReaderErrors.ReadRecordsFailed(ex);
            return Result.Failure<IReadOnlyCollection<TRecord>>(error);
        }
    }
}
