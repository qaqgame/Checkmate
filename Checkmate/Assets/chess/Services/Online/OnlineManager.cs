using Assets.chess;
using Assets.Chess;
using Checkmate.Global.Data;
using Checkmate.Global.Proto;
using Checkmate.Global.Server;
using QGF;
using QGF.Common;
using QGF.Network.Core.RPCLite;
using QGF.Network.General.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Services.Online
{
    public class OnlineManager : Singleton<OnlineManager>
    {
        private NetManager m_net;
        private UserData mMainUserData;
        private HeartBeatHandler m_heartbeat;

        public UserData MainUserData { get { return mMainUserData; } }

        public static NetManager Net
        {
            get { return Instance.m_net; }
        }


        public void Init()
        {
            m_net = new NetManager();

            m_net.Init(typeof(KCPConnect), ServerID.ZoneServer, 0);

            m_net.RegistRPCListener(this);

            GlobalEvent.onUpdate.AddListener(OnUpdate);

            m_heartbeat = new HeartBeatHandler();
            m_heartbeat.Init(m_net);
            m_heartbeat.onTimeout.AddListener(OnHeartBeatTimeout);
        }

        public void Clear()
        {
            GlobalEvent.onUpdate.RemoveListener(OnUpdate);

            if (m_net != null)
            {
                m_net.Clear();
                m_net = null;
            }

            if (m_heartbeat != null)
            {
                m_heartbeat.onTimeout.RemoveListener(OnHeartBeatTimeout);
                m_heartbeat.Clear();
                m_heartbeat = null;
            }
        }

        private void Connect()
        {
            m_net.Connect("120.79.240.163", 4050);
        }

        private void CloseConnect()
        {
            m_net.Close();
        }

        private void OnUpdate()
        {
            m_net.Update();
        }

        //处理心跳超时
        private void OnHeartBeatTimeout()
        {
            Debuger.LogError("");
            CloseConnect();

            m_heartbeat.Stop();

            ReLogin();
        }

        //重新登录
        private void ReLogin()
        {
            Connect();

            LoginProto req = new LoginProto();
            req.id = mMainUserData.id;
            req.name = mMainUserData.name;

            m_net.Send<LoginProto, LoginRsp>(ProtoCmd.LoginReq, req, OnLoginRsp, 30, OnLoginErr);
        }

        public void Login(string name)
        {
            Connect();

            LoginProto req = new LoginProto();
            req.id = 2;
            req.name = name;

            m_net.Send<LoginProto, LoginRsp>(ProtoCmd.LoginReq, req, OnLoginRsp, 10, OnLoginErr);
        }

        private void OnLoginRsp(LoginRsp rsp)
        {
            Debuger.Log("ret:{0},msg:{1}", rsp.ret.ToString(), rsp.userData.ToString());
            if (rsp.ret.code == 0)
            {
                //userdata赋值
                mMainUserData = rsp.userData;
                
                AppConfig.Value.mainUserData = mMainUserData;
                AppConfig.Save();

                m_net.SetUserId(rsp.userData.id);

                //启动心跳
                m_heartbeat.Start();

                GlobalEvent.onLoginSuccess.Invoke();
            }
            else
            {
                GlobalEvent.onLoginFailed.Invoke(rsp.ret.code, rsp.ret.info);
            }
        }




        private void OnLoginErr(int errcode)
        {
            Debuger.LogError("ErrCode:{0}", errcode);
            GlobalEvent.onLoginFailed.Invoke(errcode, "消息发送失败!");
        }


    }
}
