using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using TomPIT.BigData.Nodes;
using TomPIT.BigData.Partitions;
using TomPIT.Connectivity;
using TomPIT.Messaging;

namespace TomPIT.BigData.Connectivity
{
	internal class BigDataClient : HubClient
	{
		public BigDataClient(ITenant tenant, string authenticationToken) : base(tenant, authenticationToken)
		{
			Task.Run(() =>
			{
				Connect();
			});
		}

		protected override string HubName => "bigdata";

		protected override void Initialize()
		{
			Nodes();
			Partitions();
		}

		private void Nodes()
		{
			Hub.On<MessageEventArgs<NodeArgs>>("NodeAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<INodeService>().NotifyChanged(e.Args.Node);
			});

			Hub.On<MessageEventArgs<NodeArgs>>("NodeChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<INodeService>().NotifyChanged(e.Args.Node);
			});

			Hub.On<MessageEventArgs<NodeArgs>>("NodeRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<INodeService>().NotifyRemoved(e.Args.Node);
			});
		}

		private void Partitions()
		{
			Hub.On<MessageEventArgs<PartitionArgs>>("PartitionAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<IPartitionService>().NotifyChanged(e.Args.Configuration);
			});

			Hub.On<MessageEventArgs<PartitionArgs>>("PartitionChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<IPartitionService>().NotifyChanged(e.Args.Configuration);
			});

			Hub.On<MessageEventArgs<PartitionArgs>>("PartitionRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<IPartitionService>().NotifyRemoved(e.Args.Configuration);
			});

			Hub.On<MessageEventArgs<PartitionFileArgs>>("PartitionFileAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<IPartitionService>().NotifyFileChanged(e.Args.FileName);
			});

			Hub.On<MessageEventArgs<PartitionFileArgs>>("PartitionFileChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<IPartitionService>().NotifyFileChanged(e.Args.FileName);
			});

			Hub.On<MessageEventArgs<PartitionFileArgs>>("PartitionFileRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<IPartitionService>().NotifyFileRemoved(e.Args.FileName);
			});

			Hub.On<MessageEventArgs<PartitionFieldStatisticArgs>>("PartitionFieldStatisticsChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<IPartitionService>().NotifyFieldStatisticChanged(e.Args.File, e.Args.FieldName);
			});

			Hub.On<MessageEventArgs<TimeZoneArgs>>("TimezoneChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<ITimeZoneService>().NotifyChanged(e.Args.TimeZone);
			});

			Hub.On<MessageEventArgs<TimeZoneArgs>>("TimezoneAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<ITimeZoneService>().NotifyChanged(e.Args.TimeZone);
			});

			Hub.On<MessageEventArgs<TimeZoneArgs>>("TimezoneRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Tenant.GetService<ITimeZoneService>().NotifyRemoved(e.Args.TimeZone);
			});
		}
	}
}
