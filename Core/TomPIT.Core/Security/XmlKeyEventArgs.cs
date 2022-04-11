using System;

namespace TomPIT.Security
{
	public class XmlKeyEventArgs : EventArgs
	{
		public XmlKeyEventArgs(string id)
		{
			Id = id;
		}

		public string Id { get; }
	}
}
