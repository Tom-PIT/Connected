using System;

namespace TomPIT.Design
{
	internal class ChangeComponent : ChangeElement, IChangeComponent
	{
		public Guid Folder { get; set; }

		public Guid Microservice { get; set; }
		public byte[] Configuration { get; set; }
		public byte[] RuntimeConfiguration { get; set; }
		public string Error { get; set; }
	}
}
