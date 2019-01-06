using System;
using Newtonsoft.Json.Linq;
using TomPIT.Runtime;
using TomPIT.Runtime.ApplicationContextServices;

namespace TomPIT.ComponentModel
{
	public class OperationInvokeArguments : OperationArguments
	{
		public OperationInvokeArguments(IApplicationContext sender, IApiOperation operation, JObject arguments, IApiTransaction apiTransaction) : base(sender, operation, arguments)
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
