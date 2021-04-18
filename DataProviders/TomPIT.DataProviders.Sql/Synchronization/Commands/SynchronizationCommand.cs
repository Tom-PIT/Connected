using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal abstract class SynchronizationCommand
	{
		public SynchronizationCommand(ISynchronizer owner)
		{
			Owner = owner;
		}

		protected ISynchronizer Owner { get; }

		protected IModelSchema Model => Owner.Model;

		public string Escape(string value)
		{
			return $"[{Unescape(value)}]";
		}

		public string Unescape(string value)
		{
			return value.TrimStart('[').TrimEnd(']');
		}
		public string Escape(string schema, string name)
		{
			return $"{Escape(schema)}.{Escape(name)}";
		}
	}
}
