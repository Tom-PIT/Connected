using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Sdk.IoT;

namespace TomPIT.Sdk.Examples.Iot
{
    internal class Example
    {
        public void Execute()
        {
            var iotHub = new IotHub();

            iotHub.SetName("hub").SetUrl("http://").SetToken("").Create();
        }

    }
}
