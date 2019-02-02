using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design
{
	public interface IComponentDevelopmentService
	{
		Guid Insert(Guid microService, Guid folder, string category, string name, string type);
		void Update(Guid component, string name, Guid folder);
		void Update(IConfiguration configuration);
		void Update(IText text, string content);
		void Delete(Guid component);

		string CreateName(Guid microService, string category, string prefix);

		Guid InsertFolder(Guid microService, string name, Guid parent);
		void UpdateFolder(Guid microService, Guid folder, string name, Guid parent);
		void DeleteFolder(Guid microService, Guid folder);
	}
}
