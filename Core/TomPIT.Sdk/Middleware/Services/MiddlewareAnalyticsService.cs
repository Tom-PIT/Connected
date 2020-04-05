namespace TomPIT.Middleware.Services
{
	internal class MiddlewareAnalyticsService : MiddlewareComponent, IMiddlewareAnalyticsService
	{
		private IMiddlewareMruService _mru = null;
		public MiddlewareAnalyticsService(IMiddlewareContext context) : base(context)
		{
		}

		public IMiddlewareMruService Mru
		{
			get
			{
				if (_mru == null)
					_mru = new MiddlewareMruService(Context);

				return _mru;
			}
		}
	}
}
