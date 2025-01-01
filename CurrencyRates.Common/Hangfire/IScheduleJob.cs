namespace CurrencyRates.Common.Hangfire;

public interface IScheduleJob
{
    public string CronExpression { get; }
    
    public Task Execute();
}