using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;
using TomPIT.App.Models;
using TomPIT.ComponentModel;
using TomPIT.Globalization;
using TomPIT.Middleware;
using TomPIT.Security;
using TomPIT.Serialization;
using TomPIT.UI;

namespace TomPIT.App.UI
{
    public class MailTemplateViewEngine : ViewEngineBase, IMailTemplateViewEngine
    {
        public MailTemplateViewEngine(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, System.IServiceProvider serviceProvider) : base(viewEngine, tempDataProvider, serviceProvider)
        {
        }

        public async Task Render(Guid token)
        {
            Authenticate();

            if (Context.Response.StatusCode != (int)HttpStatusCode.OK)
                return;

            using var model = CreateModel(token);
            var actionContext = CreateActionContext(Context);
            var component = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(token);

            if (component == null)
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
            var content = await CreateContent(view, model);
            var buffer = Encoding.UTF8.GetBytes(content);

            if (Context.Response.StatusCode == (int)HttpStatusCode.OK)
                await Context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            
            await Context.Response.CompleteAsync();
        }

        private JObject Body { get; set; }

        private void Authenticate()
        {
            Body = Context.Request.Body.ToJObject();

            var user = Body.Optional("user", string.Empty);

            if (!string.IsNullOrWhiteSpace(user))
            {
                var u = MiddlewareDescriptor.Current.Tenant.GetService<IUserService>().Select(user);

                if (u == null)
                {
                    if (MiddlewareDescriptor.Current.Tenant.GetService<IAlienService>().Select(new Guid(user)) == null)
                    {
                        Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return;
                    }
                }
                else
                {
                    Context.User = new Principal(new Identity(u));

                    SetCulture(u.Language);
                }
            }

            SetCulture(Body.Optional("language", Guid.Empty));
        }

        private void SetCulture(Guid language)
        {
            if (language == Guid.Empty)
                return;

            var lang = MiddlewareDescriptor.Current.Tenant.GetService<ILanguageService>().Select(language);

            var ci = CultureInfo.GetCultureInfo(lang.Lcid);

            if (ci == null)
                return;

            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        private MailTemplateModel CreateModel(Guid token)
        {
            var ac = CreateActionContext(Context);
            var component = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(token);

            if (component == null)
                return null;

            var arguments = Body.Optional("arguments", string.Empty);
            var ja = string.IsNullOrWhiteSpace(arguments) ? new JObject() : Serializer.Deserialize<JObject>(arguments);
            var model = new MailTemplateModel(Context.Request, ac, Temp, ja);

            model.Initialize(MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(component.MicroService));

            return model;
        }
    }
}
