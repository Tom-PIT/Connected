using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
using TomPIT.Development;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class DevelopmentErrorHandler : IDevelopmentErrorHandler
	{
		public void Clear(IComponent component, Guid element, ErrorCategory category)
		{
			var w = new Writer("tompit.dev_error_clr");

			w.CreateParameter("@component", component.GetId());
			w.CreateParameter("@element", element, true);
			w.CreateParameter("@category", category);

			w.Execute();
		}

		public void Delete(Guid identifier)
		{
			var w = new Writer("tompit.dev_error_del");

			w.CreateParameter("@identifier", identifier);

			w.Execute();
		}

		public void Insert(IMicroService microService, IComponent component, List<IDevelopmentError> errors)
		{
			var w = new Writer("tompit.dev_error_ins");
			var a = new JArray();

			foreach (var error in errors)
			{
				var je = new JObject
				{
					{"message", error.Message },
					{"severity", (int)error.Severity },
					{"category", (int)error.Category },
					{"identifier", error.Identifier }
				};

				if (error.Element != Guid.Empty)
					je.Add("element", error.Element);

				if (!string.IsNullOrWhiteSpace(error.Code))
					je.Add("code", error.Code);

				a.Add(je);
			}

			w.CreateParameter("@service", microService.GetId());
			w.CreateParameter("@component", component.GetId());
			w.CreateParameter("@items", a);

			w.Execute();
		}

		public List<IDevelopmentComponentError> Query(IMicroService microService, ErrorCategory category)
		{
			var r = new Reader<DevelopmentError>("tompit.dev_error_que");

			r.CreateParameter("@service", microService == null ? 0 : microService.GetId(), true);
			r.CreateParameter("@category", (int)category, true);

			return r.Execute().ToList<IDevelopmentComponentError>();
		}

		public IDevelopmentComponentError Select(Guid identifier)
		{
			var r = new Reader<DevelopmentError>("tompit.dev_error_sel");

			r.CreateParameter("@identifier", identifier);

			return r.ExecuteSingleRow();
		}
	}
}
