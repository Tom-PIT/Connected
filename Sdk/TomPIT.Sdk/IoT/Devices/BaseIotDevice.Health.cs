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
        private string _healthMonitorEndpointKey = string.Empty;
        private bool _healthMonitoringEnabled = false;

        #region [Explicit Interface Implementations]
        string IEndPointHealthMonitoring.EndpointKey => _healthMonitorEndpointKey;

        bool IEndPointHealthMonitoring.Enabled => (!string.IsNullOrWhiteSpace(_healthMonitorEndpointKey) && _healthMonitoringEnabled);

        double IEndPointHealthMonitoring.Health { get => GetHealth(); }
        #endregion

        #region [Implicit Interface Implementations]
        public void ConfigureHealthMonitoring(bool enabled, string endpointKey)
        {
            _healthMonitoringEnabled = enabled;
            _healthMonitorEndpointKey = endpointKey;
        }
        #endregion

        public virtual double GetHealth()
        {
            return 100.0;
        }
    }
}
