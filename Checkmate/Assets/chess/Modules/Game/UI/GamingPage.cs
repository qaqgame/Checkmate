﻿using Checkmate.Game.Controller;
using FairyGUI;
using QGF.Unity.FGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamingPage : FGUIWindow
{
    // 主面板
    private GComponent mainPanel = new GComponent();

    GComponent map;
    GComponent apPoints;
    GComponent gameState;
    GComponent roleIcon;
    GComponent skillList;
    GComponent propertyList;
    GComponent buffList;

    public static void LoadPage()
    {
        FGUIManager.Instance.Open<GamingPage>("MainPage", "GamingPage",null);
    }

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
        //---------添加初始技能按钮
        skillList = mCtrlTarget.GetChildByPath("SkillList").asCom;
        GList skills = skillList.GetChild("Skills").asList;
        for (int i = 0; i < 3; i++)
        {
            createSkillButton(skills);
        }
        //------------------------

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
        // 遍历属性，创建Property栏目并加入到List中去
    }

    // 修改一个具体的属性框中的属性名和属性值
    private void SetSinglePropertyV(GComponent SingleProperty, string propertyName, string propertyValue)
    {

    }
}