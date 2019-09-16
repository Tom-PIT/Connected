using TomPIT.Environment;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Management.Environment;

namespace TomPIT.Management.Dom
{
	internal class EndpointElement : TransactionElement
	{
		public EndpointElement(IDomElement parent, IInstanceEndpoint endpoint) : base(parent)
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
			Environment.Context.Tenant.GetService<IInstanceEndpointManagementService>().Update(Endpoint.Token, Endpoint.Name, Endpoint.Type, Endpoint.Url, Endpoint.ReverseProxyUrl, Endpoint.Status, Endpoint.Verbs);

			return true;
		}
	}
}
