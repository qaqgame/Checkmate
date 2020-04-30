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
        public string mode;//游戏模式名
        [ProtoMember(4)]
        public string map;//地图名
        [ProtoMember(5)]
        public List<PlayerRoomData> players = new List<PlayerRoomData>();//所有的玩家

        [ProtoMember(6)]
        public int maxPlayerCount;//最大的玩家数

        [ProtoMember(7)]
        public List<int> teams;//每个队伍的数目 

        [ProtoMember(8)]
        public bool ready;//预备开始

        [ProtoMember(9)]
        public int time;//倒计时
        public override string ToString()
        {
            return string.Format("<id:{0},name:{1},players:{2},maxPlayers:{3}>", id, name, players.ToListString(),maxPlayerCount);
        }
    }

    [ProtoContract]
    public class RoomListData
    {
        [ProtoMember(1)]
        public List<RoomData> rooms = new List<RoomData>();

    }
}
