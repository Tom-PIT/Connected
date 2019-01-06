using TomPIT.Environment;
using TomPIT.Ide;
using TomPIT.Net;

namespace TomPIT.Dom
{
	internal class EndpointElement : TransactionElement
	{
		public EndpointElement(IEnvironment environment, IDomElement parent, IInstanceEndpoint endpoint) : base(environment, parent)
		{
			Endpoint = endpoint;

			Id = endpoint.Token.ToString();
			Title = Endpoint.Name;
			Glyph = "fal fa-file";
		}

		public override bool HasChildren => false;
		private IInstanceEndpoint Endpoint { get; }

		public override object Component => Endpoint;

		public override bool Commit(object component, string property, string attribute)
		{
			SysContext.GetService<IInstanceEndpointManagementService>().Update(Endpoint.Token, Endpoint.Name, Endpoint.Type, Endpoint.Url, Endpoint.ReverseProxyUrl, Endpoint.Status, Endpoint.Verbs);

			return true;
		}
	}
}
