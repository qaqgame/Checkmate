using Checkmate.Game.Controller;
using Checkmate.Global.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Checkmate.Game.Feature;
using Checkmate.Game.Role;
using Checkmate.Game.Map;
using Checkmate.Game.Effect;
using QGF;

namespace Checkmate.Game.Controller
{
    public class CellController :ModelController
    {
        private HexCell mCell;

        private int mRole=-1;//当前的角色


        private List<int> effects=null;//该方格存在的效果

        public HexCell Cell
        {
            get { return mCell; }
        }

        public int Role
        {
            get { return mRole; }
            set {
                //原始存在角色，触发移除事件
                if (mRole != -1)
                {
                    RoleController role = RoleManager.Instance.GetRole(mRole);
                    
                    ExecuteEffect(EffectTrigger.Leave, role);

                    //移除effect的属性加成
                    if (effects != null && effects.Count > 0)
                    {
                        foreach(var track in effects)
                        {
                            Checkmate.Game.Effect.Effect e = EffectManager.Instance.GetEffect(track);
                            role.CurrentMap.RemoveTrack(e.Current);
                            e.Current.Clear();
                        }
                    }
                }
                //角色进入.触发进入事件
                if (value != -1)
                {
                    RoleController role = RoleManager.Instance.GetRole(value);
                    
                    ExecuteEffect(EffectTrigger.Enter, role);
                }
                mRole = value;
            }
        }


        private List<int> mVisibleRoles=new List<int>();//可见该方格的角色
        
        public void SetVisibleRole(int rid)
        {
            if (!mVisibleRoles.Contains(rid))
            {
                mVisibleRoles.Add(rid);
            }
        }
        public void RemoveVisibleRole(int rid)
        {
            if (mVisibleRoles.Contains(rid))
            {
                mVisibleRoles.Remove(rid);
            }
        }


        public void RemoveVisibility(RoleController role)
        {
            
            MapManager.Instance.DecreaseVisibility(role);
        }

        public void SetVisibility(RoleController role)
        {
            
            MapManager.Instance.IncreaseVisibility(role);
        }


        private void ExecuteEffect(EffectTrigger trigger,RoleController role)
        {
            //遍历所有，如果满足触发
            if (effects != null && effects.Count > 0)
            {
                int cnt = 0;
                foreach (var track in effects)
                {
                    ++cnt;
                    EffectManager.Instance.ExecuteEffect(track, this, role,trigger);
                }
                Debuger.Log("effect {0} execute {1} when {2}", Position.ToString(), cnt.ToString(),trigger.ToString());
            }
        }

        public bool HasRole
        {
            get { return mRole > -1; }
        }

        public CellController(int cost,bool available,HexCell cell,string extra=null):base(extra)
        {
            _cost = cost;
            _available = available;
            mCell = cell;

            //如果存在特征加载效果
            if (mCell.Feature >= 0)
            {
               
                FeatureData fd = FeatureManager.Instance.GetFeatureData(mCell.Feature);
                Debuger.Log("add effect cnt:{0}", fd.effectIdx.Count.ToString());
                if (fd.effectIdx != null && fd.effectIdx.Count > 0)
                {
                    effects = new List<int>();
                    foreach (var idx in fd.effectIdx)
                    {
                        int track = EffectManager.Instance.InstanceEffect(idx);
                        effects.Add(track);
                    }
                }
            }
        }

        //控制器类别：地板
        public override int Type { get { return (int)ControllerType.Cell; } }

        
        private int _cost;//移动花费
        private bool _available;//可站立

        [GetProperty]
        public int Cost
        {
            get { return _cost; }
        }

        [GetProperty]
        public bool Available
        {
            get { return _available; }
        }

        //是否在视野内
        [GetProperty]
        public bool Visible
        {
            get
            {
                return mVisibleRoles.Count > 0;
            }
        }

        [GetProperty]
        public Position Position
        {
            get { return mCell.coordinates.ToPosition(); }
        }

        //获取地形id
        [GetProperty]
        public int Terrain
        {
            get
            {
                if (mCell.IsUnderWater || mCell.HasRiver)
                {
                    return 0;
                }
                int tidx = mCell.TerrainTypeIndex;
                //获取特征
                if (mCell.Feature >= 0)
                {
                    int fid = FeatureManager.Instance.GetFeatureId(mCell.Feature);
                    IFeature temp = FeatureManager.Instance.GetFeature(fid);
                    //如果特征覆盖了地形
                    if (temp.OverwriteTerrain)
                    {

                        return fid;
                    }
                }

                //不然就返回地形
                
                return HexGrid.GetTerrainId(tidx);

            }
        }
        //获取起作用的地面类型（地形、特征)
        [GetProperty]
        public TerrainType TerrainType
        {
            get
            {
                int id = Terrain;
                if (id == 0)
                {
                    return TerrainType.Water;
                }
                else if (id > 0 && id <= 1024)
                {
                    return TerrainType.Terrain;
                }
                else
                {
                    return TerrainType.Feature;
                }
            }
        }



        //获取邻居
        public CellController GetNeighbor(HexDirection direction)
        {
            HexCell cell = mCell.GetNeighbor(direction);
            return cell == null ? null : cell.Controller;
        }

        //判断邻居是否可达
        public bool IsReachable(HexDirection direction)
        {
            return mCell.IsReachable(direction);
        }

        public override GameObject GetGameObject()
        {
            return mCell.gameObject;
        }

        public override Position GetPosition()
        {
            return mCell.coordinates.ToPosition();
        }
    }
}
