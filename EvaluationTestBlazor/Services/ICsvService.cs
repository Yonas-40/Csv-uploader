using EvaluationTesst.Models;

namespace EvaluationTestBlazor.Services;

public interface ICsvService
{
    Task<List<CsvUser>> ParseCsvAsync(Stream csvStream);

}
