namespace TomPIT.Design.Ide.Validation
{
	public interface IPropertyValidation
	{
		IRequiredValidation Required { get; }
		IMaxLengthValidation MaxLength { get; }
		IMinValueValidation MinValue { get; }
		IMaxValueValidation MaxValue { get; }
	}
}
