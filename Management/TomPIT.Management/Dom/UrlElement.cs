﻿using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Design.Designers;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Environment;
using TomPIT.Ide;
using TomPIT.Ide.Dom;
using TomPIT.Security;
using TomPIT.Security.PermissionDescriptors;

namespace TomPIT.Management.Dom
{
    internal class UrlElement : DomElement, IUrlSecurityScope, IPermissionElement
    {
        private List<string> _urls = null;
        private List<IViewConfiguration> _views = null;
        private List<Guid> _resourceGroups = null;
        private List<string> _claims = null;
        private IPermissionDescriptor _descriptor = null;
        private IDomDesigner _designer = null;
        public UrlElement(IDomElement parent, string path) : base(parent)
        {
            Path = path;

            var tokens = path.Split('/');

            Id = path.Replace('/', '$');
            Title = tokens[tokens.Length - 1];
        }

        public string Path { get; }
        public override bool HasChildren { get { return Urls.Count > 0; } }
        public override int ChildrenCount { get { return Urls.Count; } }

        public override void LoadChildren()
        {
            Urls.Sort();

            foreach (var item in Urls)
                Items.Add(new UrlElement(this, $"{Path}/{item}"));
        }

        public override void LoadChildren(string id)
        {
            var tokens = id.Split('$');
            var url = Urls.FirstOrDefault(f => string.Compare(f, tokens[tokens.Length - 1], true) == 0);

            if (url != null)
                Items.Add(new UrlElement(this, $"{Path}/{url}"));
        }

        private List<string> Urls
        {
            get
            {
                if (_urls == null)
                {
                    _urls = new List<string>();
                    var views = Parent.Closest<UrlSecurityElement>().Views;

                    foreach (var view in views)
                    {
                        if (string.IsNullOrEmpty(view.Url))
                            continue;

                        if (!view.Url.StartsWith(Path, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var path = view.Url.Substring(Path.Length).Trim('/');

                        if (path.Length == 0)
                            continue;

                        var root = path.Split('/')[0];

                        if (_urls.Contains(root.ToLowerInvariant()))
                            continue;

                        _urls.Add(root.ToLowerInvariant());
                    }
                }

                return _urls;
            }
        }

        private List<IViewConfiguration> Views
        {
            get
            {
                if (_views is null)
                {
                    _views = new();

                    var views = Parent.Closest<UrlSecurityElement>().Views;

                    foreach (var view in views)
                    {
                        if (string.IsNullOrEmpty(view.Url))
                            continue;

                        if (!view.Url.StartsWith(Path, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (view.Url.Trim('/').StartsWith(Path))
                            _views.Add(view);
                    }
                }

                return _views;
            }
        }

        private List<Guid> ResourceGroups
        {
            get
            {
                if (_resourceGroups is null)
                {
                    _resourceGroups = new();
                    var msService = Tenant.GetService<IMicroServiceService>();
                    var rgService = Tenant.GetService<IResourceGroupService>();
                    foreach (var view in Views)
                    {
                        var resourceGroup = msService.Select(view.MicroService())?.ResourceGroup ?? default;

                        if (resourceGroup == default)
                            continue;

                        if (!ResourceGroups.Contains(resourceGroup))
                            _resourceGroups.Add(resourceGroup);
                    }
                }
                return _resourceGroups;
            }
        }

        public List<string> Claims
        {
            get
            {
                if (_claims == null)
                    _claims = new List<string>
                          {
                                TomPIT.Claims.AccessUrl
                          };

                return _claims;
            }
        }

        public string PrimaryKey => Path;

        public IPermissionDescriptor PermissionDescriptor
        {
            get
            {
                if (_descriptor == null)
                    _descriptor = new UrlPermissionDescriptor();

                return _descriptor;
            }
        }

        public override IDomDesigner Designer
        {
            get
            {
                if (_designer == null)
                    _designer = new PermissionsDesigner(this);

                return _designer;
            }
        }

        public bool SupportsInherit => true;

        public Guid ResourceGroup
        {
            get
            {
                //var defaultToken = Environment.Context.Tenant.GetService<IResourceGroupService>().Default.Token;

                if (ResourceGroups.Count == 1)
                    return ResourceGroups.First();

                return default;
            }
        }
        public string PermissionComponent => null;
    }
}