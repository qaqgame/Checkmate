using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Player;
using Checkmate.Game.Role;
using Checkmate.Game.Skill;
using Checkmate.Modules.Game.Utils;
using Checkmate.Services.Game;
using FairyGUI;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkmate.Modules.Game.Control
{
    //管理用户的输入操作
    public class InputManager:Singleton<InputManager>
    {
        public enum InputState
        {
            Idle,//等待状态
            Operate//操作状态
        }

        private InputState mState = InputState.Idle;//当前状态

        private ObjMonitor mObj;//对象监测

        private Vector3 mLastPosition=Vector3.zero;//上次的鼠标坐标

        private int mouseMask;//鼠标操作遮罩

        private int mCurrentSkill;//当前预览技能

        public InputManager()
        {
            mObj = new ObjMonitor();
            mouseMask = LayerMask.GetMask("Map", "Role");
        }

        
        public void HandleInput()
        {
            HandleMouse();
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (mObj.CurrentObj.Type == 2)
                {
                    RoleController role = mObj.CurrentObj as RoleController;
                    role.SetState(RoleState.PreSpell);
                    mCurrentSkill = role.Skills[0];
                }
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                if (mObj.CurrentObj.Type == 2)
                {
                    RoleController role = mObj.CurrentObj as RoleController;
                    role.SetState(RoleState.PreAttack);
                }
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                GamingPageManager.Instance.ShowMoreProp();
            }
            if (Input.GetKeyUp(KeyCode.C))
            {
                GamingPageManager.Instance.HideMoreProp();
            }
        }


        public void PreSkill(RoleController role,int skillId)
        {
            if (mObj.CurrentObj == role)
            {
                role.SetState(RoleState.PreSpell);
                mCurrentSkill = skillId;
            }
        }

        //处理鼠标事件
        private void HandleMouse()
        {
            //不处理ui遮挡住的鼠标事件

            if (!(EventSystem.current.IsPointerOverGameObject()||Stage.isTouchOnUI))
            {
                //处理移动信息
                HandleMouseMove(mLastPosition, Input.mousePosition);

                //处理点击
                if (Input.GetMouseButtonDown(0))
                {
                    HandleMouseDown(0, Input.mousePosition);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    HandleMouseDown(1, Input.mousePosition);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    HandleMouseUp(0, Input.mousePosition);
                }
                else if(Input.GetMouseButtonUp(1))
                {
                    HandleMouseUp(1, Input.mousePosition);
                }

                mLastPosition = Input.mousePosition;
            }
        }

        /// <summary>
        /// 处理鼠标按下事件
        /// </summary>
        /// <param name="key">0-左 1-右</param>
        /// <param name="position">坐标</param>
        private void HandleMouseDown(int key,Vector3 position)
        {
            //左键点击
            if (key == 0)
            {
                Debug.Log("btn left clicked");
                Ray ray = Camera.main.ScreenPointToRay(position);
                RaycastHit hit;

                //与地图有交点
                if(Physics.Raycast(ray,out hit, 500, mouseMask))
                {
                    ModelController temp = mObj.CurrentObj;//当前对象
                    ModelController target = mObj.OnClick(hit.point);
                    //如果是idle状态
                    if (mState == InputState.Idle)
                    {
                        //点击所属角色,且可操作进入operate状态
                        if (target.Type == 2)
                        {
                            if ((target as RoleController).CanOperate&&(PlayerManager.Instance.PID==(target as RoleController).Team)&&PlayerManager.Instance.CanOperate)
                            {
                                mState = InputState.Operate;
                                (target as RoleController).SetState(RoleState.PreMove);
                            }
                        }
                    }

                    //是操作状态
                    else
                    {
                        //如果是角色
                        if (temp!=null&&temp.Type == 2)
                        {
                            RoleController role = temp as RoleController;
                            if (role.CanOperate)
                            {
                                if (role.CurrentState == RoleState.PreMove && target.Type == 1)
                                {
                                    DrawUtil.ClearAll();
                                    //移动
                                    //
                                    GameNetManager.Instance.Move(role, target.GetPosition());
                                    //
                                    role.SetState(RoleState.Move);
                                    Animator animator = role.GetModel().GetComponent<Animator>();
                                    animator.ResetTrigger("Idle");
                                    animator.SetTrigger("Walk");
                                    mState = InputState.Idle;
                                    GameEvent.onResetAll.Invoke();
                                }
                                else if (role.CurrentState == RoleState.PreSpell&&target!=null)
                                {
                                    List<Position> borders = SkillManager.Instance.GetBorderRange(mCurrentSkill, role.Position);
                                    if (borders.Contains(target.GetPosition()))
                                    {
                                        //消耗行动点
                                        int cost = SkillManager.Instance.GetCost(mCurrentSkill);
                                        if (APManager.Instance.ReduceAp((int)PlayerManager.Instance.PID, cost))
                                        {

                                            //施法


                                            GameNetManager.Instance.Skill(mCurrentSkill, role, target.GetPosition());

                                            role.SetState(RoleState.Spell);
                                            mState = InputState.Idle;
                                            GameEvent.onResetAll.Invoke();
                                        }
                                    }
                                }
                                else if (role.CurrentState == RoleState.PreAttack && target != null)
                                {
                                    //获取目标距离
                                    int distance = HexMapUtil.GetDistance(role.Position, target.GetPosition());
                                    RoleController t = null;
                                    if (target.Type == (int)ControllerType.Cell)
                                    {
                                        CellController cell = (target as CellController);
                                        t = cell.HasRole ? RoleManager.Instance.GetRole(cell.Role) : null;
                                    }
                                    else if(target.Type == (int)ControllerType.Role)
                                    {
                                        t = target as RoleController;
                                    }
                                    if (t!=null&&distance<=role.Temp.AttackRange)
                                    {
                                        //是敌军则进行攻击
                                        if (PlayerManager.Instance.IsEnemy(t.Team))
                                        {
                                            //消耗行动点
                                            if (APManager.Instance.ReduceAp((int)PlayerManager.Instance.PID, 2))
                                            {
                                                //攻击操作
                                                GameNetManager.Instance.Attack(role, t.Position);

                                                role.SetState(RoleState.Attack);
                                                mState = InputState.Idle;
                                                GameEvent.onResetAll.Invoke();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //是地面
                        else
                        {
                            mState = InputState.Idle;
                        }
                    }
                }
            }

            //右键点击,重置状态
            else if (key == 1)
            {
                mState = InputState.Idle;
                GameEvent.onResetAll.Invoke();
            }
        }

        /// <summary>
        /// 处理鼠标松开事件
        /// </summary>
        /// <param name="key">0-左 1-右</param>
        /// <param name="position">坐标</param>
        private void HandleMouseUp(int key, Vector3 position)
        {

        }

        /// <summary>
        /// 处理鼠标移动
        /// </summary>
        /// <param name="lastPos">上次坐标</param>
        /// <param name="position">此次坐标</param>
        private void HandleMouseMove(Vector3 lastPos,Vector3 position)
        {
            //操作状态下
            if (mState == InputState.Operate)
            {
                ModelController temp = mObj.CurrentObj;//获取当前角色

                Ray ray = Camera.main.ScreenPointToRay(position);
                RaycastHit hit;

                //与地图有交点
                if (Physics.Raycast(ray, out hit, 500, mouseMask))
                {
                    Position target = MapManager.Instance.GetCell(hit.point).Position;
                    if (temp.Type == 2)
                    {
                        RoleController role = temp as RoleController;
                        if (role.CurrentState == RoleState.PreMove&&!MapManager.Instance.GetCell(target).HasRole)
                        {
                            //预览移动
                            List<Position> positions = MoveManager.Instance.Router.AstarNavigatorE(role, role.GetPosition(), target);
                            if (positions != null)
                            {
                                DrawUtil.ClearAll();
                                DrawUtil.DrawList(positions, 0);
                            }
                        }
                        else if (role.CurrentState == RoleState.PreSpell)
                        {
                            //预览施法
                            List<Position> borders = SkillManager.Instance.GetBorderRange(mCurrentSkill, role.Position);
                            

                            DrawUtil.ClearAll();
                            if (borders != null)
                            {
                                DrawUtil.DrawList(borders, 0);
                            }
                            if (borders.Contains(target))
                            {
                                List<Position> effects = SkillManager.Instance.GetEffectRange(mCurrentSkill, role.Position, target);
                                if (effects != null)
                                {
                                    DrawUtil.DrawList(effects, 2);
                                }
                            }
                        }
                        else if (role.CurrentState == RoleState.PreAttack)
                        {
                            //预览攻击
                            List<Position> borders = HexMapUtil.GetRange(role.Position, role.Temp.AttackRange);
                            DrawUtil.ClearAll();

                            if (borders == null)
                            {
                                return;
                            }
                            foreach(var pos in borders)
                            {
                                CellController cell = MapManager.Instance.GetCell(pos);
                                if (cell != null)
                                {
                                    //如果存在角色
                                    if (cell.HasRole)
                                    {
                                        RoleController targetRole = RoleManager.Instance.GetRole(cell.Role);
                                        //如果是敌方则可选
                                        if (PlayerManager.Instance.IsEnemy(targetRole.Team))
                                        {
                                            DrawUtil.DrawSingle(pos, 2);
                                            continue;
                                        }
                                    }
                                    DrawUtil.DrawSingle(pos, 0);
                                }
                            }
                            if (borders.Contains(target))
                            {
                                DrawUtil.DrawSingle(target, 1);
                            }
                        }
                    }
                }

            }
        }

        
    }
}
