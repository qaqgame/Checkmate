using Checkmate.Game.Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCellPriorityQueue
{
    //TODO: 是否存储CellController还是其他类型数据



    List<CellController> list = new List<CellController>();
    int count = 0;
    public int Count
    {
        get
        {
            return count;
        }
    }
    int minimum = int.MaxValue;

    public void Enqueue(CellController cell)
    {
        count += 1;
        int priority = cell.Cell.GetComponent<AstarCell>().Fvalue;
        if (priority < minimum)
        {
            minimum = priority;
        }
        // todo
        while (priority >= list.Count)
        {
            list.Add(null);
        }

        if(list[priority] != null)
        {
            cell.Cell.GetComponent<AstarCell>().nextWithSameFvalue = list[priority].Cell.GetComponent<AstarCell>();
        }
        list[priority] = cell;
    }

    public CellController Dequeue()
    {
        count -= 1;
        for (; minimum < list.Count; minimum++)
        {
            CellController cell = list[minimum];
            if (cell != null)
            {
                AstarCell next = cell.Cell.GetComponent<AstarCell>().nextWithSameFvalue;
                if (next == null)
                {
                    list[minimum] = null;
                } else
                {
                    list[minimum] = next.Cellctrl;
                }
                // list[minimum] = cell.Cell.GetComponent<AstarCell>().nextWithSameFvalue.Cellctrl;
                return cell;
            }
        }
        return null;
    }

    public void Change(CellController cell, int oldFvalue)
    {
        CellController current = list[oldFvalue];
        CellController next = current.Cell.GetComponent<AstarCell>().nextWithSameFvalue.Cellctrl;
        if (current == cell)
        {
            list[oldFvalue] = next;
        }
        else
        {
            while (next != cell)
            {
                current = next;
                next = current.Cell.GetComponent<AstarCell>().nextWithSameFvalue.Cellctrl;
            }
            current.Cell.GetComponent<AstarCell>().nextWithSameFvalue = cell.Cell.GetComponent<AstarCell>().nextWithSameFvalue;
        }
        Enqueue(cell);
        count -= 1;
    }

    public void Clear()
    {
        list.Clear();
        count = 0;
        minimum = int.MaxValue;
    }
}
