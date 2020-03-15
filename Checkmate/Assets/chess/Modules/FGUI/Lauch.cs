using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
public class Lauch : MonoBehaviour
{
    GComponent mRoot;
    GTextInput mActInput, mPswInput;
    GButton mConfirmBtn;
    // Start is called before the first frame update
    void Start()
    {
        UIPackage.AddPackage("ui/FGUI/Login");

        UIPanel panel = gameObject.AddComponent<UIPanel>();
        panel.packageName = "CommonPackage";
        panel.componentName = "LoginPanel";

        //设置renderMode的方式
        panel.container.renderMode = RenderMode.ScreenSpaceOverlay;
        //设置fairyBatching的方式
        panel.container.fairyBatching = true;
        //设置sortingOrder的方式
        panel.SetSortingOrder(1, true);
        //设置hitTestMode的方式
        panel.SetHitTestMode(HitTestMode.Default);
        panel.fitScreen = FitScreen.FitSize;
        //最后，创建出UI
        panel.CreateUI();

        mRoot = panel.ui;
        mActInput = mRoot.GetChild("InputField").asCom.GetChild("n1").asCom.GetChild("n5").asTextInput;
        mPswInput = mRoot.GetChild("InputField").asCom.GetChild("n3").asCom.GetChild("n5").asTextInput;
        mConfirmBtn = mRoot.GetChild("BtnField").asCom.GetChild("n0").asButton;


        mConfirmBtn.onClick.Add(OnClickConfirm);
    }

    void OnClickConfirm()
    {
        Debug.Log("act:" + mActInput.text + "\n psw:" + mPswInput.text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
