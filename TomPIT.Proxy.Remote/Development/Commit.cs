﻿using System;
using TomPIT.Development;

namespace TomPIT.Proxy.Remote.Development;

internal class Commit : ICommit
{
    public DateTime Created { get; set; }
    public Guid User { get; set; }
    public string Comment { get; set; }
    public Guid Service { get; set; }
    public Guid Token { get; set; }
}
