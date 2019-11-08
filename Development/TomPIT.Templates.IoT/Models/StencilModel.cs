using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.IoT;
using TomPIT.MicroServices.IoT.UI.Stencils;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.IoT.Models
{
	public abstract class StencilModel : MiddlewareObject
	{
		private List<IIoTFieldState> _ds = null;
		private IIoTHubConfiguration _hub = null;
		private IIoTViewConfiguration _view = null;

		public StencilModel(IMiddlewareContext context, IIoTElement element) : base(context)
		{
			Element = element;
		}

		public IIoTElement Element { get; }

		public bool IsDesignTime { get { return (Shell.GetService<IRuntimeService>().Mode & EnvironmentMode.Design) == EnvironmentMode.Design; } }

		public IIoTViewConfiguration View
		{
			get
			{
				if (_view == null)
					_view = Context.Tenant.GetService<IComponentService>().SelectConfiguration(Element.Configuration().Component) as IIoTViewConfiguration;

				return _view;
			}
		}

		public IIoTHubConfiguration Hub
		{
			get
			{
				if (_hub == null)
					_hub = View.ResolveHub(Context);

				return _hub;
			}
		}

		public List<IIoTFieldState> DataSource
		{
			get
			{
				if (_ds == null && Hub != null)
				{
					var ds = Context.Tenant.GetService<IIoTService>().SelectState(Hub.Component);

					if (Context is IDataForwardingProvider fw && fw.ForwardState != null && fw.ForwardState.Count > 0)
					{
						_ds = new List<IIoTFieldState>();

						foreach (var i in ds)
						{
							var forward = fw.ForwardState.FirstOrDefault(f => string.Compare(f.Field, i.Field, true) == 0);
							var value = i.Value;

							if (forward != null && forward.Modified > i.Modified)
								value = forward.Value;

							_ds.Add(new IoTFieldState
							{
								Field = i.Field,
								Modified = i.Modified,
								Value = value
							});
						}
					}
					else
						_ds = ds;
				}

				return _ds;
			}
		}
	}
}
