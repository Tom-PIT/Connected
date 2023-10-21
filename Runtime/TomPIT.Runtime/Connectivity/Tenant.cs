using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Caching;
using TomPIT.Runtime;

namespace TomPIT.Connectivity
{
	public class Tenant : HttpConnection, ITenant, IDependencyInjector, IInstanceMetadataProvider
	{
		private TokenValidationParameters _parameters = null;
		private CachingClient _cachingClient = null;
		private ServiceContainer _serviceContainer = null;
		private IMemoryCache _cache = null;
		private Lazy<ConcurrentDictionary<string, object>> _items = new Lazy<ConcurrentDictionary<string, object>>();
		public Tenant(string url, string authenticationToken) : base(authenticationToken)
		{
			Url = url;

			if (!string.IsNullOrEmpty(Url))
				Task.Run(CachingClient.Connect);
		}

		public string Url { get; }

		public IMemoryCache Cache
		{
			get
			{
				if (_cache == null)
					_cache = new MemoryCache(CacheScope.Shared);

				return _cache;
			}
		}

		private CachingClient CachingClient
		{
			get
			{
				if (_cachingClient == null)
					_cachingClient = new CachingClient(this, AuthenticationToken);

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

		[DebuggerStepThrough]
		public T GetService<T>()
		{
			var r = Services.Get<T>(false);

			if (r == null)
				return Shell.GetService<T>();

			return r;
		}

		public void RegisterService(Type contract, object instance, bool leaveExisting)
		{
			if (leaveExisting && Services.Exists(contract))
				return;

			RegisterService(contract, instance);
		}

		public void RegisterService(Type contract, object instance)
		{
			Services.Register(contract, instance);
		}

		public bool ResolveParameter(Type type, out object instance)
		{
			if (type == typeof(ITenant))
			{
				instance = this;
				return true;
			}

			instance = null;
			return false;
		}

		public TokenValidationParameters ValidationParameters
		{
			get
			{
				if (_parameters == null)
				{
					var p = Instance.SysProxy.Security.SelectValidationParameters();

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

		public Guid InstanceId => Instance.Id;
		public ConcurrentDictionary<string, object> Items { get { return _items.Value; } }
	}
}
