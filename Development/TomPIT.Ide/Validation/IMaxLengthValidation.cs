namespace TomPIT.Validation
{
	public interface IMaxLengthValidation : IValidationSettings
	{
		int MaxLength { get; }
	}
}