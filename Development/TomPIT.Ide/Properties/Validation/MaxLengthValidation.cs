using TomPIT.Design.Ide.Validation;

namespace TomPIT.Ide.Properties.Validation
{
	public class MaxLengthValidation : ValidationSettings, IMaxLengthValidation
	{
		public int MaxLength
		{
			get; set;
		}
	}
}
