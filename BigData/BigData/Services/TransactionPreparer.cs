using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Services;

namespace TomPIT.BigData.Services
{
	internal class TransactionPreparer
	{
		public const int FileSize = 10000000;
		private const int BlockSize = 10000;

		private IMicroService _microService = null;

		public TransactionPreparer(IBigDataApi api, JArray items)
		{
			Api = api;
			Items = items;
		}

		private IBigDataApi Api { get; }
		private JArray Items { get; set; }

		private IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = Instance.GetService<IMicroServiceService>().Select(Api.MicroService(Instance.Connection));

				return _microService;
			}
		}

		public void Prepare()
		{
			var args = new ApiInvokeArguments(ExecutionContext.Create(Instance.Connection.Url, MicroService), Items);

			Instance.GetService<ICompilerService>().Execute(MicroService.Token, Api.Invoke, this, args);

			Items = args.Items;

			if (Items == null || Items.Count == 0)
			{
				Complete();
				return;
			}

			var blocks = new ListItems<JArray>();
			var blockCount = CalculateBlockCount(Items.Count);

			for (var i = 0; i < blockCount; i++)
				blocks.Add(CreateBlock(i));

			var transaction = Instance.GetService<ITransactionService>().Insert(Api.Component, blockCount);

			try
			{
				foreach (var block in blocks)
					Instance.GetService<ITransactionService>().InsertBlock(transaction, block);
			}
			catch
			{
				Instance.GetService<ITransactionService>().Delete(transaction);

				throw;
			}

			Instance.GetService<ITransactionService>().Activate(transaction);
		}

		private void Complete()
		{
			var args = new ApiInvokeArguments(ExecutionContext.Create(Instance.Connection.Url, MicroService), Items);

			Instance.GetService<ICompilerService>().Execute(MicroService.Token, Api.Complete, this, args);
		}

		private int CalculateBlockCount(int items)
		{
			var remainder = items % BlockSize;
			var blocks = items / BlockSize;

			return remainder == 0 ? blocks : blocks + 1;
		}

		private JArray CreateBlock(int index)
		{
			var r = new JArray();
			var startIndex = index == 0 ? 0 : index * BlockSize;
			var endIndex = startIndex + BlockSize;

			if (endIndex > Items.Count)
				endIndex = Items.Count;

			for (var i = startIndex; i < endIndex; i++)
				r.Add(Items[i]);

			return r;
		}

	}
}
