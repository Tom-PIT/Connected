using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Services
{
	public interface IExtender<TInput, TOutput>
	{
		List<TOutput> Extend(List<TInput> items);
	}
}
