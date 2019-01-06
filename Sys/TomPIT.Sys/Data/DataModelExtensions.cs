using TomPIT.Api.Net;
using TomPIT.SysDb.Environment;

namespace TomPIT.Sys.Data
{
	public static class DataModelExtensions
	{
		public static IServerResourceGroup ResolveResourceGroup(this ITopic topic)
		{
			var rg = DataModel.ResourceGroups.Select(topic.ResourceGroup);

			if (rg == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			return rg;
		}

		public static IServerResourceGroup ResolveResourceGroup(this IMessage message)
		{
			var t = DataModel.MessageTopics.Select(message.Topic);

			if (t == null)
				throw new SysException(SR.ErrTopicNotFound);

			var rg = DataModel.ResourceGroups.Select(t.ResourceGroup);

			if (rg == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			return rg;
		}

		public static IServerResourceGroup ResolveResourceGroup(this ISubscriber subscriber)
		{
			var t = DataModel.MessageTopics.Select(subscriber.Topic);

			if (t == null)
				throw new SysException(SR.ErrTopicNotFound);

			var rg = DataModel.ResourceGroups.Select(t.ResourceGroup);

			if (rg == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			return rg;
		}
	}
}
