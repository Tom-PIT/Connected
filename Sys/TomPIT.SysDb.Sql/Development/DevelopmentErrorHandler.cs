using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
using TomPIT.Development;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class DevelopmentErrorHandler : IDevelopmentErrorHandler
	{
		public void Clear(IComponent component, Guid element)
		{
			var w = new Writer("tompit.dev_error_clr");

			w.CreateParameter("@component", component.GetId());
			w.CreateParameter("@element", element, true);

			w.Execute();
		}

		public void Insert(IMicroService microService, IComponent component, List<IDevelopmentComponentError> errors)
		{
			var w = new Writer("tompit.dev_error_ins");
			var a = new JArray();

			foreach(var error in errors)
			{
				var je = new JObject
				{
					{"message", error.Message },
					{"severity", (int)error.Severity }
				};

				if (error.Element != Guid.Empty)
					je.Add("element", error.Element);

				a.Add(je);
			}

			w.CreateParameter("@service", microService.GetId());
			w.CreateParameter("@component", component.GetId());
			w.CreateParameter("@items", a);

			w.Execute();
		}

		public List<IDevelopmentError> Query(IMicroService microService)
		{
			var r = new Reader<DevelopmentError>("tompit.dev_error_que");

			r.CreateParameter("@service", microService.GetId());

			return r.Execute().ToList<IDevelopmentError>();
		}
	}
}
