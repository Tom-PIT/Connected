using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	public class StringTranslation : ConfigurationElement, IStringTranslation
	{
		public int Lcid { get; set; }
		public string Value { get; set; }
	}
}
