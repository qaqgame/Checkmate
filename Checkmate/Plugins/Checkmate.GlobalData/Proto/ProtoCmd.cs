using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Proto
{
    public class ProtoCmd
    {
        public const uint LoginReq = 1;
        public const uint LoinRes = 2;
        public const uint HeartbeatReq = 3;
        public const uint HeartBeatRsp = 4;
    }

    [ProtoContract]
    public class ReturnCode
    {
        public static ReturnCode Success = new ReturnCode();
        public static ReturnCode UnknowError = new ReturnCode(1, "UnknownError");

        public ReturnCode(int code,string info)
        {
            this.code = code;
            this.info = info;
        }

        public ReturnCode()
        {
            this.code = 0;
            this.info = "";
        }

        public override string ToString()
        {
            return string.Format("{code:{0}, info:{1}}",code,info);
        }


        [ProtoMember(1)]
        public int code = 0;
        [ProtoMember(2)]
        public string info = "";
    }
}
