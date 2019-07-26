using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Globalization;
using TomPIT.Models;
using TomPIT.Security;
using TomPIT.Server.Security;

namespace TomPIT.UI
{
	public class MailTemplateViewEngine : ViewEngineBase, IMailTemplateViewEngine
	{
		public MailTemplateViewEngine(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, System.IServiceProvider serviceProvider) : base(viewEngine, tempDataProvider, serviceProvider)
		{
		}

		public void Render(Guid token)
		{
			Authenticate();

			if (Context.Response.StatusCode != (int)HttpStatusCode.OK)
				return;

			var model = CreateModel(token);
			var actionContext = CreateActionContext(Context);
			var component = Instance.GetService<IComponentService>().SelectComponent(token);

			if(component == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			var viewEngineResult = Engine.FindView(actionContext, $"Dynamic/MailTemplate/{component.MicroService}/{component.Name}", false);

			if (!viewEngineResult.Success)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			var view = viewEngineResult.View;

			if (Context.Response.StatusCode != (int)HttpStatusCode.OK)
				return;

			var invokeArgs = new ViewInvokeArguments(model, Temp);

			if (Shell.HttpContext.Response.StatusCode != (int)HttpStatusCode.OK)
				return;

			var content = CreateContent(view, invokeArgs);

			var buffer = Encoding.UTF8.GetBytes(content);

			if (Context.Response.StatusCode == (int)HttpStatusCode.OK)
				Context.Response.Body.Write(buffer, 0, buffer.Length);
		}

		private JObject Body { get; set; }

		private void Authenticate()
		{
			Body = Context.Request.Body.ToJObject();

			var user = Body.Optional("user", string.Empty);

			if (!string.IsNullOrWhiteSpace(user))
			{
				var u = Instance.GetService<IUserService>().Select(user);

				if (u == null)
				{
					Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
					return;
				}

				Context.User = new Principal(new Identity(u));

				SetCulture(u.Language);
			}

			SetCulture(Body.Optional("language", Guid.Empty));
		}

		private void SetCulture(Guid language)
		{
			if (language == Guid.Empty)
				return;

			var lang = Instance.GetService<ILanguageService>().Select(language);

			var ci = CultureInfo.GetCultureInfo(lang.Lcid);

			if (ci == null)
				return;

			Thread.CurrentThread.CurrentCulture = ci;
			Thread.CurrentThread.CurrentUICulture = ci;
		}

		private MailTemplateModel CreateModel(Guid token)
		{
			var ac = CreateActionContext(Context);
			var component = Instance.GetService<IComponentService>().SelectComponent(token);

			if (component == null)
				return null;

			var arguments = Body.Optional("arguments", string.Empty);
			var ja = string.IsNullOrWhiteSpace(arguments) ? new JObject() : Types.Deserialize<JObject>(arguments);
			var model = new MailTemplateModel(Context.Request, ac, ja);

			model.Initialize(Instance.GetService<IMicroServiceService>().Select(component.MicroService));

			return model;
		}
	}
}
