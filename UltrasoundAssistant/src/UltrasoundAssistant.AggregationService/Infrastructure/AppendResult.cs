namespace UltrasoundAssistant.AggregationService.Infrastructure;

public enum AppendResult
{
    Success = 0,
    DuplicateCommand = 1,
    ConcurrencyConflict = 2
}
