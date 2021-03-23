namespace TomPIT.Design.Ide.Validation
{
	public interface IRequiredValidation : IValidationSettings
	{
		bool IsRequired { get; }
	}
}