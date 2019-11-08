using System;

namespace TomPIT.Ide.Designers.Signatures
{
	public interface ITextSignatureParameter
	{
		string Name { get; }
		Type Type { get; }
	}
}
