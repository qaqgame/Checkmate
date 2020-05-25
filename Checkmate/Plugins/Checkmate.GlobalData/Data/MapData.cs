using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Global.Data
{

    //effect数据
    [ProtoContract]
    public class EffectData
    {
        [ProtoMember(1)] public string effect;//效果文件
    }

    public enum EffectTrigger
    {
        Stay,//停留触发（即回合结束若有单位则触发)
        Enter,//进入触发
        Leave,//离开触发
        Timely,//可用则触发
        Active,//主动触发
        OnAttached,
        OnRemoved
    }

    //feature数据
    [ProtoContract]
    public class FeatureData
    {
        [ProtoMember(1)] public int id;//特征id
        [ProtoMember(2)] public string name;//特征名
        [ProtoMember(3)] public string file;//文件名
        [ProtoMember(4)] public bool single;//单一特征
        [ProtoMember(5)] public List<int> effectIdx;//效果（idx）
    }

    //terrain数据
    [ProtoContract]
    public class TerrainData
    {
        [ProtoMember(1)] public int id;//地形id
        [ProtoMember(2)] public string name;//地形名
        [ProtoMember(3)] public string description;//描述
        [ProtoMember(4)] public string file;//文件
    }

    //地图单元格数据
    [ProtoContract]
    public class MapCellData
    {
        [ProtoMember(1)] public int cost;
        [ProtoMember(2)] public string tag;//标签
        [ProtoMember(3)] public string description;//描述
        [ProtoMember(4)] public byte terrain;//地形类型(此处为idx）
        [ProtoMember(5)] public byte elevation;//海拔
        [ProtoMember(6)] public byte waterLevel;//水平面
        [ProtoMember(7)] public int feature;//特征（idx）
        [ProtoMember(8)] public byte featureLevel;//特征级别
        [ProtoMember(9)] public byte inRiver;//进入的河流
        [ProtoMember(10)] public byte outRiver;//流出的河流
        [ProtoMember(11)] public int road;//道路数据
        [ProtoMember(12)] public bool available;//是否可站
        [ProtoMember(13)] public string extraData;//额外数据
    }


    //地图数据
    [ProtoContract]
    public class MapData
    {
        [ProtoMember(1)] public string version;//版本号
        [ProtoMember(2)] public string title;//地图名
        [ProtoMember(3)] public string mode;//模式名
        [ProtoMember(4)] public int sizeX;//宽度
        [ProtoMember(5)] public int sizeZ;//高度
        [ProtoMember(6)] public int seed;//随机数种子
        [ProtoMember(7)] public List<EffectData> effects;//效果
        [ProtoMember(8)] public List<FeatureData> features;//特征
        [ProtoMember(9)] public List<TerrainData> terrains;//地形
        [ProtoMember(10)] public List<MapCellData> cells;//所有单元格
    }
}
