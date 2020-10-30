using System;
using System.Collections.Generic;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public abstract class InboxMiddleware : MiddlewareOperation, IInboxMiddleware
	{
		private List<IInboxAddress> _addresses = null;
		public List<IInboxAddress> Addresses
		{
			get
			{
				if (_addresses == null)
					_addresses = OnCreateAddresses();

				return _addresses;
			}
		}

		protected virtual List<IInboxAddress> OnCreateAddresses()
		{
			return new List<IInboxAddress>();
		}
		public InboxMessageResult Invoke(IInboxMessage message)
		{
			try
			{
				var result = OnInvoke(message);
				Invoked();

				return result;
			}
			catch (Exception ex)
			{
				Rollback();
				throw new ScriptException(this, ex);
			}
		}

		protected virtual InboxMessageResult OnInvoke(IInboxMessage message)
		{
			return InboxMessageResult.NotImplemented;
		}
	}
}
