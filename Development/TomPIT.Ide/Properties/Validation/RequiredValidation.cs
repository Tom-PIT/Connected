using TomPIT.Design.Ide.Validation;

namespace TomPIT.Ide.Properties.Validation
{
	public class RequiredValidation : ValidationSettings, IRequiredValidation
	{
		public bool IsRequired
		{
			get; set;
		}
	}
}
