using Checkmate.Game;
using Checkmate.Game.Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarCell : MonoBehaviour
{
    // Astar算法的G,H,F值。
    public CellController Cellctrl;
    public int Gvalue; 
    public int Hvalue;
    public int Fvalue {
        get {
            return Gvalue + Hvalue;
        }
    }
    private int distance = 0;
    public int Distance {
        get {
            return distance;
        }
        set {
            distance = value;
        }
    }
    public float pathmark = 0f; // 从出发地到此地的float类型的消耗
    public float mark = 0f;  // 当前格子需要的消耗
    public int label = 0;    // 从出发地到达此地的int类型消耗
    public AstarCell nextWithSameFvalue{get;set;}
    public AstarCell pathFrom;  // 指向前一位置

    public AstarCell(CellController _cellctrl, int gvalue, int hvalue)
    {
        Cellctrl = _cellctrl;
        Gvalue = gvalue;
        Hvalue = hvalue;
    }


    public void SetLable() {
        int tmp;
        if(pathFrom == null) {
            // TODO:
            //tmp = ActionPointPanel.playerAPOriginal.currentAP;
        } else {
            // TODO:
            //tmp = ActionPointPanel.playerAPOriginal.currentAP - pathFrom.label;
        }
        // int tmp1 = (int)(ActionPointPanel.playerAPOriginal.currentAPFloat - pathmark);
        if(pathFrom == null) {
            // label = tmp - tmp1;
            return;
        }
        // label = pathFrom.label + tmp - tmp1;
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
