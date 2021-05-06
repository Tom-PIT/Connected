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
			}, new HttpRequestArgs().WithBearerCredentials(authenticationToken)), new DeployArgs
			{
				ResetMicroService = true
			});
		}

		public void Deploy(IPullRequest request, DeployArgs e)
		{
			new DeploymentSession(Tenant, request).Deploy(e);
		}

		private void Deploy(string fileName)
		{
			IPullRequest request;

			try
			{
				request = Serializer.Deserialize<PullRequest>(File.ReadAllText(fileName));
			}
			catch (Exception ex)
			{
				Tenant.LogError(ex.Source, ex.Message, LogCategories.Deployment);
				return;
			}

			var args = new DeployArgs
			{
				ResetMicroService = true
			};

			args.Commit.Enabled = false;

			Deploy(request, args);
		}
		
		public void Initialize()
		{
			var config = Shell.GetConfiguration<IClientSys>();
			var path = config.Deployment?.FileSystem?.Path;

			if (config.Deployment?.FileSystem?.Enabled != true || string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
				return;

			_watcher = new FileSystemWatcher(path)
			{
				IncludeSubdirectories = false,
				EnableRaisingEvents = true
			};

			_watcher.Filters.Add("*.json");
			_watcher.Filters.Add("*.zip");
			_watcher.NotifyFilter = NotifyFilters.FileName;

			_watcher.Created += OnPullRequest;

			var files = Directory.GetFiles(path, "*.json");

			foreach(var file in files)
				OnPullRequest(this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(file), Path.GetFileName(file)));
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
