using System;
using System.Collections.Generic;

namespace TomPIT.Design
{
	public class ChangeElement : IChangeElement
	{
		private List<IChangeElement> _elements = null;
		private IChangeBlob _blob = null;
		public Guid Id { get; set; }

		public string Name { get; set; }
		public ComponentVerb Verb { get; set; }
		public bool HasChanged { get; set; }
		public List<IChangeElement> Elements
		{
			get
			{
				if (_elements == null)
					_elements = new List<IChangeElement>();

				return _elements;
			}
		}

		public IChangeBlob Blob
		{
			get
			{
				if (_blob == null)
					_blob = new ChangeBlob();

				return _blob;
			}
		}
	}
}
