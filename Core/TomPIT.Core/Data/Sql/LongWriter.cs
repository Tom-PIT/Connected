namespace TomPIT.Data.Sql
{
	public class LongWriter : WriterBase<long>
	{
		public LongWriter(IDataTransaction transaction)
			: base(transaction)
		{
		}

		public LongWriter(string commandText)
			: base(commandText)
		{

		}

		public LongWriter(string commandText, IDataTransaction transaction)
			: base(commandText, transaction)
		{
		}

		public LongWriter(string commandText, System.Data.CommandType type)
			: base(commandText, type)
		{
		}

		public LongWriter(string commandText, System.Data.CommandType type, IDataTransaction transaction)
			: base(commandText, type, transaction)
		{
		}

		protected override long ParseResult(object result)
		{
			long.TryParse(result.ToString(), out long r);

			return r;
		}
	}
}
