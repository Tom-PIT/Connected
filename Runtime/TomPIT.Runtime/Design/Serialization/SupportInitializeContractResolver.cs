using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel;

namespace TomPIT.Design.Serialization
{
	internal class SupportInitializeContractResolver : DefaultContractResolver
	{
		// As of 7.0.1, Json.NET suggests using a static instance for "stateless" contract resolvers, for performance reasons.
		// http://www.newtonsoft.com/json/help/html/ContractResolver.htm
		// http://www.newtonsoft.com/json/help/html/M_Newtonsoft_Json_Serialization_DefaultContractResolver__ctor_1.htm
		// "Use the parameterless constructor and cache instances of the contract resolver within your application for optimal performance."
		static SupportInitializeContractResolver instance;

		// Using a static constructor enables fairly lazy initialization.  http://csharpindepth.com/Articles/General/Singleton.aspx
		static SupportInitializeContractResolver() { instance = new SupportInitializeContractResolver(); }

		public static SupportInitializeContractResolver Instance { get { return instance; } }

		readonly SerializationCallback onDeserializing;
		readonly SerializationCallback onDeserialized;

		protected SupportInitializeContractResolver()
			 : base()
		{
			onDeserializing = (o, context) =>
			{
				if (o is ISupportInitialize init)
					init.BeginInit();
			};
			onDeserialized = (o, context) =>
			{
				if (o is ISupportInitialize init)
					init.EndInit();
			};
		}

		protected override JsonContract CreateContract(Type objectType)
		{
			var contract = base.CreateContract(objectType);

			if (typeof(ISupportInitialize).IsAssignableFrom(objectType))
			{
				contract.OnDeserializingCallbacks.Add(onDeserializing);
				contract.OnDeserializedCallbacks.Add(onDeserialized);
			}

			return contract;
		}
	}
}