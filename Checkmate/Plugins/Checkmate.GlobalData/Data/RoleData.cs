using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Data
{
    [ProtoContract]
    public class v3i
    {
        [ProtoMember(1)] public int x;
        [ProtoMember(2)] public int y;
        [ProtoMember(3)] public int z;
    }

    [ProtoContract]
    public class RoleProperty {
        [ProtoMember(1)]
        public int hp;//最大生命值
        [ProtoMember(2)]
        public int mp;//最大mp
        [ProtoMember(3)]
        public float miss;//闪避率
        [ProtoMember(4)]
        public int physicalRes;//物理抵抗
        [ProtoMember(5)]
        public int magicRes;//魔法抵抗
        [ProtoMember(6)]
        public int attack;//攻击力
        [ProtoMember(7)]
        public int moveRange;//行动力
        [ProtoMember(8)]
        public float attackSpeed;//攻击速度
        [ProtoMember(9)]
        public float physicalIgnore;//物穿
        [ProtoMember(10)]
        public float magicIgnore;//法穿
        [ProtoMember(11)]
        public int viewRange;//视距
        [ProtoMember(12)]
        public int viewHeight;//视高
        [ProtoMember(13)]
        public string extraData;//额外数据
    }

    [ProtoContract]
    public class RoleData
    {
        [ProtoMember(1)]
        public string name;//显示名
        [ProtoMember(2)]
        public string model;//显示的模型名
        [ProtoMember(3)]
        public int team;//队伍id
        [ProtoMember(4)]
        public v3i position;//初始位置
        [ProtoMember(5)]
        public int status;//初始状态
        [ProtoMember(6)]
        public int mask;//可站立的方格类型掩码
        [ProtoMember(7)]
        public Dictionary<int, int> extraMove;//额外的行动力
        [ProtoMember(8)]
        public RoleProperty props;//初始属性

        [ProtoMember(9)]
        public string extraData;//额外数据
    }
}
