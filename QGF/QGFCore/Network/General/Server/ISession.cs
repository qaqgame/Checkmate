using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.General.Server
{
    public interface ISessionListener
    {
        void OnReceive(ISession session, byte[] bytes, int len);
    }

    public static class SessionID
    {
        private static uint mLastId = 0;

        public static uint NewID()
        {
            return ++mLastId;
        }
    }

    public interface ISession
    {
        //session独有id
        uint id { get; }
        uint uid { get; }

        ushort ping { get; set; }

        bool IsActived();//判断是否还处于活跃状态
        void Active(EndPoint remote);

        bool InAuth();//判定session是否已授权

        void SetAuth(uint userId);

        bool Send(byte[] bytes, int len);

        //远端地址（客户端)
        IPEndPoint remoteEndPoint { get; }

        void Tick(DateTime currentTime);
        void DoReceiveInGateway(byte[] buffer, int len);
    }
}
