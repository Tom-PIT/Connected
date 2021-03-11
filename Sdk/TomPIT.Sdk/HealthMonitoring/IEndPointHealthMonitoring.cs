/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

namespace TomPIT.Sdk.HealthMonitoring
{
    public interface IEndPointHealthMonitoring
    {
        bool Enabled { get; }

        string EndpointKey { get; }

        double Health { get; }

        void ConfigureHealthMonitoring(bool enabled, string endpointKey);
    }
}
