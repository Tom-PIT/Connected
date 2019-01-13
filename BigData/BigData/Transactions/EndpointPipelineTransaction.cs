using Amt.ComponentModel.Collections;
using Amt.Sdk.DataHub;
using System.Data;

namespace Amt.DataHub.Transactions
{
	internal class EndpointPipelineTransaction : PipelineTransaction
	{
		private Endpoint _endpoint = null;

		public EndpointPipelineTransaction(Endpoint endpoint, DataTable content) : base(content)
		{
			_endpoint = endpoint;
		}

		protected override ListItems<EndpointWorker> Workers
		{
			get
			{
				if (_endpoint == null)
					return null;

				return _endpoint.Workers;
			}
		}
	}
}