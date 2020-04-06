using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Data
{
    //此类为游戏开始时传给服务器的参数
    [ProtoContract]
    public class GameStartParam
    {
        [ProtoMember(1)]
        public GameParam gameParam = new GameParam();

        [ProtoMember(2)]
        public List<PlayerData> players = new List<PlayerData>();//所有的玩家

        [ProtoMember(3)]
        public List<RoleData> roles = new List<RoleData>();//所有的角色
    }

    //游戏启动的参数
    [ProtoContract]
    public class GameParam
    {
        [ProtoMember(1)]
        public uint id = 0;//该局游戏的id

        [ProtoMember(2)]
        public MapData mapData = new MapData();//地图数据

        [ProtoMember(3)]
        public int randomSeed = 0;//随机数种子
    }

    //地图的数据
    //如：胜利条件
    [ProtoContract]
    public class MapData
    {
        [ProtoMember(1)]
        public uint id = 0;//地图id

        [ProtoMember(2)]
        public string name = "";//地图名

        [ProtoMember(3)]
        public string mode = "";//游戏模式
    }

    //角色的数据
    [ProtoContract]
    public class RoleData
    {
        [ProtoMember(1)]
        public uint pid;//所属的player

        [ProtoMember(2)]
        public string name = "";//显示的名字

        [ProtoMember(3)]
        public string file = "";//文件
    }


    public class GameParamUtils
    {

    }
}
