using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class FeatureHandler : IFeatureHandler
	{
		public void Delete(IFeature feature)
		{
			var w = new Writer("tompit.feature_del");

			w.CreateParameter("@id", feature.GetId());

			w.Execute();
		}

		public void Insert(IMicroService service, string name, Guid token)
		{
			var w = new Writer("tompit.feature_ins");

			w.CreateParameter("@service", service.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@token", token);

			w.Execute();
		}

		public List<IFeature> Query()
		{
			return new Reader<Feature>("tompit.feature_que").Execute().ToList<IFeature>();
		}

		public IFeature Select(IMicroService microService, Guid token)
		{
			var r = new Reader<Feature>("tompit.feature_sel");

			r.CreateParameter("@service", microService.GetId());
			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public IFeature Select(IMicroService microService, string name)
		{
			var r = new Reader<Feature>("tompit.feature_sel");

			r.CreateParameter("@service", microService.GetId());
			r.CreateParameter("@name", name);

			return r.ExecuteSingleRow();
		}

		public void Update(IFeature feature, string name)
		{
			var w = new Writer("tompit.feature_upd");

			w.CreateParameter("@id", feature.GetId());
			w.CreateParameter("@name", name);

			w.Execute();
		}
	}
}
