using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.Core
{
    //协议头
    public class ProtocolHead
    {
        public const int Length = 20;//头部分长度
        public uint uid = 0;
        public uint cmd = 0;//协议类型
        public uint index = 0;//序列号
        public int dataSize = 0;//数据长度
        public uint checksum = 0;//检验和

        public ProtocolHead Deserialize(NetBuffer buffer)
        {
            ProtocolHead head = this;
            head.uid = buffer.ReadUInt();
            head.cmd = buffer.ReadUInt();
            head.index = buffer.ReadUInt();
            head.dataSize = buffer.ReadInt();
            head.checksum = buffer.ReadUInt();
            return head;
        }

        public NetBuffer Serialize(NetBuffer buffer)
        {
            buffer.WriteUInt(uid);
            buffer.WriteUInt(cmd);
            buffer.WriteUInt(index);
            buffer.WriteInt(dataSize);
            buffer.WriteUInt(checksum);
            return buffer;
        }
    }
}
