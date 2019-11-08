namespace TomPIT.Ide.Properties.Validation
{
	public interface IMaxLengthValidation : IValidationSettings
	{
		int MaxLength { get; }
	}
}