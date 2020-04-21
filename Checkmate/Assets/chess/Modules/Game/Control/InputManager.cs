using Checkmate.Game;
using Checkmate.Game.Controller;
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
    public class InputManager
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

        public InputManager()
        {
            mObj = new ObjMonitor(GameManager.MapManager);
        }

        
        public void HandleInput()
        {
            HandleMouse();
        }


        //处理鼠标事件
        private void HandleMouse()
        {
            //不处理ui遮挡住的鼠标事件
            if (!EventSystem.current.IsPointerOverGameObject())
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
                Ray ray = Camera.main.ScreenPointToRay(position);
                RaycastHit hit;

                //与地图有交点
                if(Physics.Raycast(ray,out hit, 500, mouseMask))
                {
                    BaseController temp = mObj.CurrentObj;//当前对象
                    BaseController target = mObj.OnClick(hit.point);
                    //如果是idle状态
                    if (mState == InputState.Idle)
                    {
                        //点击角色,且可操作进入operate状态
                        if (target.Type == 2)
                        {
                            if ((target as RoleController).CanOperate)
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
                        if (temp.Type == 2)
                        {
                            RoleController role = temp as RoleController;
                            if (role.CanOperate)
                            {
                                if (role.CurrentState == RoleState.PreMove && target.Type == 1)
                                {
                                    //移动
                                    role.SetState(RoleState.Move);
                                }
                                else if (role.CurrentState == RoleState.PreSpell)
                                {
                                    //施法
                                    role.SetState(RoleState.Spell);
                                }
                            }
                        }
                    }
                }
            }

            //右键点击,重置状态
            else if (key == 1)
            {

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
                BaseController temp = mObj.CurrentObj;//获取当前角色

                Ray ray = Camera.main.ScreenPointToRay(position);
                RaycastHit hit;

                //与地图有交点
                if (Physics.Raycast(ray, out hit, 500, mouseMask))
                {
                    Position target = HexMapUtil.GetRoundPosition(hit.point);
                    if (temp.Type == 2)
                    {
                        RoleController role = temp as RoleController;
                        if (role.CurrentState == RoleState.PreMove)
                        {
                            //预览移动
                        }
                        else if (role.CurrentState == RoleState.PreSpell)
                        {
                            //预览施法
                        }
                    }
                }

            }
        }

        
    }
}
