/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using TomPIT.Sdk.HealthMonitoring;

namespace TomPIT.Sdk.IoT.Devices
{
    public abstract partial class BaseIotDevice : IEndPointHealthMonitoring
    {
        public abstract bool ClientConnected { get; }
    }
}
