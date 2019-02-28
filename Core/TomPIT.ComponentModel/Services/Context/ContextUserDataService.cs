using System.Collections.Generic;
using System.Globalization;
using TomPIT.Data;
using TomPIT.Security;

namespace TomPIT.Services.Context
{
	internal class ContextUserDataService : ContextClient, IContextUserDataService
	{
		public ContextUserDataService(IExecutionContext context) : base(context)
		{
		}

		public IUserData Create(string primaryKey, object value)
		{
			return Create(primaryKey, value, null);
		}

		public IUserData Create(string primaryKey, object value, string topic)
		{
			return new UserData
			{
				PrimaryKey = primaryKey,
				Value = Types.Convert<string>(value, CultureInfo.InvariantCulture),
				Topic = topic
			};
		}

		public List<IUserData> Query(string topic)
		{
			return Context.Connection().GetService<IUserDataService>().Query(Shell.HttpContext.CurrentUserToken(), topic);
		}

		public T Select<T>(string primaryKey, string topic)
		{
			var r = Select(primaryKey, topic);

			if (string.IsNullOrWhiteSpace(r))
				return default(T);

			return Types.Convert<T>(r, CultureInfo.InvariantCulture);
		}

		public T Select<T>(string primaryKey)
		{
			var r = Select(primaryKey);

			if (string.IsNullOrWhiteSpace(r))
				return default(T);

			return Types.Convert<T>(r, CultureInfo.InvariantCulture);
		}

		public string Select(string primaryKey)
		{
			return Context.Connection().GetService<IUserDataService>().Select(Shell.HttpContext.CurrentUserToken(), primaryKey)?.Value;
		}

		public string Select(string primaryKey, string topic)
		{
			return Context.Connection().GetService<IUserDataService>().Select(Shell.HttpContext.CurrentUserToken(), primaryKey, topic)?.Value;
		}

		public void Update(string primaryKey, object value)
		{
			Update(primaryKey, value, null);
		}

		public void Update(string primaryKey, object value, string topic)
		{
			Update(new List<IUserData>
			{
				new UserData
				{
					PrimaryKey=primaryKey,
					Value=Types.Convert<string>(value, CultureInfo.InvariantCulture),
					Topic=topic
				}
			});
		}

		public void Update(List<IUserData> data)
		{
			Context.Connection().GetService<IUserDataService>().Update(Shell.HttpContext.CurrentUserToken(), data);
		}
	}
}
