using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveItem : MonoBehaviour
{
    public string objname;
    public GameObject obj;
    private RoleController rc;
    private string startPos;
    private string endPos;
    private bool IsMoving;     // 该Object的移动状态
    private bool FinishMoving;
    private List<Position> path;      // 该Object的移动路径
    public List<Position> Path
    {
        get;
    }
    private Vector3 NextBlock;         // 下一个方块的世界坐标
    private Position position;       // 下一个方块的方块坐标
    private int count;                 // path的当前index
    private int currIndex;             // object的当前index
    private Dictionary<string, bool> pathStatus = new Dictionary<string, bool>();  // 记录path经过的方块以否已经计算了消耗
    private float travelSpeed;
    private MapManager map;

    public void SetUp(string _objname, List<Position> _path, GameObject _obj, string _startpos, string _endpos, MapManager _map)
    {
        objname = _objname;
        path = _path;
        obj = _obj;
        startPos = _startpos;
        endPos = _endpos;
        currIndex = -1;
        count = 0;
        IsMoving = false;
        FinishMoving = false;
        Position tmp = Position.Parse(_startpos);
        position = new Position(tmp.x, tmp.y, tmp.z);
        travelSpeed = 3f;
        rc = obj.GetComponent<RoleController>();
        map = _map;

        // 准备移动状态
        rc.SetState(RoleState.PreMove);
    }

    public void Travel()
    {
        this.IsMoving = true;
        StartCoroutine(TravelPath());
    }

    IEnumerator TravelPath()
    {
        rc.SetState(RoleState.Move);      // 设置状态为移动中
        for (int i = 1; i < this.path.Count; i++)
        {
            // TODO: 移除当前位置的人物信息 : moveutil.RemoveCurRole()

            Vector3 a = map.GetCellWorldPosition(path[i - 1]);
            Vector3 b = map.GetCellWorldPosition(path[i]);
            for (float t = 0f; t < 1f; t += Time.deltaTime * travelSpeed)
            {
                obj.transform.position = Vector3.Lerp(a, b, t);
                yield return null;
            }
            // TODO: 是否需要在移动时进行行动点消耗和战争迷雾的刷新
            rc.Position = path[i];
        }
        // 移动结束
        rc.SetState(RoleState.EndMove);
        this.IsMoving = false;
        // 恢复为Idle状态
        rc.SetState(RoleState.Idle);
    }

    // TODO: 是否还需要进行移动点消耗的计算
    public void DoApCost(Position _pos)
    {
       
        return;
    }

    // TODO: 是否还需要进行迷雾的刷新
    public void FreshFog(int count)
    {
        Position old = this.path[count - 1];
        Position now = this.path[count];
        //this.rc.RefreshVisibility(old, now);
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
