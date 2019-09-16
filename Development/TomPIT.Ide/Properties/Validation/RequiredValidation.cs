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
