using QGF.Codec;
using QGF.Common;
using QGF.Network.Core;
using QGF.Network.Core.RPCLite;
using QGF.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.General.Client
{
    public class NetManager
    {

        private IConnection mConn;
        private uint mUid;//user id
        private RPCManagerBase mRPC;

        


        //连接类型-连接id-连接用的本地端口
        public void Init(Type connType,int connId,int bindPort)
        {
            Debuger.Log("connType:{0}, connId:{1},bindPort:{2}", connType, connId, bindPort);
            mConn = Activator.CreateInstance(connType) as IConnection;
            mConn.Init(connId, bindPort);
            mRPC = new RPCManagerBase();
            mRPC.Init();
        }


        public void Clear()
        {
            Debuger.Log();
            if (mConn != null)
            {
                mConn.Clear();
                mConn = null;
            }
        }

        public void SetUserId(uint uid)
        {
            mUid = uid;
        }

        public void Connect(string ip,int port)
        {
            Debuger.Log("ip:{0},port:{1}",ip, port);

            if (mConn.Connected)
            {
                Debuger.Log("仍存在连接，关闭后继续");
                mConn.Close();
            }

            mConn.Connect(ip, port);
            mConn.onReceive.AddListener(OnReceive);
        }

        public void Close()
        {
            Debuger.Log();
            mConn.Close();
        }

        public void Update()
        {
            mConn.Tick();
            CheckTimeout();
        }

        //处理接收消息
        private void OnReceive(byte[]bytes,int len)
        {
            NetMessage msg = new NetMessage();
            msg.Deserialize(bytes, len);

            //通过RPC
            if (msg.head.cmd == 0)
            {
                RPCMessage rpcmsg = PBSerializer.NDeserialize<RPCMessage>(msg.content);
                HandleRPCMessage(rpcmsg);
            
            }
            //通过传统方式(protobuf)
            else
            {
                HandlePBMessage(msg);
            }
        }


        //============================
        //处理RPC消息
        //================================
        #region RPCHandler
        private string mCurInvokeName;//当前RPC调用的函数

        //注册RPC函数反射
        public void RegistRPCListener(object listener)
        {
            mRPC.RegistListner(listener);
        }
        public void UnRegistRPCListener(object listener)
        {
            mRPC.UnRegistListner(listener);
        }
        public void HandleRPCMessage(RPCMessage msg)
        {
            Debuger.Log("Connection[{0}]->{1}<>", mConn.id, msg.name);
            Debuger.Log("args count:{0}",msg.args.Length);
            foreach( RPCRawArg arg in msg.raw_args)
            {
                Debuger.Log("type:{0},value:{1}",arg.type,arg.value.ToString());
            }            
            //获取方法
            var helper = mRPC.GetMethodHelper(msg.name);
            if (helper != null)
            {
                object[] args = msg.args;

                var raw_args = msg.raw_args;

                var paramInfos=helper.method.GetParameters();
                //完成剩下的反序列化
                if (raw_args.Count == paramInfos.Length)
                {
                    for (int i = 0; i < raw_args.Count; ++i)
                    {
                        //对于类型为PB对象的参数，找到该参数的具体类型，并进行转换
                        if (raw_args[i].type == RPCArgType.PBObject)
                        {
                            var type = paramInfos[i].ParameterType;
                            object arg = PBSerializer.NDeserialize(raw_args[i].raw_value, type);
                            args[i] = arg;
                        }
                    }

                    mCurInvokeName = msg.name;
                    try
                    {
                        helper.method.Invoke(helper.listener, BindingFlags.NonPublic, null, args, null);
                    }
                    catch(Exception e)
                    {
                        Debuger.LogError("RPC调用出错:{0}\n{1}", e.Message, e.StackTrace);
                    }
                    mCurInvokeName = null;
                }
                else
                {
                    Debuger.LogWarning("{0}函数参数不一致!", helper.method.Name);
                }
            }
            else
            {
                Debuger.LogError("RPC 不存在:{0}", msg.name);
            }

        }

        //调用RPC的发送
        public void Invoke(string name,params object[] args)
        {
            Debuger.Log("->Connection[{0}]  {1}<{2}>", mConn.id, name, args);

            RPCMessage msg = new RPCMessage();
            msg.name = name;
            msg.args = args;

            byte[] buffer = PBSerializer.NSerialize(msg);

            NetMessage nmsg = new NetMessage();
            nmsg.head = new ProtocolHead();
            nmsg.head.uid = mUid;
            nmsg.head.dataSize = (uint)buffer.Length;
            nmsg.content = buffer;

            byte[] temp = null;
            int len = nmsg.Serialize(out temp);
            mConn.Send(temp, len);
        }
        #endregion


        //================
        //处理传统protobuf
        //=======================
        #region ProtoBuf

        class ListenerHelper
        {
            public uint cmd;
            public uint index;//监听的索引
            public Type TMsg;//反序列化的类型
            public Delegate onMsg;//反序列化后的处理
            public Delegate onErr;//反序列化错误的处理
            public float timeout;
            public float timestamp;
        }
        //该类用于获取应答的索引
        static class MessageIndexGenerator
        {
            private static uint m_lastIndex;
            public static uint NewIndex()
            {
                return ++m_lastIndex;
            }
        }

        //处理由服务器通知的消息
        private DictionarySafe<uint, ListenerHelper> mListNtfListener = new DictionarySafe<uint, ListenerHelper>();
        //处理与服务器之间问答的消息 index-listener
        private MapList<uint, ListenerHelper> mListRepListener = new MapList<uint, ListenerHelper>();


        //发送请求并等待回复
        //req:请求内容
        //cmd：协议类型
        //onRsp:response的处理函数
        public int Send<TReq,TRsp>(uint cmd,TReq req,Action<TRsp> onRsp,float timeout = 30, Action<int> onErr = null)
        {
            NetMessage msg = new NetMessage();
            msg.head.index = MessageIndexGenerator.NewIndex();
            msg.head.cmd = cmd;
            msg.head.uid = mUid;
            Debuger.Log("send msg:{0}", req.ToString());
            msg.content = PBSerializer.NSerialize(req);
            msg.head.dataSize = (uint)msg.content.Length;

            byte[] temp;
            int len = msg.Serialize(out temp);
            Debuger.Log("contentSize:{0},uid:{1},readuid:{2}",msg.content.Length,mUid,BitConverter.ToUInt32(temp,0));
            if (mConn.Send(temp, len))
            {
                AddListener(cmd, typeof(TRsp), onRsp, msg.head.index, timeout, onErr);
            }
            else
            {
                Debuger.LogError("SendMsg failed! msglen:{0}",len);
            }
            return len;
        }

        //发送请求并不处理回复
        public int Send<TReq>(uint cmd,TReq req)
        {
            Debuger.Log("cmd:{0}", cmd);

            NetMessage msg = new NetMessage();
            msg.head.index = 0;
            msg.head.cmd = cmd;
            msg.head.uid = mUid;
            msg.content = PBSerializer.NSerialize(req);
            msg.head.dataSize = (uint)msg.content.Length;

            byte[] temp;
            int len = msg.Serialize(out temp);
            mConn.Send(temp, len);
            return len;
        }

        //添加应答式的监听器
        public void AddListener(uint cmd, Type TRep, Delegate onRep, uint index, float timeout, Action<int> onErr)
        {
            ListenerHelper helper = new ListenerHelper()
            {
                cmd = cmd,
                index = index,
                TMsg = TRep,
                onErr = onErr,
                onMsg = onRep,
                timeout = timeout,
                timestamp = QGFTime.GetTimeSinceStartup()
            };

            mListRepListener.Add(index, helper);
        }



        //添加服务器主动消息的监听器
        private void AddListener<TNtf>(uint cmd,Action<TNtf> onNtf)
        {
            Debuger.Log("cmd:{0},listener:{1},{2}", cmd, onNtf.Method.DeclaringType.Name, onNtf.Method.Name);

            ListenerHelper helper = new ListenerHelper()
            {
                TMsg = typeof(TNtf),
                onMsg = onNtf
            };
            mListNtfListener.Add(cmd, helper);
        }


        private void HandlePBMessage(NetMessage msg)
        {
            //表示该消息为服务器主动通知
            if (msg.head.index == 0)
            {
                var helper = mListNtfListener[msg.head.cmd];
                if (helper != null)
                {
                    var obj = PBSerializer.NDeserialize(msg.content, helper.TMsg);
                    if (obj != null)
                    {
                        helper.onMsg.DynamicInvoke(obj);
                    }
                    else
                    {
                        Debuger.LogError("协议内容格式错误，反序列化失败: cmd{0}", msg.head.cmd);
                    }
                }
                else
                {
                    Debuger.LogError("未找到该协议对应的监听值:cmd:{0}", msg.head.cmd);
                }
            }
            //应答消息
            else
            {
                var helper = mListRepListener[msg.head.index];
                //该部分的listener都只会执行一次
                if (helper != null)
                {
                    mListRepListener.Remove(msg.head.index);
                    object obj = PBSerializer.NDeserialize(msg.content, helper.TMsg);
                    if (obj != null)
                    {
                        helper.onMsg.DynamicInvoke(obj);
                    }
                    else
                    {
                        Debuger.LogError("协议格式错误,反序列化失败:cmd:{0}, index:{1}", msg.head.cmd, msg.head.index);
                    }

                }
                else
                {
                    Debuger.LogError("未找到对应的监听值:cmd:{0}, index:{1}", msg.head.cmd, msg.head.index);
                }
            }
        }
        #endregion

        //================
        //处理超时
        //=================
        #region Timeout

        private float mLastCheckTimeoutStamp = 0;

        private void CheckTimeout()
        {
            float curTime = QGFTime.GetTimeSinceStartup();

            if (curTime - mLastCheckTimeoutStamp >= 0)
            {
                mLastCheckTimeoutStamp = curTime;

                var list = mListRepListener.ToArray();
                for(int i = 0; i < list.Length; ++i)
                {
                    var helper = list[i];
                    float deltaTime = curTime - helper.timestamp;
                    //如果超时了
                    if (deltaTime >= helper.timeout)
                    {
                        mListRepListener.Remove(helper.index);
                        if (helper.onErr != null)
                        {
                            helper.onErr.DynamicInvoke(NetErrorCode.Timeout);
                        }

                        Debuger.LogWarning("cmd {0} is timeout!", helper.cmd);
                    }
                }
            }
        }

        #endregion

    }
}
