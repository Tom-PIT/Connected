/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TomPIT.Sdk.Base;
using TomPIT.Sdk.IoT.Devices;

namespace TomPIT.Sdk.IoT
{
    public class IotHub : BaseConnection
    {
        private readonly Collection<BaseIotDevice> _iotDevices = new Collection<BaseIotDevice>();

        public IotHub()
        { 

        }

        async protected override Task OnReconnected(string connectionId)
        {
            await Task.CompletedTask;
        }

        public void AddDevice(BaseIotDevice device)
        {
            _iotDevices.Add(device);
        }
    }
}
