using System;
using TomPIT.Caching;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Environment;

namespace TomPIT.Sys.Model.Environment
{
	public class EnvironmentVariablesModel : SynchronizedRepository<IEnvironmentVariable, string>
	{
		public EnvironmentVariablesModel(IMemoryCache container) : base(container, "environmentvars")
		{
		}

		protected override void OnInvalidate(string id)
		{
			var r = Shell.GetService<IDatabaseService>().Proxy.Environment.SelectEnvironmentVariable(id);

			if (r == null)
			{
				Remove(id);
				return;
			}

			Set(id, r, TimeSpan.Zero);
		}

		public IEnvironmentVariable Select(string name)
		{
			return Get(name,
				(f) =>
				{
					var d = Shell.GetService<IDatabaseService>().Proxy.Environment.SelectEnvironmentVariable(name);

					f.AllowNull = true;
					f.Duration = d == null ? TimeSpan.FromMinutes(5) : TimeSpan.Zero;

					Set(name, d);

					return d;
				});
		}

		protected override void OnInitializing()
		{
			var ds = Shell.GetService<IDatabaseService>().Proxy.Environment.QueryEnvironmentVariables();

			foreach (var i in ds)
				Set(i.Name, i, TimeSpan.Zero);
		}

		public void Update(string name, string value)
		{
			Shell.GetService<IDatabaseService>().Proxy.Environment.UpdateEnvironmentVariable(name, value);

			Refresh(name);

			TomPIT.Sys.Api.Configuration.NotifyEnvironmentVariableChanged(this, new TomPIT.Sys.Api.Environment.EnvironmentVariableChangedArgs(name, value));
		}

		public void Delete(string name)
		{
			Shell.GetService<IDatabaseService>().Proxy.Environment.DeleteEnvironmentVariable(name);

			Remove(name);

			TomPIT.Sys.Api.Configuration.NotifyEnvironmentVariableChanged(this, new TomPIT.Sys.Api.Environment.EnvironmentVariableChangedArgs(name, null));
		}
	}
}
