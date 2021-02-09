/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using System;
using TomPIT.Sdk.HealthMonitoring;

namespace TomPIT.Sdk.IoT.Devices
{
    public abstract partial class BaseIotDevice : IEndPointHealthMonitoring
    {
        private readonly DateTime _startTime = DateTime.Now;
        private int _readPeriod = 30000; //default 30 seconds

        #region [Properties]
        protected long ElapsedSeconds => (long)((DateTime.Now - _startTime).TotalSeconds);

        protected long Id { get; private set; }
        protected string Name { get; private set; }
        #endregion

        public BaseIotDevice SetConnectionString(string connectionString)
        {
            ParseConnectionString(connectionString);
            return this;
        }

        public BaseIotDevice SetDeviceName(string name)
        {
            Name = name;
            return this;
        }

        public BaseIotDevice SetReadPeriod(int period)
        {
            _readPeriod = period;
            return this;
        }

        public BaseIotDevice SetId(long id)
        {
            Id = id;
            return this;
        }

        public void Connect()
        {
            OnBeforeConnect();

            Thread thread = new Thread(async () => await StartDataTransfer());
            thread.Start();

            OnConnect();

            OnAfterConnect();
        }

        public void Disconnect()
        {
            OnBeforeDisconnect();

            OnDisconnect();

            OnAfterDisconnect();
        }

        public virtual void OnBeforeConnect()
        {
        }

        public virtual void OnAfterConnect()
        {
        }

        public virtual void OnConnect()
        {
        }

        public virtual void OnBeforeDisconnect()
        {
        }

        public virtual void OnAfterDisconnect()
        {
        }

        public virtual void OnDisconnect()
        {
        }
    }
}
