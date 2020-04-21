using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Modules.Game.Map;
using Checkmate.Modules.Game.Role;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AstarRoute : MonoBehaviour
{
    public int[] directions = {0,1,2,3,4,5};

    public MapManager map;
    

    public List<Position> AstarNavigatorE(RoleController rc, string start, string end)
    {
        // 获取出发点和终止点的Position
        Position starter = Position.Parse(start);
        Position ender = Position.Parse(end);

        // 获取该Position的Cell的CellController
        CellController fromCellCtrl = map.GetCell(starter);
        CellController toCellCtrl = map.GetCell(ender);

        return Astar(rc, fromCellCtrl, toCellCtrl);
    }

    private List<Position> Astar(RoleController rc, CellController fromCellCtrl, CellController toCellCtrl)
    {
        // new一个优先级队列
        HexCellPriorityQueue searchFrontier = new HexCellPriorityQueue();

        List<HexCell> closeList = new List<HexCell>();
        
        // 获取Cell的Position信息
        Position fromPos = fromCellCtrl.Position;
        Position endPos = toCellCtrl.Position;

        AstarCell fromAstarCell = fromCellCtrl.Cell.GetComponent<AstarCell>();
        fromAstarCell.Gvalue = 0;
        fromAstarCell.Cellctrl = fromCellCtrl;
        fromAstarCell.Hvalue = HexMapUtil.GetDistance(fromPos, endPos);

        searchFrontier.Enqueue(fromCellCtrl);
        closeList.Add(fromCellCtrl.Cell);
        while (searchFrontier.Count > 0)
        {
            // 获取队列头的CellController
            CellController currentCtrl = searchFrontier.Dequeue();
            AstarCell currentAstarCell = currentCtrl.Cell.GetComponent<AstarCell>();
            HexCell current = currentCtrl.Cell;

            if (currentCtrl.Cell == toCellCtrl.Cell)
            {
                // retrace path
                // TODO: 是否需要使用AstarCell作为参数
                return RetracePath(currentCtrl.Cell.GetComponent<AstarCell>(), fromCellCtrl.Cell.GetComponent<AstarCell>());
            }
            for (int i = 0; i < 6; i++)
            {
                CellController neighborCtrl = currentCtrl.GetNeighbor((HexDirection)directions[i]);
                AstarCell neighborAstarCell = neighborCtrl.Cell.GetComponent<AstarCell>();
                neighborAstarCell.Cellctrl = neighborCtrl;
                
                if (!current.IsReachable((HexDirection)directions[i]))
                {
                    continue;
                }
                
                TerrainType terrain = neighborCtrl.TerrainType;
                
                int extra = rc.GetExtraMove((int)terrain);

                if (!rc.CanStand((int)terrain))
                {
                    continue;
                }
                

                int tmpGvalue = currentAstarCell.Gvalue + neighborCtrl.Cost;



                if (!closeList.Contains(neighborCtrl.Cell))
                {
                    neighborAstarCell.Gvalue = tmpGvalue;
                    neighborAstarCell.Hvalue = HexMapUtil.GetDistance(neighborCtrl.Position, endPos);
                    neighborAstarCell.pathFrom = currentAstarCell;

                    // neighborCell.mark = (float)(neighborCell.Gvalue - neighborCell.pathFrom.Gvalue) / (float)(rc.role.basicProps.MovingRange + extra);
                    // neighborCell.pathmark = neighborCell.pathFrom.pathmark + neighborCell.mark;
                    // neighborCell.SetLable();

                    searchFrontier.Enqueue(neighborCtrl);
                    closeList.Add(neighborCtrl.Cell);
                }
                else
                {
                    if (tmpGvalue < neighborAstarCell.Gvalue)
                    {
                        int oldFvalue = neighborAstarCell.Fvalue;
                        neighborAstarCell.Gvalue = tmpGvalue;
                        neighborAstarCell.pathFrom = currentAstarCell;

                        // neighborCell.mark = (float)(neighborCell.Gvalue - neighborCell.pathFrom.Gvalue) / (float)(rc.role.basicProps.MovingRange + extra);
                        // neighborCell.pathmark = neighborCell.pathFrom.pathmark + neighborCell.mark;
                        // neighborCell.SetLable();

                        searchFrontier.Change(neighborCtrl, oldFvalue);
                        // change
                    }
                }
            }
        }
        return null;
    }

    private List<Position> RetracePath(AstarCell currentCell, AstarCell fromCell)
    {
        List<Position> path = new List<Position>();
        while (currentCell != fromCell)
        {
            path.Add(currentCell.Cellctrl.Position);
            
            currentCell = currentCell.pathFrom;
        }
        path.Add(fromCell.Cellctrl.Position);

        path.Reverse();
        return path;
    }


    // Start is called before the first frame update
    void Start()
    {
        // Init Map here
        map = GameObject.Find("Map").GetComponent<MapManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
