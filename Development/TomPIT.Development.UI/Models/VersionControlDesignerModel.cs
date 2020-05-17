using Newtonsoft.Json.Linq;

namespace TomPIT.Development.Models
{
	public class VersionControlDesignerModel : DevelopmentModel
	{
		public VersionControlDesignerModel(JObject arguments)
		{
			Arguments = arguments;
		}

		public JObject Arguments { get; }
	}
}
