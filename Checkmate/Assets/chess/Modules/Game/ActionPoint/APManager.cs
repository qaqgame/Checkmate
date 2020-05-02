using QGF.Common;
using QGF.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APManager : Singleton<APManager>
{
    public static QGFEvent<SinglePlayerAP> onAPChanged;

    public SinglePlayerAP elementAP;
    public void Init()
    {
        elementAP = GameObject.Find("GameManager").GetComponent<SinglePlayerAP>();
        elementAP.Init();
        onAPChanged = new QGFEvent<SinglePlayerAP>();
    }

    public void AddListener(System.Action<SinglePlayerAP> element)
    {
        onAPChanged.AddListener(element);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
