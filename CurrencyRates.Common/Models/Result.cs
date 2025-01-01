namespace CurrencyRates.Common.Models;

public class Result<TResult>
{
    public Result(TResult result)
    {
        ResultValue = result;
        ErrorMessage = null;
        IsSuccess = true;
    }
    
    public Result(string errorMessage)
    {
        ErrorMessage = errorMessage;
        IsSuccess = false;
    }
    
    public TResult ResultValue { get; set; }
    public string ErrorMessage { get; set; }

    public bool IsSuccess { get; set; }
    public bool IsFailure => !IsSuccess;
    
    public static Result<TResult> Success(TResult result) => new(result);
    public static Result<TResult> Failure(string errorMessage) => new(errorMessage);
}