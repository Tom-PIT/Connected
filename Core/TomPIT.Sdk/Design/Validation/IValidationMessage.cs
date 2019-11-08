namespace TomPIT.Design.Validation
{
	public enum ValidationMessageType
	{
		Suggestion = 1,
		Warning = 2,
		Error = 3
	}

	public interface IValidationMessage
	{
		string Message { get; }
		ValidationMessageType Type { get; }
	}
}
