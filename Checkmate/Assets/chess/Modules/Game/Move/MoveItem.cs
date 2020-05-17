using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Utils;
using QGF.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public string objname;
    public RoleController rc
    {
        get;
        private set;
    }
    public string startPos {
        get;
        private set;
    }
    public string endPos
    {
        get;
        private set;
    }
    private List<Position> path;      // 该Object的移动路径
    public List<Position> Path
    {
        get
        {
            return path;
        }
    }
    private Position position;       // 下一个方块的方块坐标
    private Dictionary<string, bool> pathStatus = new Dictionary<string, bool>();  // 记录path经过的方块以否已经计算了消耗
    public float travelSpeed
    {
        get;
        private set;
    }

    public Item(RoleController _rc, List<Position> _path, string _startpos, string _endpos)
    {
        objname = _rc.Name;
        path = _path;
        startPos = _startpos;
        endPos = _endpos;
        Position tmp = Position.Parse(_startpos);
        position = new Position(tmp.x, tmp.y, tmp.z);
        travelSpeed = 3f;
        rc = _rc;

        // 准备移动状态
        // rc.SetState(RoleState.PreMove);
    }
}

public class MoveItem : MonoBehaviour
{
    public static bool IsMoving = false;
    // 移动队列
    public Queue<Item> moveitems = new Queue<Item>();

    public void SetUp(RoleController _rc, List<Position> _path, string _startpos, string _endpos)
    {
        Item item = new Item(_rc, _path, _startpos, _endpos);
        moveitems.Enqueue(item);
    }

    public void Travel(Item item)
    {
        // this.IsMoving = true;
        StartCoroutine(TravelPath(item));
    }

    IEnumerator TravelPath(Item item)
    {
        IsMoving = true;
        item.rc.SetState(RoleState.Move);      // 设置状态为移动中
        for (int i = 1; i < item.Path.Count; i++)
        {
            MapManager.Instance.GetCell(item.rc.Position).SetVisibility(item.rc);
            MapManager.Instance.GetCell(item.rc.Position).Role = -1;
            while(GameExecuteManager.Instance.WaitForExecute)
            {
                Debug.LogError("Wait for a while1");
                yield return null;
            }

            


            // TODO: 移除当前位置的人物信息 : moveutil.RemoveCurRole()
            Vector3 a = MapManager.Instance.GetCellWorldPosition(item.Path[i - 1]);
            Vector3 b = MapManager.Instance.GetCellWorldPosition(item.Path[i]);
            for (float t = 0f; t < 1f; t += Time.deltaTime * item.travelSpeed)
            {
                item.rc.GetGameObject().transform.position = Vector3.Lerp(a, b, t);
                yield return null;
            }
            MapManager.Instance.GetCell(item.rc.Position).RemoveVisibility(item.rc);

            item.rc.Position = item.Path[i];
            
            MapManager.Instance.GetCell(item.rc.Position).SetVisibility(item.rc);
            MapManager.Instance.GetCell(item.rc.Position).Role = item.rc.RoleId;
            while (GameExecuteManager.Instance.WaitForExecute)
            {
                Debug.LogError("Wait for a while2");
                yield return null;
            }
        }
        // 移动结束
        item.rc.SetState(RoleState.EndMove);
        // this.IsMoving = false;
        // 恢复为Idle状态
        item.rc.SetState(RoleState.Idle);
        IsMoving = false;
    }

    // TODO: 是否还需要进行移动点消耗的计算
    public void DoApCost(Position _pos)
    {
       
        return;
    }

    // TODO: 是否还需要进行迷雾的刷新
    public void FreshFog(int count)
    {
        return;
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
