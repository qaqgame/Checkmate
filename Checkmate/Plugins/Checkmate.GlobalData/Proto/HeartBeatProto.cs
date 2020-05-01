using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
namespace Checkmate.Global.Proto
{
    [ProtoContract]
    public class HeartBeatReq
    {
        [ProtoMember(1)] public uint ping;
        [ProtoMember(2)] public uint timestamp;
    }

    [ProtoContract]
    public class HeartBeatRsp
    {
        [ProtoMember(1)] public ReturnCode ret;
        [ProtoMember(2)] public uint timestamp;
    }
}
