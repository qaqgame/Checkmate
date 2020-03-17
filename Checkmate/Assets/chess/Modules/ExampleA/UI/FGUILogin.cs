using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QGF.Unity.FGUI;
using FairyGUI;
using QGF;
using QGF.Network.General.Client;
using Checkmate.Global.Proto;
public class FGUILoginPage : FGUIPage
{
    GTextInput mActInput, mPswInput;
    GButton mBtnLogin;
    NetManager mNet;
    int cnt = 0;
    public void Init(NetManager manager)
    {
        mNet = manager;
    }
    protected override void OnLoad()
    {
        Debuger.Log("login panel loaded suc!");
        base.OnLoad();
        mActInput = mCtrlTarget.GetChildByPath("InputField.actInput.input").asTextInput;
        mPswInput = mCtrlTarget.GetChildByPath("InputField.pswInput.input").asTextInput;
        mBtnLogin = mCtrlTarget.GetChildByPath("BtnField.btnLogin").asButton;
        mBtnLogin.onClick.Add(OnLoginBtnClicked);
    }

    private void OnLoginBtnClicked()
    {
        LoginProto proto = new LoginProto();
        proto.id = cnt++;
        proto.name = "test" + cnt.ToString();
        int len=mNet.Send<LoginProto, LoginRsp>(1,proto, OnResponse,30, OnErr);
        Debuger.Log("btn login clicked, msglen:{0}",len);
    }

    private void OnResponse(LoginRsp rsp)
    {
        Debuger.LogWarning("success get response:ret:{0}, msg:{1}", rsp.ret,rsp.msg);
    }

    private void OnErr(int err)
    {
        Debuger.LogError("error net code:{0}", err);
    }
}
