using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Data
{
	public interface IDataWriter : IDataCommand
	{
		void Execute();
		T Execute<T>();

		IDataParameter SetReturnValueParameter(string name);
	}
}
