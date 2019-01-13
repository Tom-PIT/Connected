using Microsoft.AspNetCore.Mvc.Formatters;
using ProtoBuf.Meta;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TomPIT.BigData.Formatters
{
	public class ProtobufFormatter : IOutputFormatter
	{
		private static readonly MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/x-protobuf");
		private static Lazy<RuntimeTypeModel> model = new Lazy<RuntimeTypeModel>(CreateTypeModel);

		public static RuntimeTypeModel Model
		{
			get { return model.Value; }
		}

		//public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContent content, IFormatterLogger formatterLogger)
		//{
		//	var tcs = new TaskCompletionSource<object>();

		//	try
		//	{
		//		object result = Model.Deserialize(stream, null, type);
		//		tcs.SetResult(result);
		//	}
		//	catch (Exception ex)
		//	{
		//		tcs.SetException(ex);
		//	}

		//	return tcs.Task;
		//}

		//public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent content, TransportContext transportContext)
		//{
		//	var tcs = new TaskCompletionSource<object>();

		//	try
		//	{
		//		if (value != null && value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString()))
		//			Model.Serialize(stream, value);

		//		tcs.SetResult(null);
		//	}
		//	catch (Exception ex)
		//	{
		//		tcs.SetException(ex);
		//	}

		//	return tcs.Task;
		//}

		private static RuntimeTypeModel CreateTypeModel()
		{
			var typeModel = TypeModel.Create();
			typeModel.UseImplicitZeroDefaults = false;
			return typeModel;
		}

		//private static bool CanReadTypeCore(Type type)
		//{
		//	return type.GetCustomAttributes(typeof(ProtoContractAttribute), true).Any();
		//}

		public bool CanWriteResult(OutputFormatterCanWriteContext context)
		{
			return false;
		}

		public Task WriteAsync(OutputFormatterWriteContext context)
		{
			throw new NotImplementedException();
		}
	}
}