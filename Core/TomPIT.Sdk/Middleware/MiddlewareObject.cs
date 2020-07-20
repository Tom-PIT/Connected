using Newtonsoft.Json;
using TomPIT.Annotations;
using TomPIT.Reflection;

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
				{
					_context = new MiddlewareContext(MiddlewareDescriptor.Current.Tenant?.Url);

					OnContextChanged();
				}

				return _context;
			}
			/*
			 * this setter must be present because of reflection
			 */
			private set
			{
				if (_context != value)
				{
					_context = value;

					OnContextChanged();
				}
			}
		}

		protected virtual void OnContextChanged()
		{

		}

		public override string ToString()
		{
			return GetType().ShortName();
		}
	}
}
