﻿using System.Collections.Generic;
using System.Reflection;
using TomPIT.Design.Ide.Designers;

namespace TomPIT.Design.Ide.Dom
{
	public interface IDomElement : IEnvironmentObject
	{
		string Id { get; }
		string Title { get; }
		bool HasChildren { get; }
		int ChildrenCount { get; }
		string Glyph { get; }

		List<IDomElement> Items { get; }
		IDomElementBehavior Behavior { get; }
		IDomDesigner Designer { get; }
		IDomDesigner PropertyDesigner(string propertyName);

		ITransactionHandler Transaction { get; }

		object Component { get; }
		PropertyInfo Property { get; }
		object Value { get; }
		IDomElement Parent { get; }

		bool SortChildren { get; }

		void LoadChildren();
		void LoadChildren(string id);

		List<IVerb> Verbs { get; }
		IDomElementMetaData MetaData { get; }
	}
}
