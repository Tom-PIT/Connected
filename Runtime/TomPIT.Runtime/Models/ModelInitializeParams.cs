using TomPIT.ComponentModel;

namespace TomPIT.Models
{
	public class ModelInitializeParams
	{
		public string Endpoint { get; set; }
		public IMicroService MicroService { get; set; }
	}
}
