using Newtonsoft.Json;
using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.Sys.Services
{
	internal class ServerSys : IServerSys
	{
		private IServerSysAuthentication _authentication = null;
		private List<string> _storageProviders = null;
		private IServerSysConnectionStrings _connectionStrings = null;

		[JsonProperty(PropertyName = "database")]
		public string Database { get; set; }
		[JsonProperty(PropertyName = "authentication")]
		public IServerSysAuthentication Authentication
		{
			get
			{
				if (_authentication == null)
					_authentication = new ServerSysAuthentication();

				return _authentication;
			}
		}

		[JsonProperty(PropertyName = "storageProviders")]
		public List<string> StorageProviders
		{
			get
			{
				if (_storageProviders == null)
					_storageProviders = new List<string>();

				return _storageProviders;
			}
		}
		[JsonProperty(PropertyName = "connectionStrings")]
		public IServerSysConnectionStrings ConnectionStrings
		{
			get
			{
				if (_connectionStrings == null)
					_connectionStrings = new ServerSysConnectionStrings();

				return _connectionStrings;
			}
		}

		[JsonProperty(PropertyName = "plugins")]
		public PluginSet Plugins { get; set; }
	}
}
