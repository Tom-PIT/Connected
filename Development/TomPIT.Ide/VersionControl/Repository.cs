﻿using TomPIT.Development;

namespace TomPIT.Ide.VersionControl
{
	internal class Repository : IRepository
	{
		public string Name { get; set; }

		public string Url { get; set; }

		public string UserName { get; set; }

		public byte[] Password { get; set; }
	}
}