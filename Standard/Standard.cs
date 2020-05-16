using Checkmate.Game;
using Checkmate.Game.Buff;
using Checkmate.Game.Controller;
using Checkmate.Game.Map;
using Checkmate.Game.Role;
using Checkmate.Game.Utils;
using QGF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Standard
{
    public class Standard
    {
        public void Init()
        {
            
        }

        //add
        public void Add(RoleController role,string src,int value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Add", src, value.ToString(), "Int",temp, persistent);
        }
        public void Add(RoleController role, string src, float value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Add", src, value.ToString(), "Float", temp, persistent);
        }
        //sub
        public void Sub(RoleController role, string src, float value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Sub", src, value.ToString(), "Float", temp, persistent);
        }
        public void Sub(RoleController role, string src, int value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Sub", src, value.ToString(), "Int", temp, persistent);
        }
        //mul
        public void Mul (RoleController role, string src, float value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Mul", src, value.ToString(), "Float", temp, persistent);
        }
        public void Mul(RoleController role, string src, int value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Mul", src, value.ToString(), "Int", temp, persistent);
        }
        //div
        public void Div(RoleController role, string src, int value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Div", src, value.ToString(), "Int", temp, persistent);
        }
        public void Div(RoleController role, string src, float value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Div", src, value.ToString(), "Float", temp, persistent);
        }
        //set
        public void Set(RoleController role, string src, float value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Set", src, value.ToString(), "Float", temp, persistent);
        }
        public void Set(RoleController role, string src, int value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Set", src, value.ToString(), "Int", temp, persistent);
        }
        public void Set(RoleController role, string src, bool value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Set", src, value.ToString(), "Bool", temp, persistent);
        }
        public void Set(RoleController role, string src, string value, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Set", src, value.ToString(), "String", temp, persistent);
        }


        public void ChangeAttribute(RoleController role,string opt,string src,string value,string type,bool temp=true,bool persistent = false)
        {
            Debuger.Log("change attr called");
            DataTrack track = new DataTrack();
            track.controller = role;
            track.name = src;
            track.opt = (DataOpt)System.Enum.Parse(typeof(DataOpt), opt);
            track.value = GetValue(value, type);
            if (temp)
            {
                role.TempMap.AddTrack(track);
                if (!persistent)
                {
                    GameEnv.Instance.CurrentExe.Main.Temp.AddTrack(track);
                }
            }
            else
            {
                role.CurrentMap.AddTrack(track);
                if (!persistent)
                {
                    GameEnv.Instance.CurrentExe.Main.Current.AddTrack(track);
                }
            }
            
        }
        //======================

        public void Damage(RoleController controller, int damage)
        {
            if (controller != null)
            {
                controller.Current.Hp -= damage;
            }
        }


        public int AddBuff(RoleController role,string buff)
        {
            if (role != null)
            {
                return role.AddBuff(buff);
            }
            return -1;
        }

        public void RemoveBuff(RoleController role,int id)
        {
            if (id != -1&&role!=null)
            {
                role.RemoveBuff(id);
            }
        }



        public void DamagePhysically(RoleController target,int dmg,bool miss)
        {
            target.DamagePhysically(dmg, miss);
        }



        


        //=========================
        string currentEffectName;//当前name
        RoleController currentRole;//当前角色
                                   //控制流

        #region 控制流

        public void Wait(float second)
        {
            GameExecuteManager.Instance.Wait(second);
        }

        public void PlayEffect(string name,RoleController role,float time)
        {
            GameObject effect = Resources.Load("Effects/" + name) as GameObject;
            GameObject obj=GameObject.Instantiate(effect, role.GetGameObject().transform);
            obj.transform.name = name + "_" + time.ToString();
            currentEffectName = obj.transform.name;
            currentRole = role;
            GameExecuteManager.Instance.Wait(time, DestroyCurrent);
        }

        private void DestroyCurrent()
        {
            DestroyEffect(currentEffectName, currentRole);
        }

        private void DestroyEffect(string realname,RoleController role)
        {
            GameObject obj = role.GetGameObject().transform.Find(realname).gameObject;
            GameObject.Destroy(obj);
        }



        //附着效果
        public void AttachEffect(string name,RoleController role)
        {
            GameObject effect = Resources.Load("Effects/" + name) as GameObject;
            GameObject.Instantiate(effect, role.GetGameObject().transform);
        }

        //跟踪效果

        public void AddTrackEffect(string name,RoleController src,RoleController target,float time)
        {
            GameObject effect = Resources.Load("Effects/" + name) as GameObject;
            GameObject obj=GameObject.Instantiate(effect, src.GetGameObject().transform);
            startPos = obj.transform.position;
            currentTime = 0;
            maxTime = time;
            targetPos = target.GetModel().transform.position;
            moveObj = obj;
            currentEffectName = obj.transform.name;
            GameExecuteManager.Instance.Wait(time, DestroyCurrent);
        }

        private bool UpdateEffect()
        {
            moveObj.transform.LookAt(targetPos);
            return Move();
        }

        //播放动画
        public void PlayAnim(string name,RoleController role)
        {
            //获取实例
            GameObject model = role.GetModel();
            Animator animator = model.GetComponent<Animator>();
            animator.ResetTrigger("Idle");
            animator.SetTrigger(name);
            
            GameExecuteManager.Instance.Wait(() => { return WaitForAnim(name, animator); });
        }

        private bool WaitForAnim(string name,Animator animator)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(name) && info.normalizedTime > 1.0f)
            {
                //播放结束，返回true
                animator.ResetTrigger(name);
                animator.SetTrigger("Idle");
                return true;
            }
            return false;
        }
        #endregion

        //移动
        #region 移动

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        /// <param name="role">角色</param>
        /// <param name="center">目标位置</param>
        /// <param name="time">移动时间</param>
        public void MoveTo(RoleController role,Position center,float time=1)
        {
            startPos = role.GetGameObject().transform.position;
            currentTime = 0;
            maxTime = time;
            Vector3 target = MapManager.Instance.GetCellWorldPosition(center);
            role.FaceTo(target);
            moveObj = role.GetGameObject();
            targetPos = target;

            GameExecuteManager.Instance.Wait(Move);
        }

        /// <summary>
        /// 返回到角色的本来位置
        /// </summary>
        /// <param name="role">角色</param>
        /// <param name="time">时间</param>
        public void ReturnPos(RoleController role,float time = 1)
        {
            Position t = role.Position;
            MoveTo(role, t, time);
        }

        private Vector3 CalcTargetPosition(RoleController role,Position center)
        {
            Vector3 temp = MapManager.Instance.GetCellWorldPosition(center);
            

            //如果该方格有角色
            if (MapManager.Instance.GetCell(center).HasRole)
            {
                Vector3 dir = (temp - role.GetGameObject().transform.position).normalized;

                BoxCollider srcCollider = role.GetModel().GetComponent<BoxCollider>();
                float disSrc = Vector3.Dot(srcCollider.center, dir);
                disSrc += (srcCollider.size.z / 2);

                GameObject tModel = RoleManager.Instance.GetRole(MapManager.Instance.GetCell(center).Role).GetModel();
                BoxCollider collider = tModel.GetComponent<BoxCollider>();

                float disDst = Vector3.Dot(collider.center, dir);
                disDst += (collider.size.z / 2);

                Vector3 target = temp - dir * (disSrc + disDst);
                return target;
            }

            return temp;
        }

        Vector3 startPos;//开始位置
        Vector3 targetPos;//目标位置
        float currentTime = 0;
        float maxTime = 1;
        GameObject moveObj;

        private bool Move()
        {
            currentTime += Time.deltaTime;
            moveObj.transform.position = Vector3.Lerp(startPos, targetPos, currentTime/maxTime);
            if (currentTime >= maxTime)
            {
                return true;
            }
            return false;
        }
        #endregion




        //===============================
        private object GetValue(string value,string type)
        {
            switch (type)
            {
                case "Int":return int.Parse(value);
                case "Float":return float.Parse(value);
                case "String":return value;
                case "Bool":return bool.Parse(value);
            }
            return null;
        }

        
    }
}
