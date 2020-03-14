using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QGF.Network.Core
{
    class NetMessage
    {

        private static NetBuffer DefaultWriter = new NetBuffer(4096);
        private static NetBuffer DefaultReader = new NetBuffer(4096);

        public ProtocolHead head = new ProtocolHead();
        public byte[] content;//内容

        //反序列化
        public NetMessage Deserialize(NetBuffer buffer)
        {
            head.Deserialize(buffer);
            content = new byte[head.dataSize];
            buffer.ReadBytes(content, 0, head.dataSize);
            return this;
        }
        //序列化
        public NetBuffer Serialize(NetBuffer buffer)
        {
            head.Serialize(buffer);
            buffer.WriteBytes(content, 0, head.dataSize);
            return buffer;
        }

        public NetMessage Deserialize(byte[] buffer, int size)
        {
            lock (DefaultReader)
            {
                DefaultReader.Attach(buffer, size);
                return Deserialize(DefaultReader);
            }
        }

        public int Serialize(out byte[] tempBuffer)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Clear();
                this.Serialize(DefaultWriter);
                tempBuffer = DefaultWriter.GetBytes();
                return DefaultWriter.Length;
            }

        }
    }
}
