namespace TomPIT.Data.Sql
{
	public class Writer : WriterBase<int>
	{
		public Writer(IDataTransaction transaction)
			: base(transaction)
		{
		}

		public Writer(string commandText)
			: base(commandText)
		{

		}

		public Writer(string commandText, IDataTransaction transaction)
			: base(commandText, transaction)
		{
		}

		public Writer(string commandText, System.Data.CommandType type)
			: base(commandText, type)
		{
		}

		public Writer(string commandText, System.Data.CommandType type, IDataTransaction transaction)
			: base(commandText, type, transaction)
		{
		}

		protected override int ParseResult(object result)
		{
			int.TryParse(result.ToString(), out int r);

			return r;
		}
	}
}
