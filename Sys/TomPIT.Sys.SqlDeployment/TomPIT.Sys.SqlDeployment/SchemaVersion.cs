namespace TomPIT.Sys.SqlDeployment;

internal class SchemaVersion
{
    public static SchemaVersion Default => new("0.0.0.0");

    private readonly string _version;

    public SchemaVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentNullException(nameof(version));

        _version = version;
    }
    public static bool operator ==(SchemaVersion first, SchemaVersion second)
    {
        return first?.Equals(second) ?? false;
    }

    public static bool operator !=(SchemaVersion first, SchemaVersion second)
    {
        return !(first?.Equals(second) ?? false);
    }
    public override string ToString()
    {
        return _version ?? Default.ToString();
    }

    public override bool Equals(object? obj)
    {
        return obj?.ToString() == ToString();
    }

    public override int GetHashCode()
    {
        return _version.GetHashCode();
    }

    public bool IsNewerThan(SchemaVersion otherVersion)
    {
        var current = _version.Split('.');
        var target = otherVersion.ToString().Split('.');

        if (current.Length < 3 || target.Length < 3)
            return false;

        var v1 = new Version(Convert.ToInt32(current[0]), Convert.ToInt32(current[1]), Convert.ToInt32(current[2]), Convert.ToInt32(current[3]));
        var v2 = new Version(Convert.ToInt32(target[0]), Convert.ToInt32(target[1]), Convert.ToInt32(target[2]), Convert.ToInt32(target[3]));

        return v1 > v2;
    }
}
