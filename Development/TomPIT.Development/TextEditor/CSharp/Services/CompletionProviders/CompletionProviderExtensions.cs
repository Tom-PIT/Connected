using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Middleware;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal static class CompletionProviderExtensions
	{
		public static (IOrmProvider, string) CreateOrmProvider(this CompletionProviderArgs e, IModelConfiguration configuration)
		{
			if (!(e.Editor.Context.Tenant.GetService<IComponentService>().SelectConfiguration(configuration.Connection) is IConnectionConfiguration connection))
				throw new RuntimeException(nameof(CompletionProviderExtensions), SR.ErrConnectionNotFound, LogCategories.Development);

			var ctx = new MicroServiceContext(configuration.MicroService());
			var cs = connection.ResolveConnectionString(ctx, ConnectionStringContext.User);

			if (cs.DataProvider == Guid.Empty)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotSet, connection.ComponentName()))
				{
					Component = connection.Component,
					EventId = MiddlewareEvents.OpenConnection,
				};
			}

			var provider = e.Editor.Context.Tenant.GetService<IDataProviderService>().Select(cs.DataProvider);

			if (provider == null)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotFound, connection.ComponentName()))
				{
					Component = connection.Component,
					EventId = MiddlewareEvents.OpenConnection
				};
			}

			if (provider is IOrmProvider orm)
				return (orm, cs.Value);

			return (null, null);
		}

		public static List<ICompletionItem> NoConnectionSet(this IModelConfiguration configuration)
		{
			return new List<ICompletionItem>
			{
				new CompletionItem
				{
					Kind = CompletionItemKind.Text,
					Label="No connection set on the model",
					Detail="Please set the connection property on the model to make it functional."
				}
			};
		}

		public static List<ICompletionItem> NoOp(this IModelConfiguration configuration)
		{
			return new List<ICompletionItem>
			{
				new CompletionItem
				{
					Kind = CompletionItemKind.Text,
					Label="No operations",
					Detail="This model has no operations defined or its data provider does not allow discovering operations."
				}
			};
		}
	}
}
