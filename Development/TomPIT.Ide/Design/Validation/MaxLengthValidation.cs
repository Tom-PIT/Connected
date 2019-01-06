using TomPIT.Validation;

namespace TomPIT.Design.Validation
{
	public class MaxLengthValidation : ValidationSettings, IMaxLengthValidation
	{
		public int MaxLength
		{
			get; set;
		}
	}
}
