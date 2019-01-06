using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Models.MultiTenant
{
	public class MultiTenantLoginModel : LoginModel, IEndpointContext
	{
		[Required]
		public string EndpointUrl { get { return Endpoint; } set { Endpoint = value; } }

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
