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

    public void changeCurrAP(int num)
    {
        currentAP -= num;
        APManager.onAPChanged.Invoke();
    }
}
