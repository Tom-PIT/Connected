using System;
using System.IO;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Serialization;

namespace TomPIT.Design
{
	internal class Deployment : TenantObject, IDeployment
	{
		private FileSystemWatcher _watcher;

		public Deployment(ITenant tenant) : base(tenant)
		{
			Configuration = new DeploymentConfiguration();

			Initialize();
		}

		public IDeploymentConfiguration Configuration { get; }

		public void Deploy(string remote, Guid repository, long branch, long commit, string authenticationToken)
		{
			var url = $"{remote}/Connected.Repositories/Branches/Pull";

			Deploy(Tenant.Post<PullRequest>(url, new
			{
				repository,
				branch,
				commit,
				Mode = "Content",
				Reason = "Install",
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
			if (!Configuration.FileSystem.Enabled || string.IsNullOrWhiteSpace(Configuration.FileSystem.Path) || !Directory.Exists(Configuration.FileSystem.Path))
				return;

			_watcher = new FileSystemWatcher(Configuration.FileSystem.Path)
			{
				IncludeSubdirectories = false,
				EnableRaisingEvents = true
			};

			_watcher.Filters.Add("*.json");
			_watcher.Filters.Add("*.zip");
			_watcher.NotifyFilter = NotifyFilters.FileName;

			_watcher.Created += OnPullRequest;

			var files = Directory.GetFiles(Configuration.FileSystem.Path, "*.json");

			foreach (var file in files)
				OnPullRequest(this, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(file), Path.GetFileName(file)));
		}

		private void OnPullRequest(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType != WatcherChangeTypes.Created)
				return;

			if (string.Equals(Path.GetExtension(e.FullPath), ".json", StringComparison.OrdinalIgnoreCase))
			{
				Deploy(e.FullPath);
				File.Delete(e.FullPath);
			}
		}
	}
}
