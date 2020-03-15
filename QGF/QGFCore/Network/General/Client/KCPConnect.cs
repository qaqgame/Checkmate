using QGF.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets.Kcp;
using System.Buffers;
using QGF.Network.Utils;
using System.Threading;
using QGF.Utils;
using QGF.Network.Core;

namespace QGF.Network.General.Client
{
    
    public class KCPConnect : IConnection
    {
        //kcp发送时的处理
        public KcpSendHandler SendHandler;
        //接收时的处理函数
        public QGFEvent<byte[], int> onReceive { get; private set; }

        public bool Connected { get; private set; }

        public int id { get; private set; }

        public int bindPort { get; private set; }


        private string mIp;
        private int mPort;
        private IPEndPoint mRemoteEndPoint;//远端point

        private Socket mSysSocket;
        private Kcp mKcp;

        private Thread mThreadRecv;

        //临时存储接收数据的buffer
        private byte[] mReceiveBufferTemp = new byte[4096];

        //存储接收消息的长期buffer
        private SwitchQueue<byte[]> mRecvBufQueue = new SwitchQueue<byte[]>();

        //初始化
        public void Init(int connId, int bindPort)
        {
            this.id = connId;
            this.bindPort = bindPort;
            Debuger.Log("connId:{0}, bindPort:{1}", connId, bindPort);

            onReceive = new QGFEvent<byte[], int>();
            SendHandler = new KcpSendHandler();
            SendHandler.handler += HandleKcpSend;
        }

        public void Clear()
        {
            Debuger.Log();
            onReceive.RemoveAllListeners();
            Close();
        }

        public void Close()
        {
            Connected = false;
            //关闭kcp
            if (mKcp != null)
            {
                mKcp.Dispose();
                mKcp = null;

            }
            //关闭接收线程
            if (mThreadRecv != null)
            {
                mThreadRecv.Interrupt();
                mThreadRecv = null;
            }

            //关闭socket
            if (mSysSocket != null)
            {
                try
                {
                    mSysSocket.Shutdown(SocketShutdown.Both);
                }
                catch(Exception e)
                {
                    Debuger.LogWarning(e.Message + e.StackTrace);
                }
                mSysSocket.Close();
                mSysSocket = null;
            }
        }

        public void Connect(string ip, int port)
        {
            Debuger.Log("ip:{0}, port:{1}", ip, port);
            mIp = ip;
            mPort = port;
            mRemoteEndPoint = IPUtils.GetHostEndPoint(ip, port);

            mSysSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mSysSocket.Bind(new IPEndPoint(IPAddress.Any,bindPort));

            mKcp = new Kcp(0, SendHandler);
            mKcp.NoDelay(1, 10, 2, 1);
            mKcp.WndSize(128, 128);

            Connected = true;

            //开始接收线程
            mThreadRecv = new Thread(Thread_Recv) { IsBackground = true };
            mThreadRecv.Start();
        }



        //kcp发送时的回调函数
        private void HandleKcpSend(byte[] bytes,int len)
        {
            mSysSocket.SendTo(bytes, 0, len, SocketFlags.None, mRemoteEndPoint);
        }

        //=======================
        //接收线程函数
        //=======================
        private void Thread_Recv()
        {
            while (Connected)
            {
                try
                {
                    DoReceiveInThread();
                }
                catch(Exception e)
                {
                    Debuger.LogWarning(e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(1);
                }

            }
        }
        //线程中的接收函数
        private void DoReceiveInThread()
        {
            EndPoint remotePoint = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
            int cnt=mSysSocket.ReceiveFrom(mReceiveBufferTemp, mReceiveBufferTemp.Length, SocketFlags.None, ref remotePoint);

            if (cnt > 0)
            {
                //如果不是来自目标服务器
                if (mRemoteEndPoint.Equals(remotePoint))
                {
                    Debuger.LogError("收到非目标服务器的数据!");
                    return;
                }
                //取出数据
                byte[] dst = new byte[cnt];
                Buffer.BlockCopy(mReceiveBufferTemp, 0, dst, 0, cnt);

                mRecvBufQueue.Push(dst);

                
            }
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
                        onReceive.Invoke(buffer, len);
                    }
                }
            }
        }



        public bool Send(byte[] bytes)
        {
            if (!Connected)
            {
                return false;
            }
            return mKcp.Send(bytes) > 0;
        }

        public bool Send(byte[] bytes, int len)
        {
            if (!Connected)
            {
                return false;
            }
            return mKcp.Send(new Span<byte>(bytes,0,len)) > 0;
        }

        private DateTime mNextKcpUpdateTime=DateTime.MinValue;
        private bool mNeedKcpUpdateFlag = false;
        public void Tick()
        {
            if (Connected)
            {
                DoReceiveInMain();

                DateTime current = DateTime.UtcNow;

                if (mNeedKcpUpdateFlag||current >= mNextKcpUpdateTime)
                {
                    mKcp.Update(current);
                    mNextKcpUpdateTime = mKcp.Check(current);
                    mNeedKcpUpdateFlag = false;
                }
            }
            else
            {
                //处理重连
            }
        }
    }
}
