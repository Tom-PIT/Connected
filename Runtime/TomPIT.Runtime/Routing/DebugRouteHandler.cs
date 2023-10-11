using Microsoft.AspNetCore.Routing;
using System;
using System.Net;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Routing;
internal class DebugRouteHandler : RouteHandlerBase
{
	protected override void OnProcessRequest()
	{
		var ctx = Tenant ?? MiddlewareDescriptor.Current.Tenant;

		if (!ctx.GetService<IAuthorizationService>().Demand(MiddlewareDescriptor.Current.UserToken, SecurityUtils.FullControlRole))
		{
			Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
			return;
		}

		var action = Context.GetRouteValue("action") as string;

		if (string.Equals(action, "ConfigurationChanged", StringComparison.OrdinalIgnoreCase))
			ProcessConfigurationChanged();
		else if (string.Equals(action, "ConfigurationRemoved", StringComparison.OrdinalIgnoreCase))
			ProcessConfigurationRemoved();
		else if (string.Equals(action, "ConfigurationAdded", StringComparison.OrdinalIgnoreCase))
			ProcessConfigurationAdded();
		else if (string.Equals(action, "ScriptChanged", StringComparison.OrdinalIgnoreCase))
			ProcessScriptChanged();
	}

	private void ProcessConfigurationChanged()
	{
		var body = Context.Request.Body.ToJObject();

		var component = body.Required<Guid>("component");

		Instance.SysProxy.Components.Refresh(component);

		var cm = Instance.SysProxy.Components.SelectByToken(component);

		Instance.SysProxy.Development.Notifications.ConfigurationChanged(cm.MicroService, cm.Token, cm.Category);

		var componentNotification = Tenant.GetService<IComponentService>() as IComponentNotification;

		componentNotification.NotifyChanged(this, new ComponentEventArgs(cm.MicroService, cm.Folder, cm.Token, cm.NameSpace, cm.Category, cm.Name));
	}

	private void ProcessConfigurationRemoved()
	{
		var body = Context.Request.Body.ToJObject();

		var component = body.Required<Guid>("component");
		var cm = Instance.SysProxy.Components.SelectByToken(component);

		Instance.SysProxy.Components.Refresh(component);
		Instance.SysProxy.Development.Notifications.ConfigurationRemoved(cm.MicroService, cm.Token, cm.Category);

		var componentNotification = Tenant.GetService<IComponentService>() as IComponentNotification;

		componentNotification.NotifyRemoved(this, new ComponentEventArgs(cm.MicroService, cm.Folder, cm.Token, cm.NameSpace, cm.Category, cm.Name));
	}

	private void ProcessConfigurationAdded()
	{
		var body = Context.Request.Body.ToJObject();

		var component = body.Required<Guid>("component");
		var cm = Instance.SysProxy.Components.SelectByToken(component);

		Instance.SysProxy.Components.Refresh(component);
		Instance.SysProxy.Development.Notifications.ConfigurationAdded(cm.MicroService, cm.Token, cm.Category);

		var componentNotification = Tenant.GetService<IComponentService>() as IComponentNotification;

		componentNotification.NotifyAdded(this, new ComponentEventArgs(cm.MicroService, cm.Folder, cm.Token, cm.NameSpace, cm.Category, cm.Name));
	}

	private void ProcessScriptChanged()
	{
		var body = Context.Request.Body.ToJObject();

		var microService = body.Required<Guid>("microService");
		var component = body.Required<Guid>("component");
		var element = body.Required<Guid>("element");
		var token = body.Required<Guid>("token");
		/*
		 * 4 things needs to be done:
		 * --------------------------------
		 * 1) sys cache must be refreshed so the new data is loaded from the database
		 * 2) caching notification must be performed in order to notify scale out instances
		 * 3) local caching images must be refreshed
		 * 4) compiler service must invalidate script if the blob is script type
		 */

		/*
		 * 1) refresh sys cache
		 */
		Instance.SysProxy.Storage.Refresh(token);
		/*
		 * 2) perform caching notification
		 */
		Instance.SysProxy.Development.Notifications.ScriptChanged(microService, component, element);
		/*
		 * 3) refresh local cache
		 */
		var blob = Instance.SysProxy.Storage.Select(token);
		/*
		 * If blob is null it doesn't exist anymore which means it has been deleted.
		 */
		var storageNotification = Tenant.GetService<IStorageService>() as IStorageNotification;

		if (blob is null)
			storageNotification.NotifyRemoved(this, new BlobEventArgs(microService, token, blob.Type, blob.PrimaryKey));
		else
			storageNotification.NotifyChanged(this, new BlobEventArgs(microService, token, blob.Type, blob.PrimaryKey));
		/*
		 * 4) Invalidate script
		 */
		var compilerNotification = Tenant.GetService<ICompilerService>() as ICompilerNotification;

		compilerNotification.NotifyChanged(this, new ComponentModel.ScriptChangedEventArgs(microService, component, element));
	}
}
