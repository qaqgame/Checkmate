﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Data
{
    //房间阶段的玩家数据
    [ProtoContract]
    public class PlayerRoomData
    {
        [ProtoMember(1)]
        public uint id;//分配的游戏内player id

        [ProtoMember(2)]
        public uint uid;//用户id

        [ProtoMember(3)]
        public string name;//显示昵称

        [ProtoMember(4)]
        public uint team;//队伍

        [ProtoMember(5)]
        public bool isReady;//是否准备
        public override string ToString()
        {
            return string.Format("<id:{0},name:{1},uid:{2},temp:{3}>", id, name, uid, team);
        }
    }


    //此数据为玩家数据，在进入准备阶段时分配
    [ProtoContract]
    public class PlayerData
    {
        [ProtoMember(1)]
        public uint id;//分配的游戏内player id

        [ProtoMember(2)]
        public uint uid;//用户id

        [ProtoMember(3)]
        public string name;//显示昵称

        [ProtoMember(4)]
        public uint friendMask;//标识友军，1为友军

        [ProtoMember(5)]
        public uint enemyMask;//标识敌军，1为敌军

        public override string ToString()
        {
            return string.Format("<id:{0},name:{1},uid:{2},friend:{3},enemy:{4}>", id, name, uid, friendMask, enemyMask);
        }
    }

    //友军标识数据
    [ProtoContract]
    public class MaskData
    {
        [ProtoMember(1)]
        public uint pid;//分配的游戏内id

        [ProtoMember(2)]
        public uint friendMask;//友军标识

        [ProtoMember(3)]
        public uint enemyMask;//敌军标识

        [ProtoMember(4)]
        public string name;//昵称
    }

    //游戏开始时下行的队伍数据
    [ProtoContract]
    public class PlayerTeamData
    {
        [ProtoMember(1)]
        public List<MaskData> masks;//标识数据
    }


    [ProtoContract]
    public class GameParam
    {
        [ProtoMember(1)]
        public PlayerTeamData teams;//分队数据
        [ProtoMember(2)]
        public uint pid;//pid
        [ProtoMember(3)]
        public List<RoleData> roles;//所有角色
        [ProtoMember(4)]
        public string map;//地图名
    }

    [ProtoContract]
    public class PlayerAP
    {
        [ProtoMember(1)]
        public uint curPlayer;//当前玩家

        [ProtoMember(2)]
        public Dictionary<uint, int> allAp;//所有的ap
    }
}
