using Amt.Sdk.Net;
using Amt.Sys.Model.Net;
using System;
using System.Text;
using System.Web;
using System.Web.Http;

namespace Amt.DataHub.Controllers
{
	public class TestConnectionController : ApiController
	{
		[Route("TestConnection")]
		[HttpGet]
		public string Test()
		{
			var sb = new StringBuilder();

			var url = VirtualPathUtility.RemoveTrailingSlash(string.Format("{0}{1}", Request.RequestUri.GetLeftPart(UriPartial.Authority), RequestContext.VirtualPathRoot));
			var instance = AmtShell.GetService<IInstanceService>().Select(url, InstanceType.DataHub);

			if (instance == null)
				sb.Append("This is an unregistered instance of the Tom PIT Data Hub server.");
			else
			{
				sb.AppendFormat("{0}.", instance.Name);

				if (instance.Status == InstanceStatus.Inactive)
					sb.Append(" Instance is in inactive state.");
				else
					sb.AppendFormat(" Instance accepts verbs: {0}.", instance.Verbs);

				sb.AppendFormat(" Server (UTC) time is {0}.", DateTime.UtcNow.ToString("G"));
			}

			return sb.ToString();
		}
	}
}