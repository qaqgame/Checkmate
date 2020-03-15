using QGF.Common;
using QGF.Network.Core;
using QGF.Network.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QGF.Network.General.Server
{
    class Gateway
    {
        private MapList<uint, ISession> mSessions;

        private Socket mSysSocket;
        private bool mIsRunning=false;
        private Thread mThreadRecv;//接收线程

        private byte[] mRecvBufferTemp = new byte[4096];
        private ISessionListener mListener;//该listener派发消息至上一层

        private int mPort;

        public void Init(int port,ISessionListener listener)
        {
            Debuger.Log("port:{0}", port);
            mPort = port;
            mListener = listener;
            mSessions = new MapList<uint, ISession>();

            //启动
            Start();
        }

        public void Clear()
        {
            Debuger.Log();
            mSessions.Clear();
            Close();
        }

        private void Start()
        {
            Debuger.Log();
            mIsRunning = true;

            mSysSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mSysSocket.Bind(IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork,mPort));

            mThreadRecv = new Thread(Thread_Recv) { IsBackground = true };
            mThreadRecv.Start();
        }

        private void Close()
        {
            Debuger.Log();
            mIsRunning = false;
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
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + e.StackTrace);
                }
                mSysSocket.Close();
                mSysSocket = null;
            }
        }

        public ISession GetSeeion(uint sid)
        {
            ISession session = null;
            lock (mSessions)
            {
                session = mSessions[sid];
            }
            return session;
        }

        //=======================
        //接收线程函数
        //=======================
        private void Thread_Recv()
        {
            while (mIsRunning)
            {
                try
                {
                    DoReceiveInThread();
                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(1);
                }

            }
        }
        private NetBufferReader mRecvBufferReader = new NetBufferReader();
        private void DoReceiveInThread()
        {
            EndPoint remotePoint = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
            int cnt = mSysSocket.ReceiveFrom(mRecvBufferTemp, mRecvBufferTemp.Length, SocketFlags.None, ref remotePoint);

            if (cnt > 0)
            {
                mRecvBufferReader.Attach(mRecvBufferTemp, cnt);
                uint sid= mRecvBufferReader.ReadUInt();//读取session id

                lock (mSessions)
                {
                    ISession session = null;
                    if (sid == 0)
                    {
                        //第一个包
                        sid = SessionID.NewID();
                        session = new KCPSession(sid, HandleSessionSend, mListener);
                        mSessions.Add(session.id, session);
                    }
                    else
                    {
                        session = mSessions[sid];
                    }

                    //如果有读取到session
                    if (session != null)
                    {
                        session.Active(remotePoint as IPEndPoint);
                        session.DoReceiveInGateway(mRecvBufferTemp, cnt);
                    }
                    else
                    {
                        Debuger.LogWarning("无效的包! sid:{0}", sid);
                    }
                }


            }
        }

        private void HandleSessionSend(ISession session, byte[] bytes, int len)
        {
            if (mSysSocket != null)
            {
                mSysSocket.SendTo(bytes, 0, len, SocketFlags.None, session.remoteEndPoint);
            }
            else
            {
                Debuger.LogError("Socket已经关闭");
            } 
        }

        private DateTime mLastClearSessionTime = DateTime.MinValue;
        
        public void Tick()
        {
            if (mIsRunning)
            {
                lock (mSessions)
                {
                    DateTime current = DateTime.UtcNow;

                    if ((current - mLastClearSessionTime).TotalSeconds > KCPSession.SessionActiveTime)
                    {
                        mLastClearSessionTime = current;
                        ClearNoActiveSession();
                    }                  

                    var list = mSessions.AsList();
                    int cnt = list.Count;
                    for(int i = 0; i < cnt; ++i)
                    {
                        (list[i]).Tick(current);
                    }
                }
            }
        }


        private void ClearNoActiveSession()
        {
            lock (mSessions)
            {
                var list = mSessions.AsList();
                var dic = mSessions.AsDictionary();
                int cnt = list.Count;
                for(int i = cnt - 1; i >= 0; --i)
                {
                    var session = list[i];
                    if (!session.IsActived())
                    {
                        list.RemoveAt(i);
                        dic.Remove(session.id);
                    }
                }
            }
        }
    }
}
