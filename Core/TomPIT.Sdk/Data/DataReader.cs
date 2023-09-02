using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	internal class DataReader<T> : DataCommand, IDataReader<T>
	{
		public DataReader(IMiddlewareContext context) : base(context)
		{
		}

		public List<T> Query()
		{
			try
			{
				EnsureCommand();

				var result = Connection.Query<T>(Command);

				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Commit();

				return result;
			}
			finally
			{
				if (Connection.Behavior == ConnectionBehavior.Isolated)
				{
					Connection.Close();
					Connection.Dispose();
					Connection = null;
				}
			}
		}

		public T Select()
		{
			try
			{
				EnsureCommand();

				var result = Connection.Select<T>(Command);

				if (Connection.Behavior == ConnectionBehavior.Isolated)
					Connection.Commit();

				return result;
			}
			finally
			{
				if (Connection.Behavior == ConnectionBehavior.Isolated)
				{
					Connection.Close();
					Connection.Dispose();
					Connection = null;
				}
			}
		}
	}
}
