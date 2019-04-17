using System;

namespace TomPIT.ComponentModel
{
    public enum MicroServiceStatus
    {
        Development = 1,
        Staging = 2,
        Production = 3
    }

    public enum UpdateStatus
    {
        UpToDate = 0,
        UpdateAvailable = 1
    }

    public enum CommitStatus
    {
        Synchronized = 0,
        Invalidated = 1,
        Publishing = 2,
        PublishError = 3
    }

    public interface IMicroService
    {
        string Name { get; }
        string Url { get; }
        Guid Token { get; }
        MicroServiceStatus Status { get; }
        Guid ResourceGroup { get; }
        Guid Template { get; }
        Guid Package { get; }
        UpdateStatus UpdateStatus { get; }
        CommitStatus CommitStatus { get; }
        string Version { get; }
    }
}
