using TomPIT.Services.Context;

namespace TomPIT.Services
{
	internal class ContextServices : ContextClient, IContextServices
	{
		private IContextDataService _data = null;
		private IContextTimezoneService _timezone = null;
		private IContextIdentityService _identity = null;
		private IContextLocalizationService _localization = null;
		private IContextRoutingService _routing = null;
		private IContextDiagnosticService _log = null;
		private IContextCachingService _cache = null;
		private IContextValidationService _validation = null;
		private IContextEnvironmentService _environment = null;
		private IContextStorageService _storage = null;
		private IContextCdnService _cdn = null;
		private IContextIoTService _iot = null;
		private IContextMediaService _media = null;

		public ContextServices(IExecutionContext context) : base(context)
		{
		}

		public IContextMediaService Media
		{
			get
			{
				if (_media == null)
					_media = new ContextMediaService(Context);

				return _media;
			}
		}

		public IContextIoTService IoT
		{
			get
			{
				if (_iot == null)
					_iot = new ContextIoTService(Context);

				return _iot;
			}
		}

		public IContextCdnService Cdn
		{
			get
			{
				if (_cdn == null)
					_cdn = new ContextCdnService(Context);

				return _cdn;
			}
		}

		public IContextStorageService Storage
		{
			get
			{
				if (_storage == null)
					_storage = new ContextStorageService(Context);

				return _storage;
			}
		}

		public IContextEnvironmentService Environment
		{
			get
			{
				if (_environment == null)
					_environment = new ContextEnvironmentService(Context);

				return _environment;
			}
		}

		public IContextValidationService Validation
		{
			get
			{
				if (_validation == null)
					_validation = new ContextValidationService(Context);

				return _validation;
			}
		}

		public IContextCachingService Cache
		{
			get
			{
				if (_cache == null)
					_cache = new ContextCachingService();

				return _cache;
			}
		}

		public IContextDiagnosticService Diagnostic
		{
			get
			{
				if (_log == null)
					_log = new ContextDiagnosticService(Context);

				return _log;
			}
		}

		public IContextDataService Data
		{
			get
			{
				if (_data == null)
					_data = new ContextDataService(Context);

				return _data;
			}
		}

		public IContextTimezoneService Timezone
		{
			get
			{
				if (_timezone == null)
					_timezone = new ContextTimezoneService(Context);

				return _timezone;
			}
		}

		public IContextIdentityService Identity
		{
			get
			{
				if (_identity == null)
					_identity = new ContextIdentityService(Context);

				return _identity;
			}
		}

		public IContextLocalizationService Localization
		{
			get
			{
				if (_localization == null)
					_localization = new ContextLocalizationService(Context);

				return _localization;
			}
		}

		public IContextRoutingService Routing
		{
			get
			{
				if (_routing == null)
					_routing = new ContextRoutingService(Context);

				return _routing;
			}
		}
	}
}