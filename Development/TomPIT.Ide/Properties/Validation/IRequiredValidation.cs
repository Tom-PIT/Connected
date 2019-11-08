namespace TomPIT.Ide.Properties.Validation
{
	public interface IRequiredValidation : IValidationSettings
	{
		bool IsRequired { get; }
	}
}