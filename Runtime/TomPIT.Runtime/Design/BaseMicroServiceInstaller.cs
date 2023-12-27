using Lucene.Net.Analysis;

using Microsoft.Extensions.Configuration;

using NuGet.Protocol.Core.Types;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.Ide.Designers;
using TomPIT.Diagnostics;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.UI.Theming.Loggers;

namespace TomPIT.Design
{
	public class BaseMicroServiceInstaller
	{
		private const string Remote = "https://sys-connected.tompit.com/rest";

		private static bool _stateChanged = false;

		private static string? _baseUrl = null;
		private static string BaseUrl => _baseUrl ??= Shell.Configuration.GetSection("deployment").GetValue("remoteUrl", Remote) ?? Remote;

		private static string AuthenticationKey => Shell.Configuration.GetSection("deployment").GetValue<string>("userToken") ?? string.Empty;

		public static void InitializeInstance() 
		{
			var stage = Tenant.GetService<IRuntimeService>().Stage;

			switch (stage)
			{
				case EnvironmentStage.Development: { EnsureDevelopmentServices(); break; }
				case EnvironmentStage.QualityAssurance: EnsureQualityAssuranceServices(); break;
				case EnvironmentStage.Staging: EnsureStagingServices(); break;
				case EnvironmentStage.Production: EnsureProductionServices(); break;
				default: throw new NotImplementedException();
			};

			//Force restart
			if (_stateChanged)
				System.Environment.Exit(0);			
		}

		private static List<RepositoryData> GetImageServices(long subscription, string name, string? authenticationKey = null)
		{
			var url = $"{BaseUrl}/Connected.Services.App/Development/QueryImageServices";
			
			var imageContents = MiddlewareDescriptor.Current.Tenant.Post<List<RepositoryData>>(url, new
			{
				Subscription = subscription,
				Name = name
			}, new HttpRequestArgs().WithBearerCredentials(authenticationKey ?? AuthenticationKey));

			return imageContents;
		}

		private static List<RepositoryData> GetMissingServices(List<RepositoryData> services) 
		{
			var microServiceService = Tenant.GetService<IMicroServiceService>();

			var toInstall = new List<RepositoryData>();

			foreach (var service in services) 
			{
				var microService = microServiceService.Select(service.Token);
				
				if (microService is null)
					toInstall.Add(service);
			}

			return toInstall;
		}

		private static void InstallServices(List<RepositoryData> services) 
		{
			var logService = Tenant.GetService<ILoggingService>();

			var deploymentService = Tenant.GetService<IDesignService>().Deployment;

			foreach (var service in services) 
			{
				try
				{
					logService.Write(new LogEntry
					{
						Category = nameof(BaseMicroServiceInstaller),
						Message = $"Installing repository {service.Repository}, branch {service.Branch}, commit {service.Commit}",
						Level = System.Diagnostics.TraceLevel.Info
					});

					deploymentService.Deploy(BaseUrl, service.Token, service.Branch, service.Commit, 0, AuthenticationKey);

					logService.Write(new LogEntry
					{
						Category = nameof(BaseMicroServiceInstaller),
						Message = $"Repository {service.Repository}, branch {service.Branch}, commit {service.Commit} installed successfully.",
						Level = System.Diagnostics.TraceLevel.Info
					});
				}
				catch (Exception ex)
				{
					logService.Write(new LogEntry
					{
						Category = nameof(BaseMicroServiceInstaller),
						Message = $"Failed to install repository {service.Repository}, branch {service.Branch}, commit {service.Commit}",
						Level = System.Diagnostics.TraceLevel.Error
					});

					throw;
				}
				finally 
				{
					_stateChanged = true;
				}
			}
		}

		private static void RemoveServices(List<RepositoryData> services)
		{
			var logService = Tenant.GetService<ILoggingService>();

			var deploymentService = Tenant.GetService<IDesignService>().Deployment;

			foreach (var service in services)
			{
				try
				{
					logService.Write(new LogEntry
					{
						Category = nameof(BaseMicroServiceInstaller),
						Message = $"Removing repository {service.Repository}, branch {service.Branch}, commit {service.Commit}",
						Level = System.Diagnostics.TraceLevel.Info
					});

					deploymentService.Deploy(BaseUrl, service.Token, 0, 0, 0, AuthenticationKey, DeploymentVerb.Delete);

					logService.Write(new LogEntry
					{
						Category = nameof(BaseMicroServiceInstaller),
						Message = $"Repository {service.Repository}, branch {service.Branch}, commit {service.Commit} removed successfully.",
						Level = System.Diagnostics.TraceLevel.Info
					});
				}
				catch (Exception ex)
				{
					logService.Write(new LogEntry
					{
						Category = nameof(BaseMicroServiceInstaller),
						Message = $"Failed to remove repository {service.Repository}, branch {service.Branch}, commit {service.Commit}",
						Level = System.Diagnostics.TraceLevel.Error
					});

					throw;
				}
				finally
				{
					_stateChanged = true;
				}
			}
		}

		private static void EnsureDevelopmentServices() 
		{
			var developmentSettings = new StageDeploymentConfiguration();

			if (Shell.Configuration.GetSection("deployment:development").Exists())
			{
				Shell.Configuration.GetSection("deployment").Bind("development", developmentSettings);
			}
			else 
			{
				developmentSettings.Subscription = 99;
				developmentSettings.Image = "Development base";
				developmentSettings.CredentialsOverride = null;
			}

			var services = GetImageServices(developmentSettings.Subscription, developmentSettings.Image, developmentSettings.CredentialsOverride);

			var toInstall = services;

			if (!developmentSettings.Reset)
			{
				toInstall = GetMissingServices(services);
			}
			else 
			{
				RemoveServices(toInstall);
				return;
			}

			InstallServices(toInstall);
		}
				  
		private static void EnsureQualityAssuranceServices() { }
				  
		private static void EnsureStagingServices() { }
				  
		private static void EnsureProductionServices() { }

		private class RepositoryData 
		{
			public Guid Token { get; set; }

			public long Repository { get; set; }

			public long Branch { get; set; }

			public long Commit { get; set; }
		}

		private class StageDeploymentConfiguration
		{
			public int Subscription { get; set; }

			public string Image { get; set; }

			public string CredentialsOverride { get; set; }

			public bool Reset { get; set; }
	}
}
}
