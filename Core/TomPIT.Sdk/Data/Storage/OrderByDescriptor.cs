using System.ComponentModel.DataAnnotations;

namespace TomPIT.Data.Storage;

public enum OrderByMode
{
    Ascending,
    Descending
}

public sealed class OrderByDescriptor
{
    [Required]
    [MaxLength(128)]
    public string? Property { get; set; } = null;


    public OrderByMode Mode { get; set; } = OrderByMode.Ascending;
}
