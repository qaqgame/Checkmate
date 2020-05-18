using QGF.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : Singleton<MiniMap>
{
    GameObject camera = null;
    public void Init()
    {
        camera = new GameObject("mapCamera");
        camera.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, 0);
        camera.transform.rotation = new Quaternion(90, 0, 0, 0);

        camera.GetComponent<Camera>().targetTexture = Resources.Load<RenderTexture>("Assets/chess/Modules/Game/MiniMap/MiniMapTexture.renderTexture");

        // 创建Plane
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
