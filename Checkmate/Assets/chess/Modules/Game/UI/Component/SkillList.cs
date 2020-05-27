using Checkmate.Game.Controller;
using Checkmate.Game.Skill;
using FairyGUI;
using QGF.Unity.FGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Modules.Game.UI.Component
{
    public class SkillList:VirtualList
    {
        private List<int> mSkills;//所有技能实例id

        public Action<int> onSkillClicked=null;//技能点击事件
        public Action<EffectController> onTouchBegin;
        public Action onTouchEnd;
        public SkillList(GList list) : base(list, 0)
        {
            onRenderItem = RenderItem;
            onItemClicked = OnClicked;
            mList.onRightClickItem.Add(OnRightClick);
        }

        public void Clear()
        {
            onRenderItem = null;
            onItemClicked = null;
            mList.onRollOver.Clear();
            mList.onRollOut.Clear();
        }

        void RenderItem(int itemIdx, int childIdx, GObject obj)
        {
            //获取组件
            GComponent com = obj.asCom;
            GLoader icon = com.GetChild("Icon").asLoader;
            GImage mask = com.GetChild("Mask").asImage;
            GTextField count = com.GetChild("Text").asTextField;

            BaseSkill skill = SkillManager.Instance.GetInstance(mSkills[itemIdx]);

            if (skill != null)
            {
                

                Texture2D tex = SkillManager.Instance.GetIcon(mSkills[itemIdx]);
                //赋予主图片
                icon.texture = new NTexture(tex);
                //设置文字
                if (skill.CoolTurns < skill.MaxCool)
                {
                    count.visible = true;
                    count.text = (skill.MaxCool - skill.CoolTurns).ToString();
                }
                else
                {
                    count.visible = false;
                }
                //设置遮罩
                float amt = 1.0f-(skill.CoolTurns / (float)skill.MaxCool);
                mask.fillAmount = amt;
            }
        }

        public void Update(List<int> skills)
        {
            mSkills = skills;
            Refresh(mSkills.Count);
        }

        private void OnClicked(int itemIdx, int childIdx, GObject obj)
        {
            int skillId = mSkills[itemIdx];
            BaseSkill skill = SkillManager.Instance.GetInstance(skillId);
            if (onSkillClicked != null&&(skill!=null&&skill.CoolTurns==skill.MaxCool))
            {
                onSkillClicked.Invoke(skillId);
            }
        }

        private void OnRightClick(EventContext context)
        {
            GObject item = context.data as GObject;
            int index = mList.GetChildIndex(item);
            int selIdx = mList.ChildIndexToItemIndex(index);
            if (onTouchBegin != null && selIdx >= 0)
            {
                int skillId = mSkills[selIdx];
                BaseSkill skill = SkillManager.Instance.GetInstance(skillId);
                onTouchBegin.Invoke(skill);
            }
        }

    }
}
