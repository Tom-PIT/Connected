﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace TomPIT.Services
{
	internal class ClientSys : IClientSys
	{
		private List<IClientSysConnection> _connections = null;
		private List<string> _dataProviders = null;
		private List<string> _designers = null;
		private List<string> _resourceGroups = null;

		[JsonProperty(PropertyName = "connections")]
		[JsonConverter(typeof(ClientConnectionConverter))]
		public List<IClientSysConnection> Connections
		{
			get
			{
				if (_connections == null)
					_connections = new List<IClientSysConnection>();

				return _connections;
			}
		}
		[JsonProperty(PropertyName = "resourceGroups")]
		public List<string> ResourceGroups
		{
			get
			{
				if (_resourceGroups == null)
					_resourceGroups = new List<string>();

				return _resourceGroups;
			}
		}

		[JsonProperty(PropertyName = "dataProviders")]
		public List<string> DataProviders
		{
			get
			{
				if (_dataProviders == null)
					_dataProviders = new List<string>();

				return _dataProviders;
			}
		}

		[JsonProperty(PropertyName = "designers")]
		public List<string> Designers
		{
			get
			{
				if (_designers == null)
					_designers = new List<string>();

				return _designers;
			}
		}

		[JsonProperty(PropertyName = "plugins")]
		public PluginSet Plugins { get; set; }
	}
}