using TomPIT.Validation;

namespace TomPIT.Design.Validation
{
	public class RequiredValidation : ValidationSettings, IRequiredValidation
	{
		public bool IsRequired
		{
			get; set;
		}
	}
}
