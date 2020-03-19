using Assets.chess;
using Assets.Chess;
using Checkmate.Global.Data;
using Checkmate.Global.Proto;
using Checkmate.Global.Server;
using QGF;
using QGF.Common;
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
        //private HeartBeatHandler m_heartbeat;

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

        }

        public void Clear()
        {
            GlobalEvent.onUpdate.RemoveListener(OnUpdate);

            if (m_net != null)
            {
                m_net.Clear();
                m_net = null;
            }

            //if (m_heartbeat != null)
            //{
            //    m_heartbeat.onTimeout.RemoveListener(OnHeartBeatTimeout);
            //    m_heartbeat.Clean();
            //    m_heartbeat = null;
            //}
        }

        private void Connect()
        {
            m_net.Connect("127.0.0.1", 4540);
        }

        private void CloseConnect()
        {
            m_net.Close();
        }

        private void OnUpdate()
        {
            m_net.Update();
        }


        //private void OnHeartBeatTimeout()
        //{
        //    Debuger.LogError("");
        //    CloseConnect();

        //    m_heartbeat.Stop();

        //    ReLogin();
        //}

        //private void ReLogin()
        //{
        //    Connect();

        //    LoginProto req = new LoginProto();
        //    req.id = (int)m_mainUserData.id;
        //    req.name = m_mainUserData.name;

        //    m_net.Send<LoginProto, LoginRsp>(ProtoCmd.LoginReq, req, OnLoginRsp, 30, OnLoginErr);
        //}

        public void Login(string name)
        {
            Connect();

            LoginProto req = new LoginProto();
            req.id = 0;
            req.name = name;

            m_net.Send<LoginProto, LoginRsp>(ProtoCmd.LoginReq, req, OnLoginRsp, 30, OnLoginErr);
        }

        private void OnLoginRsp(LoginRsp rsp)
        {
            Debuger.Log("ret:{0},msg:{1}", rsp.ret, rsp.userData);
            if (rsp.ret.code == 0)
            {
                //userdata赋值
                mMainUserData = rsp.userData;
                AppConfig.Value.mainUserData = mMainUserData;
                AppConfig.Save();
                //启动心跳


                GlobalEvent.onLogin.Invoke(true);
            }
        }




        private void OnLoginErr(int errcode)
        {
            Debuger.LogError("ErrCode:{0}", errcode);
        }



       
        public void Logout()
        {
            //停止心跳

            if (mMainUserData != null)
            {
                m_net.Invoke("Logout");
            }

            mMainUserData = null;
        }


        //登出的回包处理
        private void OnLogout()
        {
            Debuger.Log();
            CloseConnect();
        }


    }
}
