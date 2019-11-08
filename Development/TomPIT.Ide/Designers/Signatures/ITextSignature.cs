using System;
using System.Collections.Generic;

namespace TomPIT.Ide.Designers.Signatures
{
	public interface ITextSignature
	{
		Type ReturnValue { get; }
		string Name { get; }
		List<ITextSignatureParameter> Parameters { get; }
		string Text { get; }
		string Language { get; }
	}
}
