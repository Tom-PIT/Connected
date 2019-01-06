using System;

namespace TomPIT.ComponentModel
{
	public enum ElementScope
	{
		Public = 1,
		Internal = 2,
		Private = 3
	}

	public interface IElement
	{
		Guid Id { get; }

		IElement Parent { get; }

		void Reset();
	}
}
