namespace NewsLetter.API.Shared;

// Note! This is a record. Classes are mutable, while records are immutable.
public record Error(string Code, string Message)
{
    // Pre-defined Errors
    public static readonly Error None = new Error(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null");
    public static readonly Error ConditionNotMet = new ("Error.ConditionNotMet", "The specified condition was not met");
}
