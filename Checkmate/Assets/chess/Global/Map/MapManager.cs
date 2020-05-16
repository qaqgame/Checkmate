using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Effect;
using Checkmate.Game.Feature;
using Checkmate.Game.Utils;
using Checkmate.Global.Data;
using QGF;
using QGF.Codec;
using QGF.Common;
using QGF.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Game.Map
{
    public class MapManager:Singleton<MapManager>
    {
        private HexGrid hexGrid;


        //加载指定位置的地图文件
        public bool Init(HexGrid grid,string mapPath)
        {
            hexGrid = grid;
            byte[] bytes = FileUtils.ReadFile(mapPath);
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
            HexCell cell = hexGrid.GetCell(position);
            return cell == null ? null : cell.Controller;
        }

        //获取世界坐标的单元格
        public CellController GetCell(Vector3 position)
        {
            HexCell cell = hexGrid.GetCell(position);
            return cell == null ? null : cell.Controller;
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

        //获取视野范围内的方格
        public List<CellController> GetVisibleCells(RoleController role)
        {
            int viewRange = role.Current.ViewRange;
            int viewHeight = role.Current.ViewHeight;

            Position center = role.Position;

            List<CellController> result = new List<CellController>();

            //获取范围内的坐标
            List<Position> temp = HexMapUtil.GetSingleRing(center, viewRange);

            //遍历连线进行搜索
            foreach(Position border in temp)
            {
                //获取连线
                List<Position> tempLine = HexMapUtil.GetLine(center, border);
                int maxCost = viewRange;
                //获取可见的最大高度
                int maxHeight = GetCell(tempLine[0]).Cell.Elevation + viewHeight;

                //大于1时，沿该线遍历
                if (tempLine.Count > 1)
                {
                    int cost = 1;
                    foreach(Position pos in tempLine)
                    {
                        CellController cell = GetCell(pos);
                        //无单元格或海拔过高则结束
                        if (cell == null || cell.Cell.Elevation > maxHeight+1)
                        {
                            break;
                        }

                        if (cell.Cell.Elevation - maxHeight == 1)
                        {
                            cost = 2;
                        }

                        //判断是否够cost
                        if (maxCost < cost)
                        {
                            //不够结束
                            break;
                        }

                        //不存在则加入
                        if (!result.Contains(cell))
                        {
                            result.Add(cell);
                            maxCost -= cost;
                        }
                    }
                }
            }
            return result;
        }

        //提高可见度
        public void IncreaseVisibility(RoleController role)
        {
            List<CellController> cells = GetVisibleCells(role);
            foreach(var cell in cells)
            {
                cell.SetVisibleRole(role.RoleId);
                cell.Cell.IncreaseVisibility();
            }
        }

        //减少可见度
        public void DecreaseVisibility(RoleController role)
        {
            List<CellController> cells = GetVisibleCells(role);
            foreach (var cell in cells)
            {
                cell.RemoveVisibleRole(role.RoleId);
                if (!cell.Visible)
                {
                    cell.Cell.DecreaseVisibility();
                }
            }
        }
    }
}
