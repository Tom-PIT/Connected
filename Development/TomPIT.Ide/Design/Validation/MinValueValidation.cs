using TomPIT.Validation;

namespace TomPIT.Design.Validation
{
	public class MinValueValidation : ValidationSettings, IMinValueValidation
	{
		public double Value
		{
			get; set;
		}
	}
}
