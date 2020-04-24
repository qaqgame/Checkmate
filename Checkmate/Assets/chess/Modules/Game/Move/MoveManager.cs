using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Map;

// 操作消息中的移动消息
public struct MoveInfo
{
    public string StartPosition;
    public int[] MoveDirection;
    public string EndPosition;
}

// 发送过来的操作消息
public struct OperateInfo<T>
{
    public string OperationType;
    public T OperationCnt;
    public string OperationObj;
}


public class MoveManager : MonoBehaviour
{
    public static MoveManager instance;
    public MapManager map;
    private Queue<MoveItem> moveItems = new Queue<MoveItem>();

    public void Move(string msg)
    {
        // 解析json
        OperateInfo<MoveInfo> Opn = JsonConvert.DeserializeObject<OperateInfo<MoveInfo>>(msg);
        string objname = Opn.OperationObj;
        // 获取gameobject
        GameObject obj = GameObject.Find(objname);
        AstarRoute astarRoute = GetComponent<AstarRoute>();

        Opn.OperationCnt.MoveDirection = null;
        List<Position> path = astarRoute.AstarNavigatorE(obj.GetComponent<RoleController>(), Opn.OperationCnt.StartPosition, Opn.OperationCnt.EndPosition);
        if (path == null)
        {
            Debug.LogError("path is null");
        }

        // TODO:
        /*int tmpPathCost = ActionPointPanel.CalAPCost(obj.GetComponent<RoleController>(), path);
        if (tmpPathCost >= ActionPointPanel.playerAPOriginal.currentAP)
        {
            Opn.OperationCnt.EndPosition = MoveRangePath(path, obj);
        }*/


        MoveItem moveItem = new MoveItem();
        moveItem.SetUp(objname, path, obj, Opn.OperationCnt.StartPosition, Opn.OperationCnt.EndPosition,map);

        // 
        moveItems.Enqueue(moveItem);

    }

    public string MoveRangePath(List<Position> path, GameObject obj)
    {
        // 获取rolecontroller
        RoleController role = obj.GetComponent<RoleController>();

        // TODO:
        /*int moverange = ActionPointPanel.playerAPOriginal.currentAP;
        float moverangef = ActionPointPanel.playerAPOriginal.currentAPFloat;
        int pathLen = path.Count;
        int pathIndex = 1;
        for (; pathIndex < pathLen; pathIndex++)
        {
            int cost = GetSubOfTwoAstarCell(path[pathIndex]);
            // Debug.LogError("cost: "+cost);
            moverange -= cost;
            moverangef -= Map.instance.GetCell(path[pathIndex]).GetComponent<AstarCell>().mark;
            if (moverange < 0 || moverangef < 0)
            {
                break;
            }
        }
        // Debug.LogError("pathIndex: "+pathIndex+" pathLen: "+pathLen);
        if (pathIndex < pathLen)
        {
            path.RemoveRange(pathIndex, pathLen - pathIndex);
        }
        Position end;
        if (pathIndex > 0)
        {
            end = path[pathIndex - 1];
        }
        else
        {
            path = null;
            end = role.role.tempAttrs.C_Pos;
        }
        return end.ToString();
        */
        return null;
    }

    private void DoMove(MoveItem item)
    {
        if(item && item.Path != null)
        {
            item.Travel();
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        // init instance;
        instance = GetComponent<MoveManager>();
        // map = GetComponent<MapManager>();
        // TODO: init Map
    }

    // Update is called once per frame
    void Update()
    {
        while (moveItems.Count > 0)
        {
            MoveItem item = moveItems.Dequeue();
            DoMove(item);
        }
    }
}
