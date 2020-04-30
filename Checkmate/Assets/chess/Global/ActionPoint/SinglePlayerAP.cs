using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinglePlayerAP : MonoBehaviour
{
    public   int totalAP;
    private  int currentAP;


    public void changeCurrAP(int num)
    {
        currentAP -= num;
        APManager.onAPChanged.Invoke();
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
