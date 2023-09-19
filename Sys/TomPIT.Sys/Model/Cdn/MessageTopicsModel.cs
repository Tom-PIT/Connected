using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Sys.Caching;
using TomPIT.SysDb.Messaging;

namespace TomPIT.Sys.Model.Cdn
{
	public class MessageTopicsModel : IdentityRepository<ITopic, string>
	{

		public MessageTopicsModel(IMemoryCache container) : base(container, "messagetopic")
		{
		}

		public ITopic Ensure(string name)
		{
			var r = Select(name);

			if (r is not null)
				return r;

			Insert(DataModel.ResourceGroups.Default.Token, name);

			return Select(name);
		}

		public ITopic Select(string name)
		{
			return Get(name,
				(f) =>
				{
					f.Duration = TimeSpan.Zero;

					return new Topic
					{
						Name = name,
						Id = Increment()
					};
				});
		}

		public ImmutableList<ITopic> Query()
		{
			return All();
		}

		public void Insert(Guid resourceGroup, string name)
		{
			Set(name, new Topic { Name = name, Id = Increment(), ResourceGroup = resourceGroup }, TimeSpan.Zero);
		}

		public void Delete(string name)
		{
			Remove(name);
		}
	}
}
