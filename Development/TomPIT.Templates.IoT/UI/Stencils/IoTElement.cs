using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.IoT.UI.Stencils
{
	public abstract class IoTElement : Element, IIoTElement
	{
		private ListItems<IIoTBinding> _bindings = null;

		private int _left = 0;
		private int _top = 0;

		public string Name { get; set; }

		public int Left
		{
			get { return _left; }
			set
			{
				if (value < 0)
					value = 0;

				_left = value;
			}
		}
		public int Top
		{
			get { return _top; }
			set
			{
				if (value < 0)
					value = 0;

				_top = value;
			}
		}
		public int Width { get; set; }
		public int Height { get; set; }

		public string Css { get; set; }

		public ListItems<IIoTBinding> Bindings
		{
			get
			{
				if (_bindings == null)
					_bindings = new ListItems<IIoTBinding> { Parent = this };

				return _bindings;
			}
		}

		public List<IIoTBinding> QueryBindings()
		{
			var r = new List<IIoTBinding>();

			foreach (var i in Bindings)
				r.Add(i);

			OnQueryBindings(r);

			return r;
		}

		protected virtual void OnQueryBindings(List<IIoTBinding> items)
		{

		}
	}
}
