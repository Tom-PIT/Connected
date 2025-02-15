﻿using System;

namespace TomPIT.ComponentModel
{
	public interface IComponent
	{
		string Name { get; }
		Guid MicroService { get; }
		Guid Token { get; }
		string Type { get; }
		string Category { get; }
		DateTime Modified { get; }
		Guid Folder { get; }
		string NameSpace { get; }
	}
}
