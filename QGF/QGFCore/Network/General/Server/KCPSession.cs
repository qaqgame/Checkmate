using QGF.Network.Core;
using QGF.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.General.Server
{
    class KCPSession:ISession
    {
        public static int SessionActiveTime = 30;//30m超时
        private uint mId;
        private uint mUserId;
        private ISessionListener mListener;
        private Action<ISession, byte[], int> mSender;

        private Kcp mKcp;

        //kcp发送时的处理
        private KcpSendHandler SendHandler;

        private SwitchQueue<byte[]> mRecvBufQueue = new SwitchQueue<byte[]>();

        public KCPSession(uint uid,Action<ISession,byte[],int> sender,ISessionListener listener)
        {
            mId = uid;
            mSender = sender;
            mListener = listener;

            SendHandler = new KcpSendHandler();
            SendHandler.handler += HandleKcpSend;


            mKcp = new Kcp(uid, SendHandler);
            mKcp.NoDelay(11, 10, 2, 1);
            mKcp.WndSize(128, 128);
        }

        public uint id { get { return mId; } }

        public uint uid { get { return mUserId; } }

        public ushort ping { get; set; }

        public IPEndPoint remoteEndPoint { get; private set; }


        public bool InAuth()
        {
            return mUserId > 0;
        }

        public void SetAuth(uint userId)
        {
            mUserId = userId;
        }

        private int mLastActiveTime = 0;
        private bool mActive = false;
        public bool IsActived()
        {
            if (!mActive)
            {
                return false;
            }
            int delta = (int)QGFTime.GetTimeSinceStartup() - mLastActiveTime;
            if (delta > SessionActiveTime)
            {
                mActive = false;
            }
            return mActive;
        }


        public void Active(EndPoint remoteEndPoint)
        {
            mLastActiveTime = (int)QGFTime.GetTimeSinceStartup();
            mActive = true;
            this.remoteEndPoint = remoteEndPoint as IPEndPoint;
        }


        public bool Send(byte[] bytes, int len)
        {
            if (IsActived())
            {
                Debuger.LogWarning("Session离线");
                return false;
            }
            return mKcp.Send(new Span<byte>(bytes, 0, len)) > 0;
        }

        public bool Send(byte[] bytes)
        {
            if (IsActived())
            {
                Debuger.LogWarning("Session离线");
                return false;
            }
            return mKcp.Send(bytes) > 0;
        }

        public void DoReceiveInGateway(byte[] buffer,int size)
        {
            //取出数据
            byte[] dst = new byte[size];
            Buffer.BlockCopy(buffer, 0, dst, 0, size);

            mRecvBufQueue.Push(dst);
        }


        private void DoReceiveInMain()
        {
            mRecvBufQueue.Switch();

            while (!mRecvBufQueue.Empty())
            {
                var recvBufferRaw = mRecvBufQueue.Pop();

                //放入kcp
                int ret = mKcp.Input(recvBufferRaw);
                //收到的包不正确时
                if (ret < 0)
                {
                    Debuger.LogError("不正确的kcp包:Ret:{0}", ret);
                    return;
                }


                mNeedKcpUpdateFlag = true;
                int len;
                //取出所有的数据并进行处理
                while ((len = mKcp.PeekSize()) > 0)
                {
                    var buffer = new byte[len];

                    if (mKcp.Recv(buffer) > 0)
                    {
                        mListener.OnReceive(this,buffer,len);
                    }
                }
            }
        }

        private DateTime mNextKcpUpdateTime = DateTime.MinValue;
        private bool mNeedKcpUpdateFlag = false;

        public void Tick(DateTime currentTime)
        {
            DoReceiveInMain();

            DateTime current = currentTime;

            if (mNeedKcpUpdateFlag || current >= mNextKcpUpdateTime)
            {
                mKcp.Update(current);
                mNextKcpUpdateTime = mKcp.Check(current);
                mNeedKcpUpdateFlag = false;
            }

        }

        private void HandleKcpSend(byte[] bytes,int len)
        {
            mSender(this, bytes, len);
        }


    }
}
