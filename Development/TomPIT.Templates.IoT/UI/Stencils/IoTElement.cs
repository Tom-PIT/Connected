using TomPIT.ComponentModel;

namespace TomPIT.IoT.UI.Stencils
{
	public abstract class IoTElement : Element, IIoTElement
	{
		private ListItems<IIoTBinding> _bindings = null;

		private int _width = 100;
		private int _height = 100;
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
		public int Width
		{
			get { return _width; }
			set
			{
				if (value < 1)
					value = 1;

				_width = value;
			}
		}

		public int Height
		{
			get { return _height; }
			set
			{

				if (value < 1)
					value = 1;

				_height = value;
			}
		}

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
	}
}
