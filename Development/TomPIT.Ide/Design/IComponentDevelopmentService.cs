using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design
{
	public interface IComponentDevelopmentService
	{
		Guid Insert(IComponent scope, Guid microService, Guid feature, string category, string name, string type);
		void Update(Guid component, string name);
		void Update(IConfiguration configuration);
		void Update(IText text, string content);
		void Delete(Guid component);

		string CreateName(Guid microService, string category, string prefix);
	}
}
