namespace TomPIT.Design.Ide.Validation
{
	public interface IMaxLengthValidation : IValidationSettings
	{
		int MaxLength { get; }
	}
}