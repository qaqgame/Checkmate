using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Proto
{
    [ProtoContract]
    public class LoginProto
    {
        [ProtoMember(1)]
        public int id;

        [ProtoMember(2)]
        public string name;
    }
    [ProtoContract]
    public class LoginRsp
    {
        [ProtoMember(1)]
        public int ret;
        [ProtoMember(2)]
        public string msg;
    }
}
