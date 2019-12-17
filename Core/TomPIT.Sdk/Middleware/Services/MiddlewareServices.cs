using TomPIT.Middleware.Services;

namespace TomPIT.Middleware
{
	internal class MiddlewareServices : MiddlewareObject, IMiddlewareServices
	{
		private IMiddlewareDataService _data = null;
		private IMiddlewareGlobalizationService _globalization = null;
		private IMiddlewareIdentityService _identity = null;
		private IMiddlewareRoutingService _routing = null;
		private IMiddlewareDiagnosticService _log = null;
		private IMiddlewareCachingService _cache = null;
		private IMiddlewareValidationService _validation = null;
		private IMiddlewareStorageService _storage = null;
		private IMiddlewareCdnService _cdn = null;
		private IMiddlewareIoTService _iot = null;
		private IMiddlewareMediaService _media = null;
		private IMiddlewareSearchService _search = null;
		private IMiddlewareBigDataService _bigData = null;
		private IMiddlewareAuthorizationService _authorization = null;
		private IMiddlewareIoCService _ioc = null;
		private IMiddlewareMembershipService _membership = null;

		public MiddlewareServices(IMiddlewareContext context) : base(context)
		{
		}

		public IMiddlewareMediaService Media
		{
			get
			{
				if (_media == null)
					_media = new MiddlewareMediaService(Context);

				return _media;
			}
		}

		public IMiddlewareIoTService IoT
		{
			get
			{
				if (_iot == null)
					_iot = new MiddlewareIoTService(Context);

				return _iot;
			}
		}

		public IMiddlewareCdnService Cdn
		{
			get
			{
				if (_cdn == null)
					_cdn = new MiddlewareCdnService(Context);

				return _cdn;
			}
		}

		public IMiddlewareStorageService Storage
		{
			get
			{
				if (_storage == null)
					_storage = new MiddlewareStorageService(Context);

				return _storage;
			}
		}

		public IMiddlewareValidationService Validation
		{
			get
			{
				if (_validation == null)
					_validation = new MiddlewareValidationService(Context);

				return _validation;
			}
		}

		public IMiddlewareCachingService Cache
		{
			get
			{
				if (_cache == null)
					_cache = new MiddlewareCachingService(Context);

				return _cache;
			}
		}

		public IMiddlewareDiagnosticService Diagnostic
		{
			get
			{
				if (_log == null)
					_log = new MiddlewareDiagnosticService(Context);

				return _log;
			}
		}

		public IMiddlewareDataService Data
		{
			get
			{
				if (_data == null)
					_data = new MiddlewareDataService(Context);

				return _data;
			}
		}

		public IMiddlewareGlobalizationService Globalization
		{
			get
			{
				if (_globalization == null)
					_globalization = new MiddlewareGlobalizationService(Context);

				return _globalization;
			}
		}

		public IMiddlewareIdentityService Identity
		{
			get
			{
				if (_identity == null)
					_identity = new MiddlewareIdentityService(Context);

				return _identity;
			}
		}

		public IMiddlewareRoutingService Routing
		{
			get
			{
				if (_routing == null)
					_routing = new MiddlewareRoutingService(Context);

				return _routing;
			}
		}

		public IMiddlewareSearchService Search
		{
			get
			{
				if (_search == null)
					_search = new MiddlewareSearchService(Context);

				return _search;
			}
		}

		public IMiddlewareBigDataService BigData
		{
			get
			{
				if (_bigData == null)
					_bigData = new MiddlewareBigDataService(Context);

				return _bigData;
			}
		}

		public IMiddlewareAuthorizationService Authorization
		{
			get
			{
				if (_authorization == null)
					_authorization = new MiddlewareAuthorizationService(Context);

				return _authorization;
			}
		}

		public IMiddlewareIoCService IoC
		{
			get
			{
				if (_ioc == null)
					_ioc = new MiddlewareIoCService(Context);

				return _ioc;
			}
		}

		public IMiddlewareMembershipService Membership
		{
			get
			{
				if (_membership == null)
					_membership = new MiddlewareMembershipService(Context);

				return _membership;
			}
		}
	}
}