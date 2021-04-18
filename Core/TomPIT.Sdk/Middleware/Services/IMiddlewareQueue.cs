using System;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareQueue
	{
		void Enqueue([CIP(CIP.QueueWorkersProvider)] string queue, object arguments, string bufferKey);
		void Enqueue([CIP(CIP.QueueWorkersProvider)] string queue, object arguments, string bufferKey, TimeSpan expire, TimeSpan nextVisible);
		void Enqueue([CIP(CIP.QueueWorkersProvider)]string queue, object arguments);
		void Enqueue([CIP(CIP.QueueWorkersProvider)]string queue, object arguments, TimeSpan expire, TimeSpan nextVisible);
	}
}
