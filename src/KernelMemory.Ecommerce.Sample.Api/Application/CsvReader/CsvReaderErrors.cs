using KernelMemory.Ecommerce.Sample.Api.Domain;

namespace KernelMemory.Ecommerce.Sample.Api.Application.CsvReader;

public static class CsvReaderErrors
{
    public static Error ReadRecordsFailed(Exception ex)
    {
        return new Error(
            "CsvReader.ReadRecords.Failed",
            $"An error occurred while parsing the CSV file. Details: {ex.Message}");
    }
}