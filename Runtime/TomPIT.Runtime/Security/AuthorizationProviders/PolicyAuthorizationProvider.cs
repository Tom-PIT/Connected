﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Security;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security.PermissionDescriptors;

namespace TomPIT.Security.AuthorizationProviders
{
    internal class PolicyAuthorizationProvider : IAuthorizationProvider
    {
        public string Id => "Policies";

        public async Task<AuthorizationProviderResult> Authorize(IMiddlewareContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
        {
            var middleware = ResolveMiddleware(context, new Guid(permission.Evidence));

            if (middleware == null)
                return AuthorizationProviderResult.NotHandled;

            return await middleware.Authorize(permission, e, state);
        }

        public async Task<AuthorizationProviderResult> PreAuthorize(IMiddlewareContext context, AuthorizationArgs e, Dictionary<string, object> state)
        {
            return await Task.FromResult(AuthorizationProviderResult.NotHandled);
        }

        private IPermissionDescriptorMiddleware ResolveMiddleware(IMiddlewareContext context, Guid component)
        {
            var configuration = context.Tenant.GetService<IComponentService>().SelectConfiguration(component) as IPermissionDescriptorConfiguration;

            if (configuration == null)
                return null;

            var type = context.Tenant.GetService<ICompilerService>().ResolveType(configuration.MicroService(), configuration, configuration.ComponentName(), false);

            if (type == null)
                return null;

            var ctx = new MicroServiceContext(configuration.MicroService());
            var result = context.Tenant.GetService<ICompilerService>().CreateInstance<IPermissionDescriptorMiddleware>(ctx, type);

            if (result is IMiddlewareObject o)
                ReflectionExtensions.SetPropertyValue(result, nameof(o.Context), context);

            return result;
        }

        public async Task<List<IPermissionSchemaDescriptor>> QueryDescriptors(IMiddlewareContext context)
        {
            var descriptors = context.Tenant.GetService<IComponentService>().QueryComponents(ComponentCategories.PermissionDescriptor);
            var result = new List<IPermissionSchemaDescriptor>();

            foreach (var descriptor in descriptors)
            {
                var ms = context.Tenant.GetService<IMicroServiceService>().Select(descriptor.MicroService);

                result.Add(new SchemaDescriptor
                {
                    Id = descriptor.Token.ToString(),
                    Title = descriptor.Name,
                    Description = ms.Name
                });
            }

            return await Task.FromResult(result);
        }
    }
}
