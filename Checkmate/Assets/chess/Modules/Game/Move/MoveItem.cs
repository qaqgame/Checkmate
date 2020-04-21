using Checkmate.Game;
using Checkmate.Game.Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveItem : MonoBehaviour
{
    public string objname;
    public GameObject obj;
    public RoleController rc;
    public string startPos;
    public string endPos;
    public bool IsMoving;     // 该Object的移动状态
    public bool FinishMoving;
    public List<Position> path;      // 该Object的移动路径
    public Vector3 NextBlock;         // 下一个方块的世界坐标
    public Position position;       // 下一个方块的方块坐标
    public int count;                 // path的当前index
    public int currIndex;             // object的当前index
    public Dictionary<string, bool> pathStatus = new Dictionary<string, bool>();  // 记录path经过的方块以否已经计算了消耗
    public float travelSpeed;

    public void SetUp(string _objname, List<Position> _path, GameObject _obj, string _startpos, string _endpos)
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
    }

    public void Travel()
    {
        this.IsMoving = true;
        StartCoroutine(TravelPath());
    }

    IEnumerator TravelPath()
    {
        yield return 0;
    }


    public void DoApCost(Position _pos)
    {
       
        return;
    }

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
