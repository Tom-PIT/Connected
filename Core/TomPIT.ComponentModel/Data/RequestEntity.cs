using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TomPIT.Data
{
	public abstract class RequestEntity
	{
		public virtual void OnValidating(List<ValidationResult> results)
		{

		}
	}
}
