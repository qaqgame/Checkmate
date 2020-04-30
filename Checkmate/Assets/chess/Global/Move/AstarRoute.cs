using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Modules.Game.Role;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AstarRoute : MonoBehaviour
{
    public int[] directions = {0,1,2,3,4,5};
    public static HexCellPriorityQueue searchFrontier = new HexCellPriorityQueue();
    public static List<HexCell> closeList = new List<HexCell>();
    

    public List<Position> AstarNavigatorE(RoleController rc, Position start, Position end)
    {
        // 获取该Position的Cell的CellController
        CellController fromCellCtrl = MapManager.Instance.GetCell(start);
        CellController toCellCtrl = MapManager.Instance.GetCell(end);

        return Astar(rc, fromCellCtrl, toCellCtrl);
    }

    private List<Position> Astar(RoleController rc, CellController fromCellCtrl, CellController toCellCtrl)
    {
        // clear
        searchFrontier.Clear();
        closeList.Clear();

        // 获取Cell的Position信息
        Position fromPos = fromCellCtrl.Position;
        Position endPos = toCellCtrl.Position;
        
        AstarCell fromAstarCell = fromCellCtrl.Cell.GetComponent<AstarCell>();
        fromAstarCell.Gvalue = 0;
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
                return RetracePath(currentCtrl, fromCellCtrl);
            }
            for (int i = 0; i < 6; i++)
            {
                if (!current.IsReachable((HexDirection)i))
                {
                    continue;
                }
                CellController neighborCtrl = currentCtrl.GetNeighbor((HexDirection)i);
                AstarCell neighborAstarCell = neighborCtrl.Cell.GetComponent<AstarCell>();
                
                int terrain = neighborCtrl.Terrain;

                int extra = rc.GetExtraMove(terrain);

                if (!rc.CanStand(terrain))
                {
                    continue;
                }
                

                int tmpGvalue = currentAstarCell.Gvalue + neighborCtrl.Cost;



                if (!closeList.Contains(neighborCtrl.Cell))
                {
                    neighborAstarCell.Gvalue = tmpGvalue;
                    neighborAstarCell.Hvalue = HexMapUtil.GetDistance(neighborCtrl.Position, endPos);
                    neighborAstarCell.pathFrom = currentAstarCell;
                    
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
                        
                        searchFrontier.Change(neighborCtrl, oldFvalue);
            
                    }
                }
            }
        }
        return null;
    }

    private List<Position> RetracePath(CellController currentCell, CellController fromCell)
    {
        List<Position> path = new List<Position>();path.Add(currentCell.Position);
        AstarCell cell = currentCell.Cell.GetComponent<AstarCell>();
        while (cell != fromCell.Cell.GetComponent<AstarCell>())
        {
            path.Add(cell.GetComponent<HexCell>().coordinates.ToPosition());
            cell = cell.pathFrom;
        }
        path.Add(fromCell.Position);

        path.Reverse();
        return path;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
