using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Security
{
    public interface IXmlKeyNotification
    {
        void NotifyChanged(object sender, XmlKeyEventArgs e);
    }
}
