using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.IoC;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Navigation;

namespace TomPIT.App.Models
{
    public class UIInjectionModel : AjaxModel, IViewModel
    {
        private List<IUIDependencyDescriptor> _views = null;
        public IViewConfiguration ViewConfiguration { get; set; }

        public IModelNavigation Navigation => null;
        public string Title => null;
        public IComponent Component { get; set; }

        public ITempDataProvider TempData { get; }
        private string ViewIdentifier { get; set; }
        private string PartialIdentifier { get; set; }
        private string ViewUrl { get; set; }
        protected override void OnDatabinding()
        {
            ViewIdentifier = Body.Required<string>("__view");
            PartialIdentifier = Body.Optional("partial", string.Empty);
            ViewUrl = Body.Optional("__viewUrl", string.Empty);

            var context = FromIdentifier(ViewIdentifier, MiddlewareDescriptor.Current.Tenant);

            MicroService = context.MicroService;

            var descriptor = ComponentDescriptor.View(this, ViewIdentifier);

            descriptor.Validate();

            Component = descriptor.Component;
            QualifierName = $"{MicroService.Name}/{descriptor.ComponentName}";
            ViewConfiguration = descriptor.Configuration;
            Body.Remove("__view");
            Body.Remove("__component");
        }

        public List<IUIDependencyDescriptor> Views
        {
            get
            {
                if (_views is null)
                {
                    if (string.IsNullOrWhiteSpace(PartialIdentifier))
                    {
                        _views = Tenant.GetService<IUIDependencyInjectionService>().QueryViewDependencies(ViewIdentifier, null);

                        var layout = ViewConfiguration.Layout;

                        if (!layout.Contains("/"))
                        {
                            var ms = Tenant.GetService<IMicroServiceService>().Select(ViewConfiguration.MicroService());

                            layout = $"{ms.Name}/{layout}";
                        }
                        var masterViews = Tenant.GetService<IUIDependencyInjectionService>().QueryMasterDependencies(ViewConfiguration.MicroService(), layout, null, ComponentModel.IoC.MasterDependencyKind.Client); ;

                        if (masterViews is not null && masterViews.Count > 0)
                        {
                            if (_views is null)
                                _views = masterViews;
                            else
                                _views.AddRange(masterViews);
                        }
                    }
                    else
                        _views = Tenant.GetService<IUIDependencyInjectionService>().QueryPartialDependencies(PartialIdentifier, null);


                    _views = _views?.OrderBy(f => f.Order).ToList() ?? new();
                }

                return _views;
            }
        }

        protected override void OnInitializing()
        {
            base.OnInitializing();

            if (string.IsNullOrWhiteSpace(ViewUrl))
                return;

            var route = MiddlewareDescriptor.Current.Tenant.GetService<INavigationService>().MatchRoute(Services.Routing.RelativePath(new Uri(ViewUrl).LocalPath), Controller.Request.RouteValues);

            Shell.HttpContext.ParseArguments(Arguments, null, (url) =>
            {
                var route = MiddlewareDescriptor.Current.Tenant.GetService<INavigationService>().MatchRoute(Services.Routing.RelativePath(new Uri(url).LocalPath), Controller.Request.RouteValues);
            });
        }

        public override IRuntimeModel Clone()
        {
            var clone = new UIInjectionModel()
            {
                Body = (JObject)Body.DeepClone(),
                ViewIdentifier = ViewIdentifier,
                PartialIdentifier = PartialIdentifier,
                ViewUrl = ViewUrl,
                MicroService = MicroService,
                Component = Component,
                QualifierName = QualifierName,
                ViewConfiguration =ViewConfiguration
            };

            clone.Initialize(this.Controller, this.MicroService);

            return clone;
        }
    }
}
