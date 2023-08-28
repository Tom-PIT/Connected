using System;

namespace TomPIT.Annotations.Models;

public class SchemaAttribute : Attribute
{
    public const string DefaultSchema = "dbo";
    public const string SchemaTypeTable = "Table";
    /*
     * sys schema is reserved for system views and tables by sql.
     */
    public const string SysSchema = "sxs";
    public const string TypesSchema = "typ";

    public const string SchemaTypeView = "View";
    public string? Id { get; set; }
    public string Name { get; set; }
    public string Schema { get; set; }
    public string Type { get; set; }
    public bool Ignore { get; set; }
}
