namespace TomPIT.Design.Validation
{
	public class ValidationMessage : IValidationMessage
	{
		public string Message { get; set; }
		public ValidationMessageType Type { get; set; }
	}
}
