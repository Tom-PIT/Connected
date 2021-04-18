using System;
using System.Collections.Generic;

namespace TomPIT.Design
{
	public class ChangeMicroService : IChangeMicroService
	{
		private List<IChangeComponent> _components = null;
		private List<IChangeFolder> _folders = null;

		public string Name { get; set; }

		public Guid Id { get; set; }

		public List<IChangeComponent> Components
		{
			get
			{
				if (_components == null)
					_components = new List<IChangeComponent>();

				return _components;
			}
		}

		public List<IChangeFolder> Folders
		{
			get
			{
				if (_folders == null)
					_folders = new List<IChangeFolder>();

				return _folders;
			}
		}
	}
}
