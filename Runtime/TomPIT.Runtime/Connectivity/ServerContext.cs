using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TomPIT.Caching;
using TomPIT.Runtime;

namespace TomPIT.Net
{
	internal class ServerContext : ISysContext, IDependencyInjector
	{
		private TokenValidationParameters _parameters = null;
		private ISysConnection _server = null;
		private CachingClient _cachingClient = null;
		private ServiceContainer _serviceContainer = null;
		private IMemoryCache _cache = null;

		public ServerContext(string url, string clientKey)
		{
			Url = url;
			ClientKey = clientKey;

			CachingClient.Connect();
		}

		public string Url { get; }
		private string ClientKey { get; }

		public IMemoryCache Cache
		{
			get
			{
				if (_cache == null)
					_cache = new MemoryCache();

				return _cache;
			}
		}

		public ISysConnection Connection
		{
			get
			{
				if (_server == null)
					_server = new SysConnection(this, ClientKey);

				return _server;
			}
		}

		public CachingClient CachingClient
		{
			get
			{
				if (_cachingClient == null)
					_cachingClient = new CachingClient(this);

				return _cachingClient;
			}
		}

		private ServiceContainer Services
		{
			get
			{
				if (_serviceContainer == null)
					_serviceContainer = new ServiceContainer(this);

				return _serviceContainer;
			}
		}

		public TokenValidationParameters ValidationParameters
		{
			get
			{
				if (_parameters == null)
				{
					var url = this.CreateUrl("Security", "SelectValidationParameters");
					var p = Connection.Get<ValidationParameters>(url);

					_parameters = new TokenValidationParameters
					{
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(p.IssuerSigningKey)),
						ValidAudience = p.ValidAudience,
						ValidIssuer = p.ValidIssuer
					};
				}

				return _parameters;
			}
		}

		public T GetService<T>()
		{
			return Services.Get<T>();
		}

		public void RegisterService(Type contract, object instance)
		{
			Services.Register(contract, instance);
		}

		public bool ResolveParameter(Type type, out object instance)
		{
			if (type == typeof(ISysContext))
			{
				instance = this;
				return true;
			}

			instance = null;
			return false;
		}
	}
}
