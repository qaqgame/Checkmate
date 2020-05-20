using QGF.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : Singleton<MiniMap>
{
    GameObject camera = null;
    Plane showingWindow;
    public void Init()
    {
        camera = new GameObject("mapCamera");
        camera.AddComponent<Camera>();
        camera.transform.position = new Vector3(50, 200, 50);
        camera.transform.SetRotationX(90);

        camera.GetComponent<Camera>().targetTexture = Resources.Load<RenderTexture>("Assets/chess/Modules/Game/MiniMap/MiniMapTexture.renderTexture");

        GameObject newCanvas = new GameObject("MapCanvas");
        Canvas c = newCanvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        newCanvas.AddComponent<CanvasScaler>();
        newCanvas.AddComponent<GraphicRaycaster>();
        GameObject panel = new GameObject("MapPanel");
        panel.AddComponent<CanvasRenderer>();
        Image i = panel.AddComponent<Image>();
        panel.transform.SetParent(newCanvas.transform, false);

        panel.GetComponent<Image>().material = Resources.Load<Material>("Assets/chess/Modules/Game/MiniMap/MiniMapMaterial.mat");
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
