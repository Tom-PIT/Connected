using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Services
{
	public class ExecutionContextState : IExecutionContextState
	{
		public const string DatabaseGet = "DatabaseGet";
		public const string DatabasePost = "DatabasePost";
		public const string Api = "Api";
		public const string Connection = "Connection";
		public const string Transaction = "Transaction";

		public int Event { get; set; }

		public string Authority { get; set; }

		public string Id { get; set; }

		public string Property { get; set; }

		public Guid MicroService { get; set; }

		public static IExecutionContextState Create(int eventId, string authority, string id, string property, Guid microService)
		{
			return new ExecutionContextState
			{
				MicroService = microService,
				Authority = authority,
				Event = eventId,
				Id = id,
				Property = property
			};
		}

	}
}
