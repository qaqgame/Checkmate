using ProtoBuf;
using QGF.Time;
using QGF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Data
{
    [ProtoContract]
    public class UserData
    {
        public static int OnlineTimeOut = 40;
        [ProtoMember(1)]
        public uint id;
        [ProtoMember(2)]
        public string name;
        [ProtoMember(3)]
        public string psw;
        [ProtoMember(4)]
        public int level;
        [ProtoMember(5)]
        public int defaultPlayerId;

        public ServerUserData svrData = new ServerUserData();

        public override string ToString()
        {
            return string.Format("<id:{0},name:{1},online:{2}>", id, name, svrData.online);
        }
    }

    public class ServerUserData
    {
        public uint sid = 0;
        public uint lastHeartBeatTime = 0;
        private bool mOnline = false;

        public bool online
        {
            get
            {
                //之前在线
                if (mOnline)
                {
                    //获取与上次心跳的间隔时间
                    uint dt =(uint)TimeUtils.GetTotalSecondsSince1970()-lastHeartBeatTime;
                    if (dt > UserData.OnlineTimeOut)
                    {
                        mOnline = false;
                    }
                }
                return mOnline;
            }

            set
            {
                mOnline = value;
            }
        }
    } 
}
