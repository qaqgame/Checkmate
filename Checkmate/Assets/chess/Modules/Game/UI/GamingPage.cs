using Checkmate.Game.Controller;
using Checkmate.Game.Player;
using Checkmate.Game.UI.Component;
using Checkmate.Modules.Game.UI.Component;
using FairyGUI;
using QGF.Unity.FGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkmate.Modules.Game.UI
{
    public class GamingPage : FGUIPage
    {
        GComponent mRolePanel;//角色面板根节点
        RoleDataPanel mRolePanelUI;//角色面板

        GButton mRoundEndBtn;//回合结束按钮
        public Action onRoundEnd;//回合结束时调用 

        GComponent mAPRoot;//AP的UI根节点
        GTextField mCurAP;//当前行动点

        GComponent mTipsPanel;//提示ui
        TipsPanel mTips;//提示ui类

        PlayerList mCurPlayers, mNextPlayers;

        Transition mTipsIn, mTipsOut;//提示界面进出动画
        protected override void OnLoad()
        {
            base.OnLoad();
            //todo
            mRolePanel = mCtrlTarget.GetChild("RolePanel").asCom;
            mRolePanelUI = new RoleDataPanel(mRolePanel);

            mRolePanelUI.Visible = false;
            mRolePanelUI.onEffectHover = OnEffectHover;
            mRolePanelUI.onEffectOut = OnEffectOut;

            mTipsPanel = mCtrlTarget.GetChild("Tips").asCom;
            mTips = new TipsPanel(mTipsPanel);
            //mTips.Hide();

            mTipsIn = mCtrlTarget.GetTransition("TipsIn");
            mTipsOut = mCtrlTarget.GetTransition("TipsOut");
            //------------------------
            //获取行动点显示
            mAPRoot = mCtrlTarget.GetChild("APPoint").asCom;
            mCurAP = mAPRoot.GetChild("CurrAP").asTextField;

            //---------------------
            //获取玩家显示
            GList temp = mCtrlTarget.GetChildByPath("CurrentQueue.List").asList;
            mCurPlayers = new PlayerList(temp,true);

            temp = mCtrlTarget.GetChildByPath("NextQueue.List").asList;
            mNextPlayers = new PlayerList(temp,false);

            //==================
            //获取回合结束按钮
            mRoundEndBtn = mCtrlTarget.GetChild("RoundEndBtn").asButton;
            //设置点击事件
            mRoundEndBtn.onClick.Add(OnRoundEndClicked);

            HideAll();
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
        public void ShowRole(RoleController role)
        {
            mRolePanelUI.Visible = true;
            mRolePanelUI.ShowRole(role);
        }
        /// <summary>
        /// 隐藏角色面板
        /// </summary>
        public void HideRole()
        {
            mRolePanelUI.Visible = false;
            mRolePanelUI.Clear();
        }

        public void ShowMore()
        {
            if (mRolePanelUI.Visible)
            {
                mRolePanelUI.ShowMoreProp();
            }
        }

        public void HideMore()
        {
            mRolePanelUI.HideMoreProp();
        }

        public void UpdateAP(int curAP, int maxAP)
        {
            mCurAP.text = curAP.ToString();
        }

        //更新玩家列表
        public void UpdatePlayers(uint player)
        {
            mCurPlayers.Remove((int)player);
            mNextPlayers.Add((int)player);
        }

        public void ResetPlayers()
        {
            mCurPlayers.RemoveAll();
            foreach(var pid in PlayerManager.Instance.GetAllPlayers())
            {
                int id = (int)pid;
                mCurPlayers.Add(id);
            }
            mNextPlayers.RemoveAll();
        }

        //说明界面的api
        public void ShowTips(string icon,string text)
        {
            mTips.SetIcon(icon);
            mTips.SetText(text);
            if (!mTipsPanel.visible)
            {
                mTips.Show();
                mTipsIn.Play();
            }
        }

        public void HideTips()
        {
            if (mTipsPanel.visible)
            {
                mTipsOut.Play(() => { mTips.Hide(); });
            }
        }

        private void OnEffectHover(EffectController effect)
        {
            ShowTips(effect.GetIcon(), effect.GetString());
        }

        private void OnEffectOut()
        {
            //HideTips();
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
}