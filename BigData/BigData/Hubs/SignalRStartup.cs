using Amt.DataHub.Hubs;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using System;
using System.Globalization;
using System.Threading;

[assembly: OwinStartup(typeof(SignalRStartup))]
namespace Amt.DataHub.Hubs
{
	internal class SignalRStartup
	{
		public void Configuration(IAppBuilder app)
		{
			SanitizeThreadCulture(app);

			AppDomain.CurrentDomain.Load(typeof(SignalRStartup).Assembly.FullName);

			app.UseCors(CorsOptions.AllowAll);

			app.MapSignalR();
		}

		private void SanitizeThreadCulture(IAppBuilder app)
		{
			var currentCulture = CultureInfo.CurrentCulture;

			var invariantCulture = currentCulture;

			while (invariantCulture.Equals(CultureInfo.InvariantCulture) == false)
				invariantCulture = invariantCulture.Parent;

			if (ReferenceEquals(invariantCulture, CultureInfo.InvariantCulture))
				return;

			var thread = Thread.CurrentThread;

			thread.CurrentCulture = CultureInfo.GetCultureInfo(thread.CurrentCulture.Name);
			thread.CurrentUICulture = CultureInfo.GetCultureInfo(thread.CurrentUICulture.Name);
		}
	}
}