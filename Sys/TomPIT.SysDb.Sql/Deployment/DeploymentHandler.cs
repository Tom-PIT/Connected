using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Data.Sql;
using TomPIT.Deployment;
using TomPIT.SysDb.Deployment;

namespace TomPIT.SysDb.Sql.Deployment
{
	internal class DeploymentHandler : IDeploymentHandler
	{
		public void Delete(IInstallState state)
		{
			using var w = new Writer("tompit.installer_del");

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

			using var w = new Writer("tompit.installer_ins");

			w.CreateParameter("@items", JsonConvert.SerializeObject(a));

			w.Execute();
		}

		public void InsertInstallAudit(InstallAuditType type, Guid package, DateTime created, string message, string version)
		{
			using var w = new Writer("tompit.install_audit_ins");

			w.CreateParameter("@type", type);
			w.CreateParameter("@package", package);
			w.CreateParameter("@created", created);
			w.CreateParameter("@message", message, true);
			w.CreateParameter("@version", version, true);

			w.Execute();
		}

		public void InsertInstallerConfiguration(Guid package, Guid configuration)
		{
			using var w = new Writer("tompit.installer_configuration_ins");

			w.CreateParameter("@package", package);
			w.CreateParameter("@configuration", configuration);

			w.Execute();
		}

		public List<IInstallAudit> QueryInstallAudit(Guid package)
		{
			using var r = new Reader<InstallAudit>("tompit.install_audit_que");

			r.CreateParameter("@package", package);

			return r.Execute().ToList<IInstallAudit>();
		}

		public List<IInstallAudit> QueryInstallAudit(DateTime from)
		{
			using var r = new Reader<InstallAudit>("tompit.install_audit_que");

			r.CreateParameter("@created", from);

			return r.Execute().ToList<IInstallAudit>();
		}

		public List<IInstallState> QueryInstallers()
		{
			using var r = new Reader<InstallState>("tompit.installer_que");

			return r.Execute().ToList<IInstallState>();
		}

		public IInstallState SelectInstaller(Guid package)
		{
			using var r = new Reader<InstallState>("tompit.installer_sel");

			r.CreateParameter("@package", package);

			return r.ExecuteSingleRow();
		}

		public Guid SelectInstallerConfiguration(Guid package)
		{
			using var r = new ScalarReader<Guid>("tompit.installer_configuration_sel");

			r.CreateParameter("@package", package);

			return r.ExecuteScalar(Guid.Empty);
		}

		public void Update(IInstallState state, InstallStateStatus status, string error)
		{
			using var w = new Writer("tompit.installer_upd");

			w.CreateParameter("@id", state.GetId());
			w.CreateParameter("@status", status);
			w.CreateParameter("@error", error, true);

			w.Execute();
		}
	}
}
