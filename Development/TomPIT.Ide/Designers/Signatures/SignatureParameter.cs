using System;

namespace TomPIT.Ide.Designers.Signatures
{
	internal class SignatureParameter : ITextSignatureParameter
	{
		public string Name { get; set; }

		public Type Type { get; set; }
	}
}
