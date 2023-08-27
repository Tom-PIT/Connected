using Microsoft.CodeAnalysis;
using System;

namespace TomPIT.Design;
public interface IDocumentContext : IDisposable
{
    Document Document { get; }
}
