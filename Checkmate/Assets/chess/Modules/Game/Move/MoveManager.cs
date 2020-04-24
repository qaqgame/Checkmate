using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Role;

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
    public int OperationObjID;
}


public class MoveManager : MonoBehaviour
{
    public static MoveManager instance;
    // private Queue<MoveItem> moveItems = new Queue<MoveItem>();
    private MoveItem moveItem;

    public void Move(string msg)
    {
        // 解析json
        OperateInfo<MoveInfo> Opn = JsonConvert.DeserializeObject<OperateInfo<MoveInfo>>(msg);
        // 获取gameobject
        RoleController rc = RoleManager.Instance.GetRole(Opn.OperationObjID);
        AstarRoute astarRoute = GetComponent<AstarRoute>();

        Opn.OperationCnt.MoveDirection = null;
        List<Position> path = astarRoute.AstarNavigatorE(rc, Opn.OperationCnt.StartPosition, Opn.OperationCnt.EndPosition);
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


        MoveItem moveitem = GetComponent<MoveItem>();
        moveitem.SetUp(rc, path, Opn.OperationCnt.StartPosition, Opn.OperationCnt.EndPosition);
        // 
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

    private void DoMove(Item item)
    {
        if(item != null && item.Path != null)
        {
            moveItem.Travel(item);
        } else
        {
            Debug.LogError("not travel");
        }
    }

    // Test move
    public void TestMoveObj()
    {
        RoleController rc = RoleManager.Instance.GetRole(1);
        string test = "{\"OperationType\":\"Move\",\"OperationCnt\":{\"StartPosition\":\"" + "(2,-1,-1)" + "\",\"MoveDirection\":[0,0,0,2,3],\"EndPosition\":\"" + "(2,2,2)" + "\"},\"OperationObjID\":1}";
        MoveManager.instance.Move(test);
    }

    // Start is called before the first frame update
    void Start()
    {
        // init instance;
        instance = GetComponent<MoveManager>();
        moveItem = GetComponent<MoveItem>();
    }

    // Update is called once per frame
    void Update()
    {
        while (moveItem.moveitems.Count > 0)
        {
            // Debug.LogError("count: " + moveItems.Count);
            Item item = moveItem.moveitems.Dequeue();
            DoMove(item);
        }
    }
}
