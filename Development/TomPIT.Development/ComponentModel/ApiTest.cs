using System;

namespace TomPIT.ComponentModel
{
	internal class ApiTest : IApiTest
	{
		public string Api { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Tags { get; set; }
		public Guid Identifier { get; set; }
	}
}
