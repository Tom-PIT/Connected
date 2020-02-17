using System;
using System.Collections.Generic;
using TomPIT.Analytics;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareMruService : MiddlewareComponent, IMiddlewareMruService
	{
		public MiddlewareMruService(IMiddlewareContext context) : base(context)
		{

		}

		public void Modify(int type, string primaryKey, List<string> tags)
		{
			var user = MiddlewareDescriptor.Current.UserToken;

			if (user == Guid.Empty)
				return;

			Context.Tenant.GetService<IAnalyticsService>().Mru.Modify(type, primaryKey, AnalyticsEntity.User, user.ToString(), tags, 25);
		}

		public List<IMru> Query(List<string> tags)
		{
			var user = MiddlewareDescriptor.Current.UserToken;

			if (user == Guid.Empty)
				return new List<IMru>(); ;

			return Context.Tenant.GetService<IAnalyticsService>().Mru.Query(AnalyticsEntity.User, user.ToString(), tags);
		}
	}
}
