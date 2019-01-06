using System;
using System.Collections;
using System.Collections.Generic;

namespace TomPIT.Design
{
	public interface INamingService
	{
		string Create(string prefix, IEnumerable existingItems);
		string Create(Type type, IEnumerable existingItems);
		void Create(object instance, IEnumerable existingItems);
		string Create(string name, IEnumerable<string> existingNames, bool standardCharactersOnly, int initialIndex);
		string Create(string name, IEnumerable<string> existingNames, bool standardCharactersOnly);

	}
}
