using System;

namespace TomPIT.Annotations.Design
{
	public sealed class FileNameExtensionAttribute : Attribute
	{
		public FileNameExtensionAttribute(string extension)
		{
			Extension = extension;
		}

		public string Extension { get; }
	}
}
