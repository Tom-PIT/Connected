using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.DataProviders.BigData;
using Xunit;

namespace TomPIT.Core.Tests.DataProviders
{
	public class BigDataBatchingTests
	{
		private const string ConnectionString = "http://bigdata.test";

		[Fact]
		public void Execute_SamePartition_ProducesSingleBatchInExecutionOrder()
		{
			var provider = new BigDataProvider();
			using var connection = provider.OpenConnection(null, ConnectionString, ConnectionBehavior.Shared);

			for (var i = 1; i <= 3; i++)
				provider.Execute(null, new Descriptor("ms/a", i), connection);

			var batches = Transaction(connection).CreateBatches();

			var batch = Assert.Single(batches);
			Assert.Equal("ms/a", batch.CommandText);
			Assert.Equal(new[] { 1, 2, 3 }, Values(batch));
		}

		[Fact]
		public void Execute_SameDescriptor_ProducesSingleBatch()
		{
			var provider = new BigDataProvider();
			using var connection = provider.OpenConnection(null, ConnectionString, ConnectionBehavior.Shared);
			var descriptor = new Descriptor("ms/a", 0);

			for (var i = 1; i <= 3; i++)
			{
				descriptor.Parameters[0].Value = i;
				provider.Execute(null, descriptor, connection);
			}

			var batch = Assert.Single(Transaction(connection).CreateBatches());
			Assert.Equal(new[] { 1, 2, 3 }, Values(batch));
		}

		[Fact]
		public void Execute_MultiplePartitions_ProducesBatchPerPartitionInOrder()
		{
			var provider = new BigDataProvider();
			using var connection = provider.OpenConnection(null, ConnectionString, ConnectionBehavior.Shared);

			provider.Execute(null, new Descriptor("ms/a", 1), connection);
			provider.Execute(null, new Descriptor("ms/b", 2), connection);
			provider.Execute(null, new Descriptor("ms/a", 3), connection);

			var batches = Transaction(connection).CreateBatches();

			Assert.Equal(new[] { "ms/a", "ms/b" }, batches.Select(f => f.CommandText));
			Assert.Equal(new[] { 1, 3 }, Values(batches[0]));
			Assert.Equal(new[] { 2 }, Values(batches[1]));
		}

		[Fact]
		public void CreateBatches_CalledAgain_DoesNotDuplicateRows()
		{
			var provider = new BigDataProvider();
			using var connection = provider.OpenConnection(null, ConnectionString, ConnectionBehavior.Shared);

			provider.Execute(null, new Descriptor("ms/a", 1), connection);
			provider.Execute(null, new Descriptor("ms/a", 2), connection);

			var transaction = Transaction(connection);

			transaction.CreateBatches();

			var batch = Assert.Single(transaction.CreateBatches());
			Assert.Equal(new[] { 1, 2 }, Values(batch));
		}

		[Fact]
		public void Rollback_DiscardsAllRows()
		{
			var provider = new BigDataProvider();
			using var connection = provider.OpenConnection(null, ConnectionString, ConnectionBehavior.Shared);

			provider.Execute(null, new Descriptor("ms/a", 1), connection);
			provider.Execute(null, new Descriptor("ms/b", 2), connection);

			var transaction = Transaction(connection);

			transaction.Rollback();

			Assert.All(transaction.CreateBatches(), f => Assert.Equal(0, f.RowCount));
		}

		[Fact]
		public void Execute_Concurrently_KeepsAllRows()
		{
			const int count = 2000;
			var provider = new BigDataProvider();
			using var connection = provider.OpenConnection(null, ConnectionString, ConnectionBehavior.Shared);

			connection.Open();

			Parallel.For(0, count, i => provider.Execute(null, new Descriptor("ms/a", i), connection));

			var batch = Assert.Single(Transaction(connection).CreateBatches());
			Assert.Equal(Enumerable.Range(0, count), Values(batch).OrderBy(f => f));
		}

		private static BigDataTransaction Transaction(IDataConnection connection)
		{
			return Assert.IsType<BigDataTransaction>(connection.Transaction);
		}

		private static int[] Values(BigDataCommand command)
		{
			return command.Rows.Select(f => (int)f["value"]).ToArray();
		}

		private sealed class Descriptor : IDataCommandDescriptor
		{
			public Descriptor(string commandText, int value)
			{
				CommandText = commandText;
				Parameters = new List<ICommandParameter> { new Parameter("value", value) };
			}

			public int CommandTimeout => 30;
			public string ConnectionString => null;
			public string CommandText { get; }
			public CommandType CommandType => CommandType.Text;
			public List<ICommandParameter> Parameters { get; }
		}

		private sealed class Parameter : ICommandParameter
		{
			public Parameter(string name, object value)
			{
				Name = name;
				Value = value;
			}

			public string Name { get; }
			public DbType DataType => DbType.Int32;
			public object Value { get; set; }
			public ParameterDirection Direction => ParameterDirection.Input;
		}
	}
}
