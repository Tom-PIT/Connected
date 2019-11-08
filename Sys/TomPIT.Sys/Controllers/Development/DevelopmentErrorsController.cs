using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Development;
using TomPIT.Storage;
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
			var category = body.Required<ErrorCategory>("category");

			DataModel.DevelopmentErrors.Clear(component, element, category);
		}

		[HttpPost]
		public List<IDevelopmentComponentError> Query()
		{
			var body = FromBody();
			var service = body.Optional("microService", Guid.Empty);
			var category = body.Optional("category", ErrorCategory.NotSet);

			return DataModel.DevelopmentErrors.Query(service, category);
		}

		[HttpPost]
		public IDevelopmentComponentError Select()
		{
			var body = FromBody();
			var identifier = body.Required<Guid>("identifier");

			return DataModel.DevelopmentErrors.Select(identifier);
		}

		[HttpPost]
		public void Insert()
		{
			var body = FromBody();
			var component = body.Required<Guid>("component");
			var service = body.Required<Guid>("microService");
			var a = body.Required<JArray>("errors");
			var items = new List<IDevelopmentError>();

			foreach (JObject error in a)
			{
				items.Add(new DevelopmentError
				{
					Element = error.Optional("element", Guid.Empty),
					Message = error.Required<string>("message"),
					Severity = error.Required<DevelopmentSeverity>("severity"),
					Code = error.Optional("code", string.Empty),
					Category = error.Required<ErrorCategory>("category"),
					Identifier = Guid.NewGuid()
				});
			}

			DataModel.DevelopmentErrors.Insert(service, component, items);
		}

		[HttpPost]
		public void AutoFix()
		{
			var body = FromBody();
			var provider = body.Required<string>("provider");
			var error = body.Required<Guid>("error");

			DataModel.DevelopmentErrors.AutoFix(provider, error);
		}

		[HttpPost]
		public List<IQueueMessage> Dequeue()
		{
			var body = FromBody();
			var count = body.Required<int>("count");

			return DataModel.DevelopmentErrors.Dequeue(count);
		}

		[HttpPost]
		public void Complete()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");

			DataModel.DevelopmentErrors.Complete(popReceipt);
		}

		[HttpPost]
		public void Ping()
		{
			var body = FromBody();
			var popReceipt = body.Required<Guid>("popReceipt");
			var nextVisible = TimeSpan.FromSeconds(body.Required<int>("nextVisible"));

			DataModel.DevelopmentErrors.Ping(popReceipt, nextVisible);
		}
	}
}
