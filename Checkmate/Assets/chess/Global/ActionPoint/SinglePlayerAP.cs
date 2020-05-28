using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerAP
{
    public   int totalAP;
    private  int currentAP;

    public SinglePlayerAP(int total,int current)
    {
        totalAP = total;
        currentAP = current;
    }


    //回复ap
    public void AddCurAp(int num)
    {
        currentAP += num;
    }

    //消耗AP
    public bool ReduceCurAP(int num)
    {
        if (currentAP < num)
        {
            return false;
        }
        currentAP -= num;
        return true;
        
    }

    public void SetCurAP(int num)
    {
        currentAP = num;
    }
    public int GetCurrentAP()
    {
        return currentAP;
    }

    public void Reset(int total,int current)
    {
        totalAP = total;
        currentAP = current;
    }
}
