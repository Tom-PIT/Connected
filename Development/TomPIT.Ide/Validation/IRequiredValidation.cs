namespace TomPIT.Validation
{
	public interface IRequiredValidation : IValidationSettings
	{
		bool IsRequired { get; }
	}
}