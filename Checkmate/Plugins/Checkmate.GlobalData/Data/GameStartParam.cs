﻿using ProtoBuf;
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





    public class GameParamUtils
    {

    }
}
