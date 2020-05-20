using Checkmate.Game.Controller;
using Checkmate.Game.UI.Component;
using Checkmate.Modules.Game.Control;
using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Modules.Game.UI.Component
{
    public class RoleDataPanel
    {
        private GComponent mRoot;//根节点
        static List<string> MainProps = new List<string> { "Hp", "Mp", "Attack", "PhysicalRes", "MagicRes" };
        static List<string> SecondProp = new List<string> { "AttackSpeed","PhysicalIgnore", "MagicIgnore", "ViewRange", "ViewHeight", "AttackRange", "Miss" };

        private RolePropertyList mMainProperty;//主属性表
        private RolePropertyList mSecondProperty;//副属性

        private GImage mMoreBg;//更多属性的背景
       


        private BuffList mBuffList;//buff列表

        private SkillList mSkillList;//技能列表

        private GLoader mRoleIcon;//角色图标

        private bool _visible = true;

        private RoleController mCurrentRole;//当前角色
        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    mRoot.visible = value;
                    _visible = value;
                }
            }
        }

        public RoleDataPanel(GComponent root)
        {
            mRoot = root;
            GList skillList = mRoot.GetChildByPath("SkillList.Skills").asList;
            GList buffList = mRoot.GetChildByPath("BuffList.Buffs").asList;
            GList mainPropList = mRoot.GetChildByPath("PropertyList.ShowingProperty").asList;
            GList secondPropList = mRoot.GetChildByPath("PropertyList.HidenProperty").asList;
            mMoreBg = mRoot.GetChildByPath("PropertyList.Background2").asImage;

            mRoleIcon = mRoot.GetChildByPath("RoleIcon.Icon").asLoader;

            mSkillList = new SkillList(skillList);
            mBuffList = new BuffList(buffList);
            mMainProperty = new RolePropertyList(mainPropList);
            mSecondProperty = new RolePropertyList(secondPropList);
            mSkillList.onSkillClicked = OnSkillBtnClicked;
        }

        private void RegistRole(RoleController role)
        {
            mCurrentRole = role;
            mCurrentRole.SetBuffChangeListener(UpdateBuff);
            mCurrentRole.SetRoleChangeListener(UpdateProp);
            mCurrentRole.SetSkillChangeListener(UpdateSkill);
        }

        public void Clear()
        {
            mCurrentRole.ClearAllListener();
            mCurrentRole = null;
        }

        public void ShowRole(RoleController role)
        {
            RegistRole(role);
            UpdateBuff(role,role.Buffs);
            UpdateSkill(role.Skills);
            UpdateMainProp(role);
            UpdateSecondProp(role);
        }

        public void ShowMoreProp()
        {
            mMoreBg.visible = true;
            mSecondProperty.Visible = true;
        }

        public void HideMoreProp()
        {
            mMoreBg.visible = false;
            mSecondProperty.Visible = false;
        }

        private void UpdateBuff(RoleController role,List<int> buffs)
        {
            if (Visible)
            {
                mBuffList.Update(buffs);
            }
        }

        private void UpdateSkill(List<int> skills)
        {
            if (Visible)
            {
                mSkillList.Update(skills);
            }
        }

        private void UpdateProp(RoleController role)
        {
            UpdateMainProp(role);
            UpdateSecondProp(role);
        }

        private void UpdateMainProp(RoleController role)
        {
            if (Visible)
            {
                mMainProperty.Update(role, MainProps);

            }
        }

        private void UpdateSecondProp(RoleController role)
        {
            if (Visible)
            {
                mSecondProperty.Update(role, SecondProp);
            }
        }



        private void OnSkillBtnClicked(int skillId)
        {
            if (mCurrentRole != null)
            {
                InputManager.Instance.PreSkill(mCurrentRole, skillId);
            }
        }
    }
}
