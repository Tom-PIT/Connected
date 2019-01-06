using Microsoft.AspNetCore.Http;
using TomPIT.Runtime.ApplicationContextServices;

namespace TomPIT.Runtime
{
	internal class Services : ApplicationContextClient, IServices, IHttpRequestOwner
	{
		private IDataService _data = null;
		private ITimezoneService _timezone = null;
		private IIdentityService _identity = null;
		private ILocalizationService _localization = null;
		private IRoutingService _routing = null;
		private ILoggingService _log = null;
		private ICachingService _cache = null;
		private IValidationService _validation = null;
		private IEnvironmentService _environment = null;

		public Services(IApplicationContext context, HttpRequest request) : base(context)
		{
			HttpRequest = request;
		}

		public IEnvironmentService Environment
		{
			get
			{
				if (_environment == null)
					_environment = new EnvironmentService(Context);

				return _environment;
			}
		}

		public IValidationService Validation
		{
			get
			{
				if (_validation == null)
					_validation = new ValidationService(Context);

				return _validation;
			}
		}

		public ICachingService Cache
		{
			get
			{
				if (_cache == null)
					_cache = new CachingService();

				return _cache;
			}
		}

		public ILoggingService Log
		{
			get
			{
				if (_log == null)
					_log = new LoggingService(Context);

				return _log;
			}
		}

		public IDataService Data
		{
			get
			{
				if (_data == null)
					_data = new ApplicationContextServices.Data(Context);

				return _data;
			}
		}

		public ITimezoneService Timezone
		{
			get
			{
				if (_timezone == null)
					_timezone = new TimezoneService(Context);

				return _timezone;
			}
		}

		public IIdentityService Identity
		{
			get
			{
				if (_identity == null)
					_identity = new IdentityService(Context);

				return _identity;
			}
		}

		public ILocalizationService Localization
		{
			get
			{
				if (_localization == null)
					_localization = new Localization(Context);

				return _localization;
			}
		}

		public IRoutingService Routing
		{
			get
			{
				if (_routing == null)
					_routing = new ApplicationContextServices.Routing(Context, this);

				return _routing;
			}
		}

		public HttpRequest HttpRequest { get; }
	}
}