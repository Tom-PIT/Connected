using Newtonsoft.Json.Linq;
using TomPIT.Serialization;

namespace TomPIT.IoT.Hubs
{
	internal class DataProcessor : IoTProcessor
	{
		public DataProcessor(JObject data)
		{
			DeviceName = data.Required<string>("device");
			Arguments = data.Required<JObject>("data");
		}

		public string Group { get; private set; }
		public object Process()
		{
			Descriptor.Validate();
			Descriptor.ValidateConfiguration();

			Group = string.Format("{0}/{1}", Descriptor.MicroService.Name, Descriptor.Component.Name);

			SetSchema();
			Device.Invoke();
			Serializer.Populate(Device, Schema);

			return Schema;
		}

		private void SetSchema()
		{
			Serializer.Populate(Arguments, Schema);
		}
	}
}
