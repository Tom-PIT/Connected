using TomPIT.Validation;

namespace TomPIT.Design.Validation
{
	public class MaxValueValidation : ValidationSettings, IMaxValueValidation
	{
		public double Value
		{
			get; set;
		}
	}
}
