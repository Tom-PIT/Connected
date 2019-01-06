using Newtonsoft.Json.Linq;
using System;
using TomPIT.Services;
using TomPIT.Services.Context;

namespace TomPIT.ComponentModel.Apis
{
	public class OperationInvokeArguments : OperationArguments
	{
		public OperationInvokeArguments(IExecutionContext sender, IApiOperation operation, JObject arguments, IApiTransaction apiTransaction) : base(sender, operation, arguments)
		{
			ApiTransaction = apiTransaction;
		}

		public IApiTransaction ApiTransaction { get; }

		public IApiTransaction BeginTransaction()
		{
			return new ApiTransaction(this)
			{
				Id = Guid.NewGuid()
			};
		}

		public IApiTransaction BeginTransaction(string name)
		{
			return new ApiTransaction(this)
			{
				Id = Guid.NewGuid(),
				Name = name
			};
		}
	}
}
