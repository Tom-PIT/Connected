using System;
using TomPIT.Collections;

namespace TomPIT.ComponentModel.Data
{
	public interface IModelConfiguration : IConfiguration, IText
	{
		ListItems<IModelOperation> Operations { get; }
		ListItems<IModelOperation> Views { get; }

		Guid Connection { get; }
	}
}
