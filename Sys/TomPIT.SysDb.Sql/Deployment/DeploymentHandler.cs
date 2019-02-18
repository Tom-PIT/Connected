using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Deployment;
using TomPIT.SysDb.Deployment;

namespace TomPIT.SysDb.Sql.Deployment
{
	internal class DeploymentHandler : IDeploymentHandler
	{
		public void Delete(IInstallState state)
		{
			var w = new Writer("tompit.installer_del");

			w.CreateParameter("@package", state.Package);

			w.Execute();
		}

		public void Insert(List<IInstallState> installers)
		{
			var a = new JArray();

			foreach (var i in installers)
			{
				var o = new JObject
				{
					{"package", i.Package }
				};

				if (i.Parent != Guid.Empty)
					o.Add("parent", i.Parent);

				a.Add(o);
			};

			var w = new Writer("tompit.installer_ins");

			w.CreateParameter("@items", JsonConvert.SerializeObject(a));

			w.Execute();
		}

		public void InsertInstallerConfiguration(Guid package, Guid configuration)
		{
			var w = new Writer("tompit.installer_configuration_ins");

			w.CreateParameter("@package", package);
			w.CreateParameter("@configuration", configuration);

			w.Execute();
		}

		public List<IInstallState> QueryInstallers()
		{
			return new Reader<InstallState>("tompit.installer_que").Execute().ToList<IInstallState>();
		}

		public IInstallState SelectInstaller(Guid package)
		{
			var r = new Reader<InstallState>("tompit.installer_sel");

			r.CreateParameter("@package", package);

			return r.ExecuteSingleRow();
		}

		public Guid SelectInstallerConfiguration(Guid package)
		{
			var r = new ScalarReader<Guid>("tompit.installer_configuration_sel");

			r.CreateParameter("@package", package);

			return r.ExecuteScalar(Guid.Empty);
		}

		public void Update(IInstallState state, InstallStateStatus status)
		{
			var w = new Writer("tompit.installer_upd");

			w.CreateParameter("@id", state.GetId());
			w.CreateParameter("@status", status);

			w.Execute();
		}
	}
}
