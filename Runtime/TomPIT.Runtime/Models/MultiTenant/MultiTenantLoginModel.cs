using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Models.MultiTenant
{
	public class MultiTenantLoginModel : LoginModel, ITenantProvider
	{
		[Required]
		public string EndpointUrl { get; set; }
		public string Endpoint => MiddlewareDescriptor.Current.Tenant.Url;

		public override void MapAuthenticate(JObject data)
		{
			base.MapAuthenticate(data);

			EndpointUrl = data.Required<string>("endpoint");
		}

		public override void MapChangePassword(JObject data)
		{
			base.MapChangePassword(data);

			EndpointUrl = data.Required<string>("endpoint");
		}
	}
}
