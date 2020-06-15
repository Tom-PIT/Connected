using System;
using System.Diagnostics.CodeAnalysis;

namespace TomPIT.Data
{
	internal class ModelOperationSchema : IModelOperationSchema, IEquatable<IModelOperationSchema>
	{
		public string Text { get; set; }

		public bool Equals([AllowNull] IModelOperationSchema other)
		{
			return string.Compare(Text, other.Text, false) == 0;
		}
	}
}
