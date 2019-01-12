using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.Environment;
using TomPIT.Sys.Data;
using TomPIT.SysDb.Environment;

namespace TomPIT.Sys.Controllers.Management
{
	public class EnvironmentUnitManagementController : SysController
	{
		[HttpPost]
		public Guid Insert()
		{
			var body = FromBody();

			var name = body.Required<string>("name");
			var parent = body.Optional<Guid>("parent", Guid.Empty);
			var ordinal = body.Required<int>("ordinal");

			return DataModel.EnvironmentUnits.Insert(name, parent, ordinal);
		}

		[HttpPost]
		public void Update()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");
			var name = body.Required<string>("name");
			var parent = body.Optional<Guid>("parent", Guid.Empty);
			var ordinal = body.Required<int>("ordinal");

			DataModel.EnvironmentUnits.Update(token, name, parent, ordinal);
		}

		[HttpPost]
		public void UpdateBatch()
		{
			var body = FromBody();
			var items = body.ToResults();
			var par = new List<EnvironmentUnitBatchDescriptor>();

			foreach (JObject i in items)
			{
				var token = i.Required<Guid>("token");
				var name = i.Required<string>("name");
				var ordinal = i.Required<int>("ordinal");
				var parent = i.Optional("parent", Guid.Empty);

				var unit = DataModel.EnvironmentUnits.GetByToken(token);

				if (unit == null)
					throw new SysException(SR.ErrEnvironmentUnitNotFound);

				IEnvironmentUnit pu = null;

				if (parent != Guid.Empty)
				{
					pu = DataModel.EnvironmentUnits.GetByToken(parent);

					if (pu == null)
						throw new SysException(SR.ErrEnvironmentUnitNotFound);
				}

				par.Add(new EnvironmentUnitBatchDescriptor
				{
					Name = name,
					Ordinal = ordinal,
					Parent = pu,
					Unit = unit
				});
			}

			DataModel.EnvironmentUnits.Update(par);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var token = body.Required<Guid>("token");

			DataModel.EnvironmentUnits.Delete(token);
		}
	}
}
