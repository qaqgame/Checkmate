using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Effect;
using Checkmate.Game.Feature;
using Checkmate.Game.Utils;
using Checkmate.Global.Data;
using QGF;
using QGF.Codec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Modules.Game.Map
{
    public class MapManager
    {
        private HexGrid hexGrid;


        public FeatureManager Features
        {
            get
            {
                return hexGrid.Features;
            }
        }

        public EffectManager Effects
        {
            get
            {
                return hexGrid.Effects;
            }
        }

        //加载指定位置的地图文件
        public bool Init(HexGrid grid,string mapPath)
        {
            hexGrid = grid;
            byte[] bytes = File.ReadAllBytes(mapPath);
            if (bytes.Length <= 0)
            {
                Debuger.LogError("read map file:{0} error!", "testMap");
                return false;
            }
            MapData data = PBSerializer.NDeserialize<MapData>(bytes);
            if (data.version != "1.0")
            {
                Debuger.LogWarning("map version error!");
            }
            //加载网格数据(用于显示)
            hexGrid.Load(data);
            
            
            return true;
        }

        //获取方块的世界坐标
        public Vector3 GetCellWorldPosition(Position cellPos)
        {
            return hexGrid.GetCell(cellPos).transform.localPosition;
        }

        //获取逻辑坐标的单元格
        public CellController GetCell(Position position)
        {
            return hexGrid.GetCell(position).Controller;
        }

        //获取世界坐标的单元格
        public CellController GetCell(Vector3 position)
        {
            return hexGrid.GetCell(position).Controller;
        }

        //获取单元格列表
        public List<CellController> GetCells(List<Position> positions)
        {
            List<CellController> result = new List<CellController>();
            foreach(var p in positions)
            {
                HexCell cell = hexGrid.GetCell(p);
                if (cell != null)
                {
                    result.Add(cell.Controller);
                }
            }
            return result;
        }
        
        //通过范围获取单元格列表
        public List<CellController> GetCells(IRange range,Position start,Position center)
        {
            List<Position> positions = range.GetResult(start, center);
            return GetCells(positions);
        }
    }
}
