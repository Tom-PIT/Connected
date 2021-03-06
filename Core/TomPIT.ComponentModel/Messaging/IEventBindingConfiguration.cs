﻿using TomPIT.Collections;

namespace TomPIT.ComponentModel.Messaging
{
	public interface IEventBindingConfiguration : IConfiguration, INamespaceElement
	{
		ListItems<IEventBinding> Events { get; }
	}
}
