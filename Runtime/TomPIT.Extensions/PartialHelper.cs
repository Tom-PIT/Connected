using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.IoC;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Runtime;
using TomPIT.Serialization;
using TomPIT.UI;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT
{
    public class PartialHelper : HelperBase
    {
        private List<IRuntimeViewModifier> ViewModifiers => MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceRuntimeService>()
           .QueryRuntimes()
           .Select(e => e?.ViewModifier)
           .Where(e => e is not null)
           .OrderBy(e => e.Priority)
           .ToList();

        public PartialHelper(IHtmlHelper helper) : base(helper)
        {
        }

        public async Task<IHtmlContent> Render([CIP(CIP.PartialProvider)] string name)
        {
            AppendDependenciesHeader(name, null);
            return await RenderView(name, Html.ViewData.Model as IRuntimeModel);
        }


        public async Task<IHtmlContent> Render([CIP(CIP.PartialProvider)] string name, object arguments)
        {
            var model = CreateModel(arguments);
            AppendDependenciesHeader(name, model.Arguments);
            return await RenderView(name, model);
        }

        public async Task<IHtmlContent> Render([CIP(CIP.PartialProvider)] string name, JObject arguments)
        {
            var model = CreateModel(arguments);
            AppendDependenciesHeader(name, model.Arguments);
            return await RenderView(name, model);
        }

        public async Task<IHtmlContent> Render([CIP(CIP.PartialProvider)] string name, PartialRenderArguments e)
        {
            var model = CreateModel(e.Arguments, e.MergeArguments);
            AppendDependenciesHeader(name, model.Arguments);
            return await RenderView(name, model);
        }

        private void AppendDependenciesHeader(string name, object arguments)
        {
            var header = GetInjectionCountHeaderForPartial(name, arguments);
            if (!Shell.HttpContext.Response.Headers.Any(e => e.Key == header.Key))
                Shell.HttpContext.Response.Headers.Add(header);
        }

        private IRuntimeModel CreateModel(object arguments, bool merge = true)
        {
            var a = arguments is null ? null : Serializer.Deserialize<JObject>(arguments);
            var runtimeModel = Html.ViewData.Model as IRuntimeModel;
            var partialModel = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().CreateModel(runtimeModel);

            if (a is not null)
            {
                if (merge)
                    partialModel.MergeArguments(a);
                else
                    partialModel.ReplaceArguments(a);
            }

            return partialModel;
        }

        private async Task<IHtmlContent> RenderView(string name, IRuntimeModel model)
        {
            var modifiers = this.ViewModifiers;

            IComponent component = null;
            IConfiguration configuration = null;
            IMicroService microService = null;

            var modelClone = model.Clone();

            if (modifiers.Any())
            {
                microService = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(name.Split('/').FirstOrDefault());
                component = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(microService.Token, "Partial", name.Split('/').LastOrDefault());
                configuration = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(component.RuntimeConfiguration);

                var preRenderArgs = new PartialViewPreRenderModificationArguments
                {
                    Arguments = modelClone.Arguments,
                    Component = component,
                    MicroService = microService,
                    Configuration = configuration as IPartialViewConfiguration,
                    Name = name
                };

                foreach (var modifier in modifiers)
                {
                    preRenderArgs = modifier.PreRenderPartialView(preRenderArgs);
                }

                modelClone.ReplaceArguments(preRenderArgs.Arguments);
            }

            var content = await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), modelClone);

            if (!modifiers.Any())
                return content;

            var stringContent = GetString(content);

            var postRenderArgs = new PartialViewPostRenderModificationArguments
            {
                Arguments = modelClone.Arguments,
                Component = component,
                MicroService = microService,
                Configuration = configuration as IPartialViewConfiguration,
                Name = name,
                Content = stringContent
            };

            foreach (var modifier in modifiers)
            {
                postRenderArgs.Content = modifier.PostRenderPartialView(postRenderArgs);
            }

            stringContent = postRenderArgs.Content;

            return GetHtmlContent(stringContent);
        }

        private static string GetString(IHtmlContent content)
        {
            using var writer = new StringWriter();
            content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        private static IHtmlContent GetHtmlContent(string html)
        {
            var builder = new HtmlContentBuilder();
            builder.AppendHtmlLine(html);
            return builder;
        }

        private IPartialViewConfiguration ResolveView(string qualifier)
        {
            var tokens = qualifier.Split('/');
            var model = Html.ViewData.Model as IRuntimeModel;
            var ms = model.MicroService;
            var name = qualifier;

            if (tokens.Length > 1)
            {
                ms = model.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

                if (ms is null)
                    return null;

                name = tokens[1];
            }

            return model.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, "Partial", name) as IPartialViewConfiguration;
        }

        public static KeyValuePair<string, StringValues> GetInjectionCountHeaderForPartial(string name, object arguments)
        {
            var dependencies = MiddlewareDescriptor.Current.Tenant.GetService<IUIDependencyInjectionService>().QueryPartialDependencies(name, arguments);
            return new KeyValuePair<string, StringValues>("x-tp-partial-injection-" + name.Replace("/", "--"), (dependencies?.Count ?? 0).ToString());
        }
    }
}