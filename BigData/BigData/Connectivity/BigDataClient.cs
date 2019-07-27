using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using TomPIT.BigData.Services;
using TomPIT.Connectivity;
using TomPIT.Notifications;

namespace TomPIT.BigData.Connectivity
{
	internal class BigDataClient : HubClient
	{
		public BigDataClient(ISysConnection connection, string authenticationToken) : base(connection, authenticationToken)
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

				Connection.GetService<INodeService>().NotifyChanged(e.Args.Node);
			});

			Hub.On<MessageEventArgs<NodeArgs>>("NodeChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Connection.GetService<INodeService>().NotifyChanged(e.Args.Node);
			});

			Hub.On<MessageEventArgs<NodeArgs>>("NodeRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Connection.GetService<INodeService>().NotifyRemoved(e.Args.Node);
			});
		}

		private void Partitions()
		{
			Hub.On<MessageEventArgs<PartitionArgs>>("PartitionAdded", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Connection.GetService<IPartitionService>().NotifyChanged(e.Args.Configuration);
			});

			Hub.On<MessageEventArgs<PartitionArgs>>("PartitionChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Connection.GetService<IPartitionService>().NotifyChanged(e.Args.Configuration);
			});

			Hub.On<MessageEventArgs<PartitionArgs>>("PartitionRemoved", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				Connection.GetService<IPartitionService>().NotifyRemoved(e.Args.Configuration);
			});
		}
	}
}
