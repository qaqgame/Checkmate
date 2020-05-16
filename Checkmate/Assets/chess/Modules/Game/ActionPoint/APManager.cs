using Checkmate.Game.Player;
using QGF.Common;
using QGF.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APManager : Singleton<APManager>
{
    private static QGFEvent onAPChanged;

    public Dictionary<int,SinglePlayerAP> elementAP;
    public void Init()
    {
        elementAP = new Dictionary<int, SinglePlayerAP>();
        onAPChanged = new QGFEvent();
    }

    public void Reset()
    {
        foreach(uint pid in PlayerManager.Instance.GetAllPlayers())
        {
            int id = (int)pid;
            if (!elementAP.ContainsKey(id))
            {
                elementAP.Add(id, new SinglePlayerAP(20, 20));
            }
            else
            {
                elementAP[id].Reset(20, 20);
            }
        }
    }

    public void AddListener(System.Action element)
    {
        onAPChanged.AddListener(element);
    }

    public void ReduceAp(int uid,int num)
    {
        elementAP[uid].ReduceCurAP(num);
        if (uid == PlayerManager.Instance.PID)
        {
            onAPChanged.Invoke();
        }
    }

    public int GetCurAP()
    {
        int uid = (int)PlayerManager.Instance.PID;
        return elementAP[uid].GetCurrentAP();
    }

    public int GetMaxAp()
    {
        int uid = (int)PlayerManager.Instance.PID;
        return elementAP[uid].totalAP;
    }
}
