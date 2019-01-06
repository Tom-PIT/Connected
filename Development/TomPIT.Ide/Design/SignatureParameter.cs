using System;
using TomPIT.Ide;

namespace TomPIT.Design
{
	internal class SignatureParameter : ITextSignatureParameter
	{
		public string Name { get; set; }

		public Type Type { get; set; }
	}
}
