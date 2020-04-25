using Checkmate.Game.Controller;
using Checkmate.Global.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Checkmate.Game.Feature;

namespace Checkmate.Game.Controller
{
    public class CellController :ModelController
    {
        private HexCell mCell;

        private int mRole=-1;//当前的角色

        public HexCell Cell
        {
            get { return mCell; }
        }

        public int Role
        {
            get { return mRole; }
            set { mRole = value; }
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
                    int fid = HexGrid.Features.GetFeatureId(mCell.Feature);
                    IFeature temp = HexGrid.Features.GetFeature(fid);
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
