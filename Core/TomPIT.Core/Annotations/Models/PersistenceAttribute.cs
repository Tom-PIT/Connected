using System;

namespace TomPIT.Annotations.Models;

[Flags]
public enum ColumnPersistence
{
    InMemory = 0,
    Read = 1,
    Write = 2,
    ReadWrite = 3
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public sealed class PersistenceAttribute : Attribute
{
    public ColumnPersistence Persistence { get; set; }

    public bool IsReadOnly => (Persistence & ColumnPersistence.Read) == ColumnPersistence.Read;
    public bool IsWriteOnly => (Persistence & ColumnPersistence.Write) == ColumnPersistence.Write;
    public bool IsReadWrite => (Persistence & ColumnPersistence.ReadWrite) == ColumnPersistence.ReadWrite;
    public bool IsVirtual => Persistence == ColumnPersistence.InMemory;
}
