using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Game.UI
{
    public class RolePanel
    {
        private GProgressBar mHpBar;//血条槽
        private GImage mDownHPBar;//血条底槽

        private GList mBuffList;//buff栏
        private GTextField mName;//姓名栏



        public RolePanel(UIPanel root,int maxHP,int curHP,string name)
        {
            GComponent comRoot = root.ui;
            
            mHpBar = comRoot.GetChildByPath("HPPanel.HP").asProgress;

            mDownHPBar = mHpBar.GetChild("DownBar").asImage;
            mHpBar.max = maxHP;
            mHpBar.value = curHP;
            mDownHPBar.scaleX = curHP / (float)maxHP;

            mBuffList = comRoot.GetChild("BuffList").asList;

            mName = comRoot.GetChild("Title").asTextField;
            mName.text = name;
            
        }

        public void SetHP(int current)
        {
            if (current < mHpBar.value)
            {
                mHpBar.value = current;
                mDownHPBar.TweenScaleX(current / (float)mHpBar.max, 0.5f);
            }
            else if (current > mHpBar.value)
            {
                mHpBar.value = current;
                mDownHPBar.scaleX = current / (float)mHpBar.max;
            }
        }


    }
}
