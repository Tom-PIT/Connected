﻿namespace TomPIT.ComponentModel.Resources
{
	public interface IMediaResourceFolder : IConfigurationElement
	{
		ListItems<IMediaResourceFolder> Folders { get; }
		ListItems<IMediaResourceFile> Files { get; }

		string Name { get; }

	}
}