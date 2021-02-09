/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Sdk.HealthMonitoring;

namespace TomPIT.Sdk.IoT.Devices
{
    public abstract partial class BaseIotDevice : IEndPointHealthMonitoring
    {
        private readonly System.Timers.Timer _intervalTimer;

        //public delegate void Queue(object sender, IotDataEventArgs e);

        private bool _threadActive = false;
        private readonly object _lockObject = new object();

        //public event Queue QueueDataForSending;

        private void ReadDeviceData()
        {
            if (!ClientConnected)
            {
                return;
            }

            _intervalTimer.Stop();

            OnBeforeAcquire();

            if (!IsClientReady())
            {
                return;
            }

            var data = Acquire();

            OnAfterAcquire();

            _intervalTimer.Start();

            if (data is null)
                return;

            foreach (var obj in data)
                PrepareForSending(obj);
        }

        private void PrepareForSending(JObject data)
        {
            lock (_lockObject)
            {
                //var args = new IotDataEventArgs();
                //args.Data = JObject.FromObject(data);

                //QueueDataForSending?.Invoke(this, args);
            }
        }

        private async Task StartDataTransfer()
        {
            _intervalTimer.Start();
            _threadActive = true;
        }

        private void StopDataTransfer()
        {
            _intervalTimer.Stop();
            _threadActive = false;
        }

        public virtual List<JObject> Acquire()
        {
            return null;
        }

        public virtual void OnBeforeAcquire()
        {
        }

        public virtual void OnAfterAcquire()
        {
        }

        public virtual bool IsClientReady()
        {
            return true;
        }
    }
}
