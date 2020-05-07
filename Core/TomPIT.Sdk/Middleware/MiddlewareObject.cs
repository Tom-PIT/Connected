using Newtonsoft.Json;
using TomPIT.Annotations;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareObject : IMiddlewareObject
	{
		private IMiddlewareContext _context = null;
		protected MiddlewareObject()
		{

		}

		protected MiddlewareObject(IMiddlewareContext context)
		{
			Context = context;
		}

		[JsonIgnore]
		[SkipValidation]
		public IMiddlewareContext Context
		{
			get
			{
				if (_context == null)
					_context = new MiddlewareContext(MiddlewareDescriptor.Current.Tenant?.Url);

				return _context;
			}
			/*
			 * this setter must be present because of reflection
			 */
			private set
			{
				_context = value;
			}
		}
	}
}
