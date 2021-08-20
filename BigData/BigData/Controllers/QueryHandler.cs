using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using TomPIT.BigData.Persistence;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.BigData.Controllers
{
	internal class QueryHandler : MicroServiceContext
	{
		public QueryHandler(HttpContext context)
		{
			Context = context;

			var ms = context.GetRouteValue("microService");
			var partition = context.GetRouteValue("partition");

			var microService = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(ms.ToString());

			if (microService == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			Configuration = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(microService.Token, ComponentCategories.BigDataPartition, partition.ToString()) as IPartitionConfiguration;

			if (Configuration == null)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			MicroService = microService;

			Initialize(MiddlewareDescriptor.Current.Tenant.Url);

			Body = Context.Request.Body.ToType<JArray>();
		}

		public void ProcessRequest()
		{
			var parameters = new List<QueryParameter>();

			foreach (JObject parameter in Body)
			{
				var property = parameter.First as JProperty;

				var value = ((JValue)property.Value).Value;

				try
				{
					value = Serializer.Deserialize<JArray>(value);
				}
				catch { }

				parameters.Add(new QueryParameter
				{
					Name = property.Name,
					Value = value
				});
			}

			var result = MiddlewareDescriptor.Current.Tenant.GetService<IPersistenceService>().Query(Configuration, parameters);

			if (result != null)
			{
				var content = Serializer.Serialize(result);
				var buffer = Encoding.UTF8.GetBytes(content);

				Shell.HttpContext.Response.Clear();
				Shell.HttpContext.Response.ContentLength = buffer.Length;
				Shell.HttpContext.Response.ContentType = "application/json";
				Shell.HttpContext.Response.StatusCode = StatusCodes.Status200OK;

				Shell.HttpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length).Wait();

				Shell.HttpContext.Response.CompleteAsync().Wait(); 
			}
		}

		private IPartitionConfiguration Configuration { get; }
		private HttpContext Context { get; }
		private JArray Body { get; set; }
	}
}
