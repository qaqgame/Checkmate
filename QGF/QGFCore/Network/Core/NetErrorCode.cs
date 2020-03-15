using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.Core
{
    public enum NetErrorCode
    {
        UnkownError = -1,
        NoError = 0,
        Timeout = 1,
        Disconnected = 2,
    }
}
