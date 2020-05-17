using System;
using System.Collections.Generic;

namespace TomPIT.Ide.VersionControl
{
	internal class VersionControlDescriptor : IVersionControlDescriptor
	{
		private List<IVersionControlDescriptor> _items = null;
		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Syntax { get; set; }

		public Guid Folder { get; set; }

		public Guid Blob { get; set; }
		public Guid Microservice { get; set; }
		public Guid Component { get; set; }

		public List<IVersionControlDescriptor> Items
		{
			get
			{
				if (_items == null)
					_items = new List<IVersionControlDescriptor>();

				return _items;
			}
		}
	}
}
