using System;
using System.Collections.Generic;

namespace TomPIT.Design
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
