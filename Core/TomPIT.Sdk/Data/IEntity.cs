using System.ComponentModel;
using TomPIT.Annotations.Models;

namespace TomPIT.Data;

public enum TransactionVerb
{
    Update,
    Add,
    Delete
}

public interface IEntity
{
    [Ignore, Browsable(false)]
    TransactionVerb Verb { get; init; }
}
