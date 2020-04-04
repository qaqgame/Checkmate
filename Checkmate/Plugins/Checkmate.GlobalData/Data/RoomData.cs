using ProtoBuf;
using QGF.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Data
{
    //房间的数据
    [ProtoContract]
    public class RoomData
    {
        [ProtoMember(1)]
        public uint id;//房间id
        [ProtoMember(2)]
        public string name;//房间名
        [ProtoMember(3)]
        public List<PlayerRoomData> players = new List<PlayerRoomData>();//所有的玩家

        public override string ToString()
        {
            return string.Format("<id:{0},name:{1},players:{2}>", id, name, players.ToListString());
        }
    }

    [ProtoContract]
    public class RoomListData
    {
        [ProtoMember(1)]
        public List<RoomData> rooms = new List<RoomData>();

    }
}
