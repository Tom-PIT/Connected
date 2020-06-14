using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.IoT.Hubs
{
	internal class TransactionProcessor : IoTProcessor
	{
		public TransactionProcessor(JObject data)
		{
			DeviceName = data.Required<string>("device").ToLowerInvariant();
			Transaction = data.Required<string>("transaction");
			Arguments = data.Required<JObject>("arguments");
		}

		public string Transaction { get; }
		public JObject TransactionArguments { get; private set; }
		public void Process()
		{
			Descriptor.Validate();
			Descriptor.ValidateConfiguration();

			var transaction = FindTransaction(Transaction);
			Serializer.Populate(Arguments, transaction);

			transaction.Invoke();

			TransactionArguments = Serializer.Deserialize<JObject>(Serializer.Serialize(transaction));
		}
	}
}
