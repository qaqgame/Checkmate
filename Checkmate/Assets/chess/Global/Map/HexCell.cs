using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Checkmate.Global.Data;
using Checkmate.Game.Controller;
using Checkmate.Game.Feature;

namespace Checkmate.Game
{
    public enum HexDirection
    {
        NE, SE, S, SW, NW, N
    }
    public enum TerrainType
    {
        Water,
        Terrain,//1-1024留给地形
        Feature//1024+为特征
    }
    public static class HexDirectionExtensioons
    {
        //存储6个方向
        public static readonly List<Position> directions = new List<Position>{
        new Position(1, -1, 0),new Position(1, 0, -1),new Position(0, 1, -1),
        new Position(-1, 1, 0),new Position(-1, 0, 1),new Position(0, -1, 1)
    };
        public static HexDirection Opposite(this HexDirection direction)
        {
            return (int)direction < 3 ? (direction + 3) : (direction - 3);
        }
        public static HexDirection Next(this HexDirection direction)
        {
            return direction == HexDirection.N ? HexDirection.NE : (direction + 1);
        }
        public static HexDirection Previous(this HexDirection direction)
        {
            return direction == HexDirection.NE ? HexDirection.N : (direction - 1);
        }
        public static HexDirection Previous2(this HexDirection direction)
        {
            direction -= 2;
            return direction >= HexDirection.NE ? direction : (direction + 6);
        }
        public static HexDirection Next2(this HexDirection direction)
        {
            direction += 2;
            return direction <= HexDirection.N ? direction : (direction - 6);
        }
        public static Position ToPosition(this HexDirection direction)
        {
            return directions[(int)direction];
        }
    }
    public class HexCell : MonoBehaviour
    {
        private CellController mController;//属性控制器
        //public List<int> traps;      // 可以触发的陷阱列表
        //public Dictionary<TrapTrigger, List<int>> triggerTraps = new Dictionary<TrapTrigger, List<int>>();
        public HexPosition coordinates;//hex坐标
        public HexGridChunk chunk;//所属的块
        private int terrainTypeIndex;//地形类型
        private int currentRole = -1;//当前站在上面的角色id
        private int effects = -1;//效果的idx


        public CellController Controller
        {
            get
            {
                return mController;
            }
        }

        //数据（权重，地形等）
        public HexCellShaderData ShaderData
        {
            get;
            set;
        }
        //代表在HexGrid数组中的下标
        public int Index { get; set; }
        public int Effects
        {
            get
            {
                return effects;
            }
            set
            {
                effects = value;
                if (effects != -1)
                {
                    //效果不为空时的操作
                }
            }
        }
        public int TerrainTypeIndex
        {
            get
            {
                return terrainTypeIndex;
            }
            set
            {
                if (terrainTypeIndex != value)
                {
                    terrainTypeIndex = value;
                    ShaderData.RefreshTerrain(this);
                }
            }
        }

        

        //是否可见
        [SerializeField]
        int visibility = 0;
        public bool IsVisible
        {
            get
            {
                return visibility > 0;
            }
        }

        private int feature = -1;//特征类型
        private int featureLevel = -1;//特征等级
        public int Feature
        {
            get
            {
                return feature;
            }
            set
            {
                if (feature == value)
                {
                    return;
                }
                feature = value;
                ValidateAccess();
                RefreshSelfOnly();
            }
        }
        public int FeatureLevel
        {
            get
            {
                return featureLevel;
            }
            set
            {
                if (featureLevel == value)
                {
                    return;
                }
                featureLevel = value;
                ValidateAccess();
                RefreshSelfOnly();
            }
        }

        public bool HasLargeFeature
        {
            get
            {
                return HasFeatures&&FeatureManager.Instance.GetFeatureData(Feature).single;
            }
        }

        public bool HasFeatures
        {
            get
            {
                return feature >= 0 && featureLevel >= 0;
            }
        }
        public bool firstAddFeature = false;//第一次添加特征
        public RectTransform uiRect;//labe的位置
        public Vector3 Position
        {
            get
            {
                return transform.localPosition;
            }
        }
        private int elevation = int.MinValue;//高度

        int waterLevel = int.MinValue;//水位
        public int WaterLevel
        {
            get
            {
                return waterLevel;
            }
            set
            {
                //存在特征时无法修改水位
                if (waterLevel == value || HasFeatures)
                {
                    return;
                }
                waterLevel = value;
                ValidateRivers();
                ValidateAccess();
                Refresh();
            }
        }
        //是否在水下
        public bool IsUnderWater
        {
            get
            {
                return waterLevel > elevation;
            }
        }
        //水平面的y
        public float WaterSurfaceY
        {
            get
            {
                return (waterLevel + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
            }
        }

        [SerializeField]
        HexCell[] neighbors;//邻接点

        [SerializeField]
        bool[] roads;//是否存在道路

        [SerializeField]
        bool[] access;//是否可通行

        bool hasIncomingRiver, hasOutgoingRiver;//进入，离开的河流
        HexDirection incomingRiver, outingRiver;//进入和离开的方向

        public bool HasIncomingRiver
        {
            get
            {
                return hasIncomingRiver;
            }
        }
        public bool HasOutgoingRiver
        {
            get
            {
                return hasOutgoingRiver;
            }
        }

        public HexDirection IncomingRiver
        {
            get
            {
                return incomingRiver;
            }
        }

        public HexDirection OutgoingRiver
        {
            get
            {
                return outingRiver;
            }
        }

        public bool HasRiver
        {
            get
            {
                return hasIncomingRiver || hasOutgoingRiver;
            }
        }

        public bool HasRiverBeginOrEnd
        {
            get
            {
                return hasIncomingRiver != hasOutgoingRiver;
            }
        }

        public float StreamBedY
        {
            get
            {
                return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;
            }
        }
        public float RiverSurfaceY
        {
            get
            {
                return (elevation + HexMetrics.waterElevationOffset) * HexMetrics.elevationStep;
            }
        }

        public HexDirection RiverBeginOrEndDirection
        {
            get
            {
                return hasIncomingRiver ? incomingRiver : outingRiver;
            }
        }

        public bool HasRiverThroughEdge(HexDirection direction)
        {
            return (hasIncomingRiver && incomingRiver == direction) ||
                    (hasOutgoingRiver && outingRiver == direction);
        }
        //移除流出的河
        public void RemoveOutgoingRiver()
        {
            if (!hasOutgoingRiver)
            {
                return;
            }
            hasOutgoingRiver = false;
            RefreshSelfOnly();

            HexCell neighbor = GetNeighbor(OutgoingRiver);
            neighbor.hasIncomingRiver = false;
            neighbor.RefreshSelfOnly();
        }
        //移除流入的河流
        public void RemoveIncomingRiver()
        {
            if (!hasIncomingRiver)
            {
                return;
            }
            hasIncomingRiver = false;
            RefreshSelfOnly();

            HexCell neighbor = GetNeighbor(incomingRiver);
            neighbor.hasOutgoingRiver = false;
            neighbor.RefreshSelfOnly();
        }
        //移除河流
        public void RemoveRiver()
        {
            RemoveOutgoingRiver();
            RemoveIncomingRiver();
        }
        //添加流出的河流
        public void SetOutgoingRiver(HexDirection direction)
        {
            //已存在则不管,若有特征则不管
            if (hasOutgoingRiver && outingRiver == direction && !HasFeatures)
            {
                return;
            }
            //确保有邻居且高度更低
            HexCell neighbor = GetNeighbor(direction);
            if (!IsValidRiverDestination(neighbor))
            {
                return;
            }
            //清除原来的流出河以及该方向上的流入河
            RemoveOutgoingRiver();
            if (hasIncomingRiver && incomingRiver == direction)
            {
                RemoveIncomingRiver();
            }
            //设置流出河
            hasOutgoingRiver = true;
            outingRiver = direction;

            //设置邻居的河流方向
            neighbor.RemoveIncomingRiver();
            neighbor.hasIncomingRiver = true;
            neighbor.incomingRiver = direction.Opposite();

            SetRoad((int)direction, false);
        }

        //道路

        //是否存在道路
        public bool HasRoads
        {
            get
            {
                for (int i = 0; i < roads.Length; ++i)
                {
                    if (roads[i])
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        //是否有道路穿过某侧
        public bool HasRoadThroughEdge(HexDirection direction)
        {
            return roads[(int)direction];
        }

        public void RemoveRoads()
        {
            for (int i = 0; i < neighbors.Length; ++i)
            {
                if (roads[i])
                {
                    SetRoad(i, false);
                }
            }
        }

        public void AddRoad(HexDirection d)
        {
            //防止与河流同一方向,防止高度差过高
            if (!roads[(int)d] && !HasRiverThroughEdge(d) && GetElevationDifference(d) <= 1)
            {
                SetRoad((int)d, true);
            }
        }
        //设置某个方向的道路
        private void SetRoad(int idx, bool state)
        {
            roads[idx] = state;
            neighbors[idx].roads[(int)((HexDirection)idx).Opposite()] = state;
            neighbors[idx].RefreshSelfOnly();
            RefreshSelfOnly();
        }

        public int Elevation
        {
            get
            {
                return elevation;
            }
            set
            {
                if (elevation == value)
                {
                    return;
                }
                elevation = value;
                RefreshPosition();

                //清除流动错误的河流,防止河流从低向高
                ValidateRivers();
                ValidateAccess();

                //清除道路
                for (int i = 0; i < roads.Length; ++i)
                {
                    if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                    {
                        SetRoad(i, false);
                    }
                }
                Refresh();
            }
        }

        //获取高度差异
        public int GetElevationDifference(HexDirection dir)
        {
            int origin = IsUnderWater ? waterLevel : elevation;
            int target = GetNeighbor(dir).IsUnderWater ? GetNeighbor(dir).WaterLevel : GetNeighbor(dir).Elevation;
            int diff = origin-target;
            return diff >= 0 ? diff : -diff;
        }



        private void Awake()
        {

        }

        private void Start()
        {
            ValidateAccess();
        }

        //获取某方向的邻接
        public HexCell GetNeighbor(HexDirection dir)
        {
            return neighbors[(int)dir];
        }
        //设置目标方向的邻接
        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            neighbors[(int)direction] = cell;
            cell.neighbors[(int)direction.Opposite()] = this;
        }

        public HexEdgeType GetEdgeType(HexDirection direction)
        {
            if (neighbors[(int)direction] == null)
            {
                throw new System.Exception("error get edge type,direction not have a neighbor");
            }
            return HexMetrics.GetEdgeType(
                elevation, neighbors[(int)direction].elevation
            );
        }

        public HexEdgeType GetEdgeType(HexCell other)
        {
            return HexMetrics.GetEdgeType(elevation, other.elevation);
        }

        //判断河流能否流过去
        bool IsValidRiverDestination(HexCell neighbor)
        {
            return neighbor && elevation >= neighbor.elevation || waterLevel == neighbor.elevation;
        }
        //在改变高度或水位时被调用
        void ValidateRivers()
        {
            //当流出的河流不再可行时，移除
            if (hasOutgoingRiver && IsValidRiverDestination(GetNeighbor(outingRiver)))
            {
                RemoveOutgoingRiver();
            }
            //检查流入的河流
            if (hasIncomingRiver && !GetNeighbor(incomingRiver).IsValidRiverDestination(this))
            {
                RemoveIncomingRiver();
            }
        }

        //验证可通行性
        void ValidateAccess()
        {
            Debug.Log("start validate access");
            //检测高度
            for (HexDirection direction = HexDirection.NE; direction <= HexDirection.N; ++direction)
            {
                HexCell neighbor = GetNeighbor(direction);
                //无邻居，设为否
                if (!neighbor)
                {
                    access[(int)direction] = false;
                }
                //有邻居,检测其海拔相差绝对值是否在1以内
                else
                {
                    if (GetElevationDifference(direction) > 1)
                    {
                        access[(int)direction] = false;
                    }
                    else
                    {
                        access[(int)direction] = true;
                    }
                }
            }

        }



        // Update is called once per frame
        void Update()
        {

        }
        //仅刷新自己
        void RefreshSelfOnly()
        {
            chunk.Refresh();
            ValidateAccess();
        }
        //刷新自身以及邻居
        void Refresh()
        {
            if (chunk)
            {
                chunk.Refresh();
                for (int i = 0; i < neighbors.Length; ++i)
                {
                    HexCell neighbor = neighbors[i];
                    if (neighbor != null && neighbor.chunk != chunk)
                    {
                        neighbor.chunk.Refresh();
                    }
                }
            }
            ValidateAccess();
        }

        //是否可以添加小型特征
        public bool IsValidSmallFeature()
        {
            return !IsUnderWater;
        }

        //是否可以添加大型特征
        public bool IsValidLargeFeature()
        {
            return !IsUnderWater && !HasRiver && !HasRoads;
        }

        //是否可以通行
        private bool IsAccessible(HexDirection dir)
        {
            return access[(int)dir];
        }

        //是否可以到达
        public bool IsReachable(HexDirection direction)
        {
            HexCell neighbor = GetNeighbor(direction);
            if (!neighbor)
            {
                return false;
            }
            return neighbor.IsAccessible(direction.Opposite());
        }
        //禁用标签
        public void DisableLabel()
        {
            Text text = uiRect.GetChild(0).GetComponent<Text>();
            text.enabled = false;
            text.fontSize = 4;
            text.color = Color.black;
        }

        //使用标签
        public void EnableLable(string label)
        {
            Text text = uiRect.GetChild(0).GetComponent<Text>();
            text.text = label;
            text.enabled = true;
        }

        public void EnableLable(string label, int size, Color color)
        {
            Text text = uiRect.GetChild(0).GetComponent<Text>();
            text.text = label;
            text.enabled = true;
            text.fontSize = size;
            text.color = color;
        }

        //禁用颜色
        public void DisableHighlight()
        {
            Image highlight = uiRect.GetChild(1).GetComponent<Image>();
            highlight.enabled = false;
        }
        //启用颜色
        public void EnableHighlight(Color color)
        {
            Image highlight = uiRect.GetChild(1).GetComponent<Image>();
            highlight.color = color;
            highlight.enabled = true;
        }

        public Color GetHighlightColor()
        {
            Image highlight = uiRect.GetChild(1).GetComponent<Image>();
            return highlight.color;
        }
        //更新位置
        private void RefreshPosition()
        {
            Vector3 position = transform.localPosition;
            position.y = elevation * HexMetrics.elevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;
        }

        //// Add trapid to cell
        //public void AddToHexcell(TrapTrigger trigger, int trapId)
        //{
        //    if (this.triggerTraps.ContainsKey(trigger))
        //    {
        //        if (!this.triggerTraps[trigger].Contains(trapId))
        //        {
        //            this.triggerTraps[trigger].Add(trapId);
        //        }
        //    }
        //    else
        //    {
        //        List<int> trapids = new List<int>();
        //        trapids.Add(trapId);
        //        this.triggerTraps.Add(trigger, trapids);
        //    }
        //}

        //// remove trapid from cell
        //public void RemoveFromHexcell(TrapTrigger trigger, int trapId)
        //{
        //    if (this.triggerTraps.ContainsKey(trigger))
        //    {
        //        if (this.triggerTraps[trigger].Contains(trapId))
        //        {
        //            // Debug.LogError("remove trap id: "+trapId);
        //            this.triggerTraps[trigger].Remove(trapId);
        //        }
        //    }
        //}

        //设置当前的角色,triggerEvent:是否触发相关事件
        public void SetCurRole(int rc, bool triggerEvent = true)
        {
            currentRole = rc;
            //if (triggerEvent && this.triggerTraps.ContainsKey(TrapTrigger.OnEnter))
            //{
            //    int[] tmp = this.triggerTraps[TrapTrigger.OnEnter].ToArray();
            //    for (int i = 0; i < tmp.Length; i++)
            //    {
            //        TrapMonitor.ExecuteTrap(tmp[i], TrapTrigger.OnEnter, rc);
            //    }
            //}
            //调用相关事件
            if (triggerEvent && effects != -1)
            {
                //effects.OnEnter(rc);
            }
        }
        //移除当前角色
        public void RemoveCurRole(bool triggerEvent = true)
        {
            //调用相关事件
            if (triggerEvent && effects != -1 && currentRole != -1)
            {
                //effects.OnLeave(currentRole);
            }
            currentRole = -1;
        }
        //获取当前角色
        public int GetRole()
        {
            return currentRole;
        }


        //提升可见度
        public void IncreaseVisibility()
        {
            visibility++;
            Debug.Log(coordinates.ToPosition().ToString() + " increase visibility");
            if (visibility == 1)
            {
                ShaderData.RefreshVisibility(this);
            }
        }
        //减少可见度
        public void DecreaseVisibility()
        {
            visibility = 0;
            if (visibility == 0)
            {
                ShaderData.RefreshVisibility(this);
            }
        }

        //保存
        public MapCellData Save()
        {
            MapCellData data = new MapCellData();
            data.terrain = (byte)terrainTypeIndex;
            data.elevation =(byte)elevation;
            data.waterLevel = (byte)waterLevel;
            data.feature = feature;
            data.featureLevel = (byte)featureLevel;
            //写入河流
            if (hasIncomingRiver)
            {
                data.inRiver = (byte)(incomingRiver + 128);
            }
            else
            {
                data.inRiver = (byte)0;
            }

            if (hasOutgoingRiver)
            {
                data.outRiver = (byte)(outingRiver + 128);
            }
            else
            {
                data.outRiver = (byte)0;
            }

            //写入道路
            int roadFlags = 0;
            for (int i = 0; i < roads.Length; ++i)
            {
                if (roads[i])
                {
                    roadFlags |= 1 << i;
                }
            }
            data.road = roadFlags;

            return data;
        }

        //加载
        public void Load(MapCellData data)
        {
            
            //=================================
            //加载显示用数据
            //加载地形纹理数据后刷新
            
            terrainTypeIndex =data.terrain;
            ShaderData.RefreshTerrain(this);

            elevation = data.elevation;
            RefreshPosition();
            waterLevel = data.waterLevel;
            feature = data.feature;
            featureLevel = data.featureLevel;

            byte riverData = data.inRiver;
            if (riverData >= 128)
            {
                hasIncomingRiver = true;
                incomingRiver = (HexDirection)(riverData - 128);
            }
            else
            {
                hasIncomingRiver = false;
            }

            riverData = data.outRiver;
            if (riverData >= 128)
            {
                hasOutgoingRiver = true;
                outingRiver = (HexDirection)(riverData - 128);
            }
            else
            {
                hasOutgoingRiver = false;
            }

            //写入道路
            int roadFlags = data.road;
            for (int i = 0; i < roads.Length; ++i)
            {
                roads[i] = (roadFlags & (1 << i)) != 0;
            }

            //=================加载游戏中数据
            int cost = data.cost;
            bool available = data.available;
            string extra = data.extraData;
            mController = new CellController(cost, available, this, extra);


        }
    }
}