using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Development;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers.Development
{
	public class DevelopmentErrorsController : SysController
	{
		[HttpPost]
		public void Clear()
		{
			var body = FromBody();
			var component = body.Required<Guid>("component");
			var element = body.Optional("element", Guid.Empty);

			DataModel.DevelopmentErrors.Clear(component, element);
		}

		[HttpPost]
		public List<IDevelopmentError> Query()
		{
			var body = FromBody();
			var service = body.Required<Guid>("microService");

			return DataModel.DevelopmentErrors.Query(service);
		}

		[HttpPost]
		public void Insert()
		{
			var body = FromBody();
			var component = body.Required<Guid>("component");
			var service = body.Required<Guid>("microService");
			var a = body.Required<JArray>("errors");
			var items = new List<IDevelopmentComponentError>();

			foreach(JObject error in a)
			{
				items.Add(new DevelopmentComponentError
				{
					Element = error.Optional("element", Guid.Empty),
					Message=error.Required<string>("message"),
					Severity=error.Required<DevelopmentSeverity>("severity")
				});
			}

			DataModel.DevelopmentErrors.Insert(service, component, items);
		}
	}
}
