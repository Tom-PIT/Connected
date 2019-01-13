using Amt.ComponentModel.Dev;
using Amt.DataHub.Transactions;
using Amt.Sdk.DataHub;
using Amt.Sdk.Design;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;

namespace Amt.DataHub.Endpoints
{
	public static class EndpointsModel
	{
		private static Lazy<ConcurrentDictionary<string, Endpoint>> _endpoints = new Lazy<ConcurrentDictionary<string, Endpoint>>();

		static EndpointsModel()
		{
			AmtShell.GetService<IConfigurationService>().ConfigurationChanged += OnConfigurationChanged;
		}

		private static void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
		{
			if (e.Component.Type == Endpoint.TypeId && e.Configuration != null)
			{
				var instance = Endpoints.FirstOrDefault(f => f.Value.DesignId == e.Configuration.DesignId);
				Endpoint ep = null;

				if (instance.Value != null)
					Endpoints.TryRemove(instance.Key, out ep);
			}
		}

		public static Endpoint Select(string url)
		{
			Endpoint result = null;

			if (Endpoints.ContainsKey(url))
			{
				if (Endpoints.TryGetValue(url, out result))
					return result;
			}

			var comps = AmtShell.GetService<IComponentService>().Query(Endpoint.TypeId);

			foreach (var i in comps)
			{
				var ep = AmtShell.GetService<IConfigurationService>().Select<Endpoint>(i.Identifier);

				if (string.Compare(ep.Url, url, true) == 0)
				{
					result = ep;
					Endpoints.TryAdd(url, result);

					break;
				}
			}

			return result;
		}

		public static void Push(string endpointUrl, DataTable data)
		{
			var ep = Select(endpointUrl);
			var transaction = new EndpointPipelineTransaction(ep, data);

			transaction.Start();
		}

		private static ConcurrentDictionary<string, Endpoint> Endpoints { get { return _endpoints.Value; } }
	}
}