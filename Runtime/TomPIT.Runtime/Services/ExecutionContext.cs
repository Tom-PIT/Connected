using System;

namespace TomPIT.Runtime
{
	public class ExecutionContext : IExecutionContext
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

		public static IExecutionContext Create(int eventId, string authority, string id, string property, Guid microService)
		{
			return new ExecutionContext
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
