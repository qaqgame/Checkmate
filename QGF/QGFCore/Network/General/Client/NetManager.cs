using QGF.Codec;
using QGF.Network.Core;
using QGF.Network.Core.RPCLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.General.Client
{
    class NetManager
    {

        private IConnection mConn;
        private uint mUid;//user id
        private RPCManagerBase mRPC;

        private string mCurInvokeName;//当前RPC调用的函数


        //连接类型-连接id-连接用的本地端口
        public void Init(Type connType,int connId,int bindPort)
        {
            Debuger.Log("connType:{0}, connId:{1},bindPort:{3}", connType, connId, bindPort);
            mConn = Activator.CreateInstance(connType) as IConnection;
            mConn.Init(connId, bindPort);
            mRPC = new RPCManagerBase();
            mRPC.Init();
        }


        private void Clear()
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
                //HandlePBMessage(msg);
            }
        }


        //============================
        //处理RPC消息
        //================================
        //注册RPC函数反射
        public void RegistRPCListener(object listener)
        {
            mRPC.RegistListner(listener);
        }
        public void HandleRPCMessage(RPCMessage msg)
        {
            Debuger.Log("Connection[{0}]->{1}<{2}>", mConn.id, msg.name, msg.args.ToString());
            
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
            nmsg.head.dataSize = (ushort)buffer.Length;
            nmsg.content = buffer;

            byte[] temp = null;
            int len = nmsg.Serialize(out temp);
            mConn.Send(temp, len);
        }


    }
}
