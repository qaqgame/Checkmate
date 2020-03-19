using Checkmate.Global.Data;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Proto
{
    [ProtoContract]
    public class LoginTestProto
    {
        [ProtoMember(1)]
        public int uid;

        [ProtoMember(2)]
        public string name;

        public override string ToString()
        {
            return "id:" + uid + ", name:" + name;
        }
    }

    [ProtoContract]
    public class LoginProto
    {
        [ProtoMember(1)]
        public uint id;

        [ProtoMember(2)]
        public string name;

        public override string ToString()
        {
            return "id:" + id + ", name:" + name;
        }
    }

    [ProtoContract]
    public class LoginTestRsp
    {
        [ProtoMember(1)]
        public int ret;
        [ProtoMember(2)]
        public string msg;
    }

    [ProtoContract]
    public class LoginRsp
    {
        [ProtoMember(1)]
        public ReturnCode ret;
        [ProtoMember(2)]
        public UserData userData;
    }
}
