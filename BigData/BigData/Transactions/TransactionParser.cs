using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.BigData.Transactions
{
	internal class TransactionParser
	{
		private List<List<object>> _blocks = null;
		public const int FileSize = 10000000;
		private const int BlockSize = 10000;

		private IMicroService _microService = null;

		public TransactionParser(IPartitionConfiguration partition, JArray items)
		{
			Partition = partition;
			Items = items;
		}

		public IPartitionConfiguration Partition { get; }
		private JArray Items { get; set; }
		public int BlockCount { get; private set; }

		public List<List<object>> Blocks
		{
			get
			{
				if (_blocks == null)
					_blocks = new List<List<object>>();

				return _blocks;
			}
		}
		public IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)Partition).MicroService());

				return _microService;
			}
		}

		public void Execute()
		{
			var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(MicroService.Token, Partition, Partition.ComponentName());
			var argument = type.GetInterface(typeof(IPartitionMiddleware<>).FullName).GetGenericArguments()[0];
			//TODO: not implemented
			dynamic instance = null;// MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance< type.CreateInstance();
			var items = (IList)typeof(List<>).MakeGenericType(argument).CreateInstance();

			foreach (var item in Items)
			{
				var itemInstance = argument.CreateInstance();

				Serializer.Populate(item, itemInstance);

				items.Add(itemInstance);
			}

			var methods = instance.GetType().GetMethods();
			MethodInfo target = null;

			foreach (MethodInfo method in methods)
			{
				if (string.Compare(method.Name, "Invoke", false) != 0)
					continue;

				var parameters = method.GetParameters();

				if (parameters.Length != 1)
					continue;

				if (parameters[0].ParameterType == items.GetType())
				{
					target = method;
					break;
				}
			}

			items = target.Invoke(instance, new object[] { items });

			if (items == null || items.Count == 0)
				return;

			BlockCount = CalculateBlockCount(items.Count);

			for (var i = 0; i < BlockCount; i++)
				Blocks.Add(CreateBlock(i));
		}

		private int CalculateBlockCount(int items)
		{
			var remainder = items % BlockSize;
			var blocks = items / BlockSize;

			return remainder == 0 ? blocks : blocks + 1;
		}

		private List<object> CreateBlock(int index)
		{
			var result = new List<object>();
			var startIndex = index == 0 ? 0 : index * BlockSize;
			var endIndex = startIndex + BlockSize;

			if (endIndex > Items.Count)
				endIndex = Items.Count;

			for (var i = startIndex; i < endIndex; i++)
				result.Add(Items[i]);

			return result;
		}
	}
}
