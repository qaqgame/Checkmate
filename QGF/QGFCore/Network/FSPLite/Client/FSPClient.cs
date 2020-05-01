using QGF.Codec;
using QGF.Network.Core;
using QGF.Network.Utils;
using QGF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QGF.Network.FSPLite.Client
{
    public class FSPClient
    {
        private bool mIsRunning;//是否正在运行

        private uint mSid;
        private int mAuthId;
        private uint mPlayerId;


        //通讯部分参数
        private string mIp;
        private int mPort;
        private Socket mSystemSocket;
        private Thread mThreadRecv;
        private IPEndPoint mRemoteEndPoint;//远端
        private bool mWaitForReconnect = false;//是否在等待重连
        private uint mLastRecvTimestamp = 0;//上次接收数据的时间戳

        //KCP
        private Kcp mKcp;
        private KcpSendHandler SendHandler;
        private bool mNeedKcpUpdateFlag = false;//是否需要调用kcp的update
        private DateTime mNextKcpUpdateTime = DateTime.MinValue;

        private Action<FSPFrame> mRecvListener;//接收消息的监听函数

        private byte[] mRecvBufferTemp = new byte[4086];//接收的临时缓存
        private SwitchQueue<byte[]> mRecvBufQueue = new SwitchQueue<byte[]>();//长期缓存

        //发送数据部分
        private FSPDataC2S mTempSendData = new FSPDataC2S();//发送的消息包装
        private byte[] mSendBufferTemp = new byte[4096];//发送消息临时缓存
        //====================================
        //构建部分
        public void Init(uint sid,uint playerId)
        {
            Debuger.Log("sid:{0}", sid);
            mSid = sid;
            mPlayerId = playerId;

            mTempSendData.sid = sid;
            mTempSendData.msgs.Add(new FSPMessage());

            SendHandler = new KcpSendHandler();
            SendHandler.handler += HandleKcpSend;

            //初始化kcp
            mKcp = new Kcp(2, SendHandler);
            mKcp.NoDelay(1, 10, 2, 1);
            mKcp.WndSize(128, 128);
        }

        public void Clear()
        {
            Debuger.Log();
            if (mKcp != null)
            {
                mKcp.Dispose();
                mKcp = null;
            }
            mRecvListener = null;
            Close();
        }

        //================================
        //授权
        public void SetFSPAuthInfo(int authId)
        {
            Debuger.Log(authId);
            mAuthId = authId;
        }

        //设置监听者
        public void SetFSPListener(Action<FSPFrame> listener)
        {
            Debuger.Log();
            mRecvListener = listener;

        }

        //验证授权
        public void VerifyAuth()
        {
            Debuger.Log();
            byte[] data=PBSerializer.NSerialize(mAuthId);
            SendFSP(FSPCommand.AUTH,mPlayerId,0,data);
        }


        //===============================

        public bool IsRunning { get { return mIsRunning; } }

        public bool Connect(string ip,int port)
        {
            if (mSystemSocket != null)
            {
                Debuger.LogError("已存在连接!");
                return false;
            }
            Debuger.Log("建立FSP连接：ip={0},port={1}", ip, port);
            mIp = ip;
            mPort = port;
            mLastRecvTimestamp = (uint)TimeUtils.GetTotalMillisecondsSince1970();

            try
            {
                mRemoteEndPoint = IPUtils.GetHostEndPoint(mIp, mPort);
                if (mRemoteEndPoint == null)
                {
                    Debuger.LogError("无法将Host解析为IP！");
                    Close();
                    return false;
                }
                Debuger.Log("HostEndPoint = {0}", mRemoteEndPoint.ToString());

                //创建Socket
                Debuger.Log("创建Socket, AddressFamily = {0}", mRemoteEndPoint.AddressFamily);
                mSystemSocket = new Socket(mRemoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                mSystemSocket.Bind(IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0));


                mIsRunning = true;

                mThreadRecv = new Thread(Thread_Recv) { IsBackground = true };
                mThreadRecv.Start();

            }
            catch (Exception e)
            {
                Debuger.LogError(e.Message + e.StackTrace);
                Close();
                return false;
            }

            return true;
        }

        //关闭连接
        private void Close()
        {
            Debuger.Log();

            mIsRunning = false;

            if (mThreadRecv != null)
            {
                mThreadRecv.Abort();
                mThreadRecv = null;
            }

            if (mSystemSocket != null)
            {
                try
                {
                    mSystemSocket.Shutdown(SocketShutdown.Both);

                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + e.StackTrace);
                }

                mSystemSocket.Close();
                mSystemSocket = null;
            }
        }

        //重连
        private void Reconnect()
        {
            mWaitForReconnect = false;
            Close();
            Connect(mIp, mPort);
            VerifyAuth();
        }
        //=====================================
        //接收线程函数
        private void Thread_Recv()
        {
            while (mIsRunning)
            {
                try
                {
                    DoReceiveInThread();
                }
                catch (SocketException se)
                {
                    Debuger.LogWarning("SocketErrorCode:{0}, {1}", se.SocketErrorCode, se.Message);
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(1);
                }
            }
        }

        private void DoReceiveInThread()
        {
            //将所有数据写入交换链
            EndPoint remotePoint = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
            int cnt = mSystemSocket.ReceiveFrom(mRecvBufferTemp, mRecvBufferTemp.Length, SocketFlags.None, ref remotePoint);

            if (cnt > 0)
            {
                if (!mRemoteEndPoint.Equals(remotePoint))
                {
                    Debuger.LogError("收到非目标服务器的数据！");
                    return;
                }
                

                byte[] dst = new byte[cnt];
                Buffer.BlockCopy(mRecvBufferTemp, 0, dst, 0, cnt);
                mRecvBufQueue.Push(dst);

            }
        }


        private void DoReceiveInMain()
        {
            mRecvBufQueue.Switch();
            while (!mRecvBufQueue.Empty())
            {
                var recvBufferRaw = mRecvBufQueue.Pop();
                int ret = mKcp.Input(recvBufferRaw);

                //收到的不是一个正确的KCP包
                if (ret < 0)
                {
                    Debuger.LogError("收到不正确的KCP包, Ret:{0}", ret);
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
                        mLastRecvTimestamp = (uint)TimeUtils.GetTotalMillisecondsSince1970();
                        var data = PBSerializer.NDeserialize<FSPDataS2C>(buffer);
                        if (mRecvListener != null)
                        {
                            for(int i = 0; i < data.frames.Count; ++i)
                            {
                                mRecvListener(data.frames[i]);
                            }
                        }
                    }
                }
            }
        }

        //====================================
        //kcp发送时的回调函数
        private void HandleKcpSend(byte[] bytes, int len)
        {
            mSystemSocket.SendTo(bytes, 0, len, SocketFlags.None, mRemoteEndPoint);
        }
        //发送
        public bool SendFSP(int cmd,int curFrame,byte[] content)
        {
            return SendFSP(cmd, mPlayerId, curFrame, content);
        }
        //发送
        public bool SendFSP(int cmd,uint playerId,int frame,byte[] content)
        {

            if (mIsRunning)
            {

                FSPMessage msg = mTempSendData.msgs[0];
                msg.cmd = cmd;
                msg.PlayerId = playerId;
                msg.ClientFrameId = frame;
                msg.content=content;

                mTempSendData.msgs.Clear();
                mTempSendData.msgs.Add(msg);


                int len = PBSerializer.NSerialize(mTempSendData, mSendBufferTemp);
                Debuger.Log("send fsp with sid:{0}", mTempSendData.sid);
                return len>0&&mKcp.Send(mSendBufferTemp) >= 0;
            }

            return false;
        }


        //=========================
        //检查超时
        private void CheckTimeout()
        {
            uint current = (uint)TimeUtils.GetTotalMillisecondsSince1970();
            var dt = current - mLastRecvTimestamp;
            if (dt > 5000)
            {
                mWaitForReconnect = true;
            }
        }

        //外部调用
        public void Tick()
        {
            if (!mIsRunning)
            {
                return;
            }

            DoReceiveInMain();

            DateTime current = DateTime.UtcNow;
            if (mNeedKcpUpdateFlag || current >= mNextKcpUpdateTime)
            {
                if (mKcp != null)
                {
                    mKcp.Update(current);
                    mNextKcpUpdateTime = mKcp.Check(current);
                    mNeedKcpUpdateFlag = false;

                }
            }


            if (mWaitForReconnect)
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    Reconnect();
                }
                else
                {
                    Debuger.Log("等待重连，但是网络不可用！");
                }
            }


            CheckTimeout();
        }
    }
}
