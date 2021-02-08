using System;

namespace TomPIT.Cdn.Documents
{
	public interface IDocumentProvider
	{
		string Name { get; }
		void Print(IPrintJob job);
		IDocumentDescriptor Create(IPrintJob job);
		IDocumentDescriptor Create(Guid report, DocumentCreateArgs e);
	}
}
