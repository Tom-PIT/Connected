using System;
using System.IO;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Runtime.Configuration;
using TomPIT.Serialization;

namespace TomPIT.Design
{
	internal class Deployment : TenantObject, IDeployment
	{
		private FileSystemWatcher _watcher;

		public Deployment(ITenant tenant) : base(tenant)
		{
		}

		public void Deploy(string remote, Guid repository, string authenticationToken)
		{
			var url = $"{remote}/Repositories/Branches/Pull";

			Deploy(Tenant.Post<PullRequest>(url, new
			{
				repository
			}, new HttpRequestArgs().WithBearerCredentials(authenticationToken)));
		}

		public void Deploy(IPullRequest request)
		{
			new DeploymentSession(Tenant, request).Deploy();
		}

		private void Deploy(string fileName)
		{
			try
			{
				var request = Serializer.Deserialize<PullRequest>(File.ReadAllText(fileName));
			}
			catch (Exception ex)
			{
				Tenant.LogError(ex.Source, ex.Message, LogCategories.Deployment);
			}
		}

		public void Initialize()
		{
			var config = Shell.GetConfiguration<IClientSys>();

			if (config.Deployment?.FileSystem?.Enabled != true || string.IsNullOrWhiteSpace(config.Deployment.FileSystem.Path) || !Directory.Exists(config.Deployment.FileSystem.Path))
				return;

			_watcher = new FileSystemWatcher(config.Deployment.FileSystem.Path)
			{
				IncludeSubdirectories = false,
				EnableRaisingEvents = true
			};

			_watcher.Filters.Add("*.json");
			_watcher.Filters.Add("*.zip");
			_watcher.NotifyFilter = NotifyFilters.FileName;

			_watcher.Created += OnPullRequest;
		}

		private void OnPullRequest(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType != WatcherChangeTypes.Created)
				return;

			if (string.Compare(Path.GetExtension(e.FullPath), ".json", true) == 0)
			{
				Deploy(e.FullPath);
				File.Delete(e.FullPath);
			}
		}
	}
}
