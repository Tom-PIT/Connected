using System;
using System.Collections.Generic;

namespace TomPIT.Design
{
	internal class ComponentImage : IComponentImage
	{
		private List<IComponentImageBlob> _dependencies = null;

		public Guid Token { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public Guid Folder { get; set; }
		public Guid MicroService { get; set; }
		public string Type { get; set; }
		public IComponentImageBlob Configuration { get; set; }

		public List<IComponentImageBlob> Dependencies
		{
			get
			{
				if (_dependencies == null)
					_dependencies = new List<IComponentImageBlob>();

				return _dependencies;
			}
		}

		public string NameSpace { get; set; }
	}
}
