using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.IoT.UI.Stencils;
using TomPIT.Services;

namespace TomPIT.IoT.Models
{
	public abstract class StencilModel
	{
		private List<IIoTFieldState> _ds = null;
		private IIoTHub _hub = null;
		private IIoTView _view = null;

		public StencilModel(IExecutionContext context, IIoTElement element)
		{
			Context = context;
			Element = element;
		}

		public IIoTElement Element { get; }
		public IExecutionContext Context { get; }

		public bool IsDesignTime { get { return (Shell.GetService<IRuntimeService>().Mode & EnvironmentMode.Design) == EnvironmentMode.Design; } }

		public IIoTView View
		{
			get
			{
				if (_view == null)
					_view = Context.Connection().GetService<IComponentService>().SelectConfiguration(Element.Configuration().Component) as IIoTView;

				return _view;
			}
		}

		public IIoTHub Hub
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
					var ds = Context.Connection().GetService<IIoTService>().SelectState(Hub.Component);

					if (Context is IForwardDataProvider fw && fw.ForwardState != null && fw.ForwardState.Count > 0)
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
