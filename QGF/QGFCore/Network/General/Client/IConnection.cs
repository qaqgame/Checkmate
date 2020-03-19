using QGF.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.General.Client
{
    public interface IConnection
    {
        //字节-字节长度
        //接收到数据时调用
        QGFEvent<byte[],int> onReceive { get; }

        void Init(int connId,int bindPort);
        void Clear();

        bool Connected { get; }
        int id { get; }
        int bindPort { get; }

        void Connect(string ip, int port);
        void Close();

        bool Send(byte[] bytes, int len);

        void Tick();//周期调用
    }
}
