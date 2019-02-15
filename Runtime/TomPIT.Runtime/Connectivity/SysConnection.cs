using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Caching;
using TomPIT.Services;

namespace TomPIT.Connectivity
{
	public class SysConnection : HttpConnection, ISysConnection, IDependencyInjector, IInstanceMetadataProvider
	{
		private TokenValidationParameters _parameters = null;
		private CachingClient _cachingClient = null;
		private ServiceContainer _serviceContainer = null;
		private IMemoryCache _cache = null;

		public SysConnection(string url, string authenticationToken) : base(authenticationToken)
		{
			Url = url;

			Task.Run(() =>
			{
				CachingClient.Connect();
			});
		}

		public string Url { get; }

		public IMemoryCache Cache
		{
			get
			{
				if (_cache == null)
					_cache = new MemoryCache();

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
			if (type == typeof(ISysConnection))
			{
				instance = this;
				return true;
			}

			instance = null;
			return false;
		}

		private ISysConnection Context { get; }

		public TokenValidationParameters ValidationParameters
		{
			get
			{
				if (_parameters == null)
				{
					var url = this.CreateUrl("Security", "SelectValidationParameters");
					var p = Get<ValidationParameters>(url);

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
	}
}
