namespace TomPIT.Data;
public interface IPrimaryKey<T> where T : notnull
{
    T Id { get; init; }
}
