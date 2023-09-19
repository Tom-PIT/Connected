namespace TomPIT.Middleware;
public interface ICallerContext
{
    string? Component { get; }

    string? Method { get; }
}
