using Assets.Chess;
using Checkmate.Global.Proto;
using QGF;
using QGF.Event;
using QGF.Network.Core;
using QGF.Network.General.Client;
using QGF.Time;
using QGF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Services.Online
{
    public class HeartBeatHandler
    {
        private NetManager mNet;
        private uint mPing;
        private float mLastHeartBeatTime = 0;

        public QGFEvent onTimeout = new QGFEvent();
        public void Init(NetManager net)
        {
            mNet = net;

        }

        public void Clear()
        {
            Stop();
            mNet = null;
        }


        public void Start()
        {
            GlobalEvent.onUpdate.AddListener(OnUpdate);
        }

        public void Stop()
        {
            GlobalEvent.onUpdate.RemoveListener(OnUpdate);
        }

        private void OnUpdate()
        {
            float current=QGFTime.GetTimeSinceStartup();
            //相隔5s
            if (current - mLastHeartBeatTime > 5.0f)
            {
                mLastHeartBeatTime = current;

                HeartBeatReq req = new HeartBeatReq();
                req.ping = (ushort)mPing;
                req.timestamp = (uint)TimeUtils.GetTotalMillisecondsSince1970();
                mNet.Send<HeartBeatReq, HeartBeatRsp>(ProtoCmd.HeartbeatReq, req, OnHeartBeatRsp, 15, OnHeartBeatError);
            }
        }

        //收到心跳包回复
        private void OnHeartBeatRsp(HeartBeatRsp rsp)
        {
            //Debuger.Log();
            if (rsp.ret.code == 0)
            {
                uint current = (uint)TimeUtils.GetTotalMillisecondsSince1970();
                uint dt = current - rsp.timestamp;
                mPing = dt / 2;
                Debuger.Log("ping:{0}", mPing);
            }
        }

        private void OnHeartBeatError(int code)
        {
            //超时
            if (code == (int)NetErrorCode.Timeout)
            {
                Debuger.LogError("heartbeat timeout");
                Stop();
                onTimeout.Invoke();
            }
        }
    }
}
