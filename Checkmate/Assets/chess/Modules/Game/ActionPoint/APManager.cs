using Checkmate.Game.Player;
using QGF;
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
        Reset();
        onAPChanged = new QGFEvent();
    }

    public void Clear()
    {
        elementAP.Clear();
        onAPChanged.RemoveAllListeners();
    }

    public void Reset()
    {
        foreach(uint pid in PlayerManager.Instance.GetAllPlayers())
        {
            int id = (int)pid;
            if (!elementAP.ContainsKey(id))
            {
                elementAP.Add(id, new SinglePlayerAP(10, 10));
            }
            else
            {
                elementAP[id].Reset(10, 10);
            }
        }
        if (onAPChanged != null)
        {
            onAPChanged.Invoke();
        }
    }

    public void AddListener(System.Action element)
    {
        onAPChanged.AddListener(element);
    }

    public bool ReduceAp(int uid,int num)
    {
        if (elementAP[uid].ReduceCurAP(num))
        {
            Debuger.Log("reduce ap:{0}, {1}", uid, num);
            if (uid == PlayerManager.Instance.PID)
            {
                onAPChanged.Invoke();
            }
            return true;
        }
        return false;
    }

    public void SetAP(int uid,int num)
    {
        elementAP[uid].SetCurAP(num);
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

    public int GetCurAP(int pid)
    {
        return elementAP[pid].GetCurrentAP();
    }

    public int GetMaxAp()
    {
        int uid = (int)PlayerManager.Instance.PID;
        return elementAP[uid].totalAP;
    }
}
