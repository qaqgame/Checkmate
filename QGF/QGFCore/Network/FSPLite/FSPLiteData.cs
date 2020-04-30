using ProtoBuf;
using QGF.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.FSPLite
{
    [ProtoContract]
    public class FSPParam
    {
        [ProtoMember(1)]
        public string host;
        [ProtoMember(2)]
        public int port;
        [ProtoMember(3)]
        public uint sid;//session id
        [ProtoMember(4)]
        public int serverFrameInterval = 66;//服务器帧间隔
        [ProtoMember(5)]
        public int serverTimeout = 15000;//ms
        [ProtoMember(6)]
        public int clientFrameRateMultiple = 2;//服务器与客户端的帧率倍数
        [ProtoMember(7)]
        public int authId = 0;//授权id
        [ProtoMember(8)]
        public bool useLocal = false;//是否是本地模拟
        [ProtoMember(9)]
        public int maxFrameId = 1800;//本局最高帧数（结束帧数）

        [ProtoMember(10)]
        public bool enableSpeedUp = true;
        [ProtoMember(11)]
        public int defaultSpeed = 1;
        [ProtoMember(12)]
        public int jitterBufferSize = 0;//缓冲大小
        [ProtoMember(13)]
        public bool enableAutoBuffer = true;






        public FSPParam Clone()
        {
            byte[] buffer = PBSerializer.NSerialize(this);
            return (FSPParam)PBSerializer.NDeserialize(buffer, typeof(FSPParam));
        }

        public string ToString(string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\n{0}host:{1}:{2}", prefix, host, port);
            sb.AppendFormat("\n{0}serverFrameInterval:{1}", prefix, serverFrameInterval);
            sb.AppendFormat("\n{0}clientFrameRateMultiple:{1}", prefix, clientFrameRateMultiple);
            sb.AppendFormat("\n{0}serverTimeout:{1}", prefix, serverTimeout);
            sb.AppendFormat("\n{0}maxFrameId:{1}", prefix, maxFrameId);

            return sb.ToString();
        }
    }

    //上行协议的内容
    [ProtoContract]
    public class FSPMessage
    {
        [ProtoMember(1)] public int cmd;//该消息类型
        [ProtoMember(2)] public uint playerId;//玩家id
        [ProtoMember(3)] public byte[] content;//操作内容
        [ProtoMember(4)] public int frameId;//当前帧

        public uint PlayerId
        {
            get { return playerId; }
            set { playerId = value; }
        }

        public int ClientFrameId
        {
            get { return frameId; }
            set { frameId = value; }
        }

        public override string ToString()
        {
            return string.Format("player:{0}, content:{1}, frame:{2}", playerId, content.Length, frameId);
        }

    }

    //上行协议
    [ProtoContract]
    public class FSPDataC2S
    {
        [ProtoMember(1)] public uint sid = 0;
        [ProtoMember(2)] public List<FSPMessage> msgs = new List<FSPMessage>();
    }

    //下行协议
    [ProtoContract]
    public class FSPDataS2C
    {
        [ProtoMember(1)] public List<FSPFrame> frames = new List<FSPFrame>();
    }

    //下行协议的内容
    [ProtoContract]
    public class FSPFrame
    {
        [ProtoMember(1)] public int frameId;
        [ProtoMember(2)] public List<FSPMessage> msgs = new List<FSPMessage>();


        public bool IsEmpty()
        {
            return (msgs == null || msgs.Count == 0);
        }


        //public bool Contains(int cmd)
        //{
        //    if (!IsEmpty())
        //    {
        //        for (int i = 0; i < msgs.Count; i++)
        //        {
        //            if (msgs[i].cmd == cmd)
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

        public override string ToString()
        {
            string tmp = "";
            for (int i = 0; i < msgs.Count - 1; i++)
            {
                tmp += msgs[i] + ",";
            }

            if (msgs.Count > 0)
            {
                tmp += msgs[msgs.Count - 1].ToString();
            }

            return string.Format("frameId:{0}, msgs:[{1}]", frameId, tmp);
        }

    }


}
