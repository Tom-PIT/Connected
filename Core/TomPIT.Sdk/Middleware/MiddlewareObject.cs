using Newtonsoft.Json;
using System.ComponentModel;
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
            HasOwnContext = false;
            _context = context;
        }

        private bool Disposed { get; set; }
        private bool HasOwnContext { get; set; }

        [JsonIgnore]
        [SkipValidation]
        [Browsable(false)]
        public IMiddlewareContext Context
        {
            get
            {
                if (_context is null && !Disposed)
                {
                    _context = new MiddlewareContext();

                    HasOwnContext = true;

                    OnContextChanged();
                }

                return _context;
            }
            /*
			 * this setter must be present because of reflection
			 */
            protected set
            {
                if (_context != value)
                {
                    if (_context != null && HasOwnContext)
                    {
                        _context.Dispose();
                        _context = null;
                    }

                    _context = value;

                    HasOwnContext = false;

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

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    OnDisposing();

                    if (HasOwnContext && _context is not null)
                        _context.Dispose();

                    _context = null;

                }

                Disposed = true;
            }
        }

        protected virtual void OnDisposing()
        {

        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }
    }
}
