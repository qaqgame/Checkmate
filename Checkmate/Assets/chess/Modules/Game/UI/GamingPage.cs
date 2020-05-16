using Checkmate.Game.Controller;
using FairyGUI;
using QGF.Unity.FGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamingPage : FGUIPage
{
    // 主面板
    private GComponent mainPanel = new GComponent();

    GComponent map;
    GComponent gameState;

    GComponent mRolePanel;//角色面板根节点
    GComponent roleIcon;
    GComponent skillList;
    GComponent propertyList;
    GList propertyShow;
    GList propertyHide;
    GComponent buffList;

    GButton mRoundEndBtn;//回合结束按钮
    public Action onRoundEnd;//回合结束时调用 

    GComponent mAPRoot;//AP的UI根节点
    GTextField mCurAP;//当前行动点

    private GButton createSkillButton(GList SkillList ,string url = null)
    {   
        GButton gButton = SkillList.AddItemFromPool("ui://nksqv53jntn1p").asButton;
        if (url != null)
        {
            GLoader loader = gButton.GetChild("SkillIconLoader").asLoader;
            loader.url = url;
        }
        return gButton;
    }
    protected override void OnLoad()
    {
        base.OnLoad();
        //todo
        mRolePanel = mCtrlTarget.GetChild("RolePanel").asCom;
        propertyList = mRolePanel.GetChildByPath("PropertyList").asCom;
        propertyShow = propertyList.GetChild("ShowingProperty").asList;
        propertyHide = propertyList.GetChild("HidenProperty").asList;

        string[] tmp = new string[5]{"HP","MP","Attack","PRejection","MRejection"};

        for (int i = 0; i < 5; i++)
        {
            propertyShow.GetChildAt(i).asCom.GetChild("PropertyName").asTextField.text = tmp[i];
        }
        
        //---------添加初始技能按钮
        skillList = mRolePanel.GetChildByPath("SkillList").asCom;
        GList skills = skillList.GetChild("Skills").asList;
        for (int i = 0; i < 3; i++)
        {
            createSkillButton(skills);
        }

        mRolePanel.visible = false;
        //------------------------
        //获取行动点显示
        mAPRoot = mCtrlTarget.GetChild("APPoint").asCom;
        mCurAP = mAPRoot.GetChild("CurrAP").asTextField;

        //==================
        //获取回合结束按钮
        mRoundEndBtn = mCtrlTarget.GetChild("RoundEndBtn").asButton;
        //设置点击事件
        mRoundEndBtn.onClick.Add(OnRoundEndClicked);
    }

    protected override void OnOpen(object arg)
    {
        base.OnOpen(arg);
        // 
    }

    protected override void OnPanelDestroy()
    {
        base.OnPanelDestroy();
        //
    }
    //===============================================
    //外部接口
    /// <summary>
    /// 隐藏所有可隐藏项
    /// </summary>
    public void HideAll()
    {
        mRoundEndBtn.visible = false;
        mAPRoot.visible = false;
    }
    /// <summary>
    /// 显示所有可显示项
    /// </summary>
    public void ShowAll()
    {
        mRoundEndBtn.visible = true;
        mAPRoot.visible = true;
    }

    /// <summary>
    /// 显示角色面板
    /// </summary>
    public void ShowRole()
    {
        mRolePanel.visible = true;
    }
    /// <summary>
    /// 隐藏角色面板
    /// </summary>
    public void HideRole()
    {
        mRolePanel.visible = false;
    }

    public void UpdateAP(int curAP,int maxAP)
    {
        mCurAP.text = curAP.ToString();
    }
    public void UpdateRoleInfo(RoleController rc)
    {

        //----------更新技能图标------------

        //----------更新人物属性------------
        UpdateRoleProperty(rc.Current);
        //----------更新地图----------------

        //----------更新人物图标Icon--------
    }

    public void UpdateGameInfo(APManager aPManager)
    {
        //----------更新行动力--------------

        //----------更新游戏当前信息--------
    }

    // 修改角色属性部分的信息
    private void UpdateRoleProperty(RoleAttributeController roleAttributeController)
    {
        propertyList = mCtrlTarget.GetChildByPath("PropertyList").asCom;
        GList alwaysShowList = propertyList.GetChild("ShowingProperty").asList;
        GList onOverShowList = propertyList.GetChild("HidenProperty").asList;
    }

    

    // 修改一个具体的属性框中的属性名和属性值
    private GComponent AlterSinglePropertyTo(GList target,string propertyName, string propertyValue)
    {
        return null;
    }


    //回合结束事件
    private void OnRoundEndClicked()
    {
        if (onRoundEnd != null)
        {
            onRoundEnd.Invoke();
        }
    }

}
