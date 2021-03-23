using TomPIT.Design.Ide.Validation;

namespace TomPIT.Ide.Properties.Validation
{
	public class MaxValueValidation : ValidationSettings, IMaxValueValidation
	{
		public double Value
		{
			get; set;
		}
	}
}
