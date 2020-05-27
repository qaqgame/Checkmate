using Checkmate.Game;
using Checkmate.Game.Buff;
using Checkmate.Game.Controller;
using Checkmate.Game.Effect;
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
        public void Add(RoleController role,string src,string value,string type, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Add", src, value, type,temp, persistent);
        }
        //sub
        public void Sub(RoleController role, string src, string value,string type, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Sub", src, value, type, temp, persistent);
        }
        //mul
        public void Mul (RoleController role, string src, string value,string type, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Mul", src, value, type, temp, persistent);
        }
        //div
        public void Div(RoleController role, string src, string value,string type, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Div", src, value, type, temp, persistent);
        }
        //set
        public void Set(RoleController role, string src, string value,string type, bool temp = true, bool persistent = false)
        {
            ChangeAttribute(role, "Set", src, value, type, temp, persistent);
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

        public void ChangeDamage(string opt,string value)
        {
            float v = float.Parse(value);
            float result=GameEnv.Damage;
            switch (opt)
            {
                case "Add":
                    {
                        result = DataUtil.Add(GameEnv.Damage, v);
                        break;
                    }
                case "Sub":
                    {
                        result = DataUtil.Sub(GameEnv.Damage, v);
                        break;
                    }
                case "Mul":
                    {
                        result = DataUtil.Mul(GameEnv.Damage, v);
                        break;
                    }
                case "Div":
                    {
                        result = DataUtil.Div(GameEnv.Damage, v);
                        break;
                    }
            }
            GameEnv.Damage = (int)result;
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

        public int AddCellEffect(Position pos,string effect)
        {
            int id = EffectManager.Instance.AddEffect(pos, effect);
            return id;
        }

        public void AddCellEffects(CellController cell,string effect)
        {
            EffectManager.Instance.AddEffect(cell.Position, effect);
        }

        public void RemoveCellEffect(int id)
        {
            if (id != -1)
            {
                EffectManager.Instance.RemoveEffect(id);
            }
        }

        public void RemoveCellEffects(CellController cell)
        {
            EffectManager.Instance.RemoveAllEffects(cell.Position);
        }


        public void DamagePhysically(RoleController target,int dmg,bool miss)
        {
            target.DamagePhysically(dmg, miss);
        }

        public void DamageMagically(RoleController target,int dmg,bool miss)
        {
            target.DamageMagically(dmg, miss);
        }
        public void Attack(RoleController target,bool isMagic,bool canMiss)
        {
            target.Attacked(isMagic, canMiss);
        }


        


        //=========================
        string currentEffectName;//当前name
        RoleController currentRole;//当前角色
        ModelController currentParent;//当前特效父节点
        bool parentModel = false;
        bool attachOnRole = true;


        #region 控制流

        public void Wait(float second)
        {
            GameExecuteManager.Instance.Wait(second);
        }

        public void PlayEffect(string name,RoleController role,float time)
        {
            if (role.Visible)
            {
                GameObject effect = Resources.Load("Effects/" + name) as GameObject;
                GameObject obj = GameObject.Instantiate(effect, role.GetGameObject().transform);
                obj.transform.name = name + "_" + time.ToString();
                currentEffectName = obj.transform.name;
                currentRole = role;
                currentParent = role;
                parentModel = false;
                attachOnRole = true;
                GameExecuteManager.Instance.Wait(time, DestroyCurrent);
            }
        }

        public void PlayGroundEffect(string name,Position position,float time)
        {
            Position src = GameEnv.Instance.CurrentExe.Src.GetPosition();
            CellController srcCell = MapManager.Instance.GetCell(src);
            if (srcCell != null && srcCell.Visible)
            {
                CellController cell = MapManager.Instance.GetCell(position);
                if (cell != null&&cell.Visible)
                {
                    GameObject effect = Resources.Load("Effects/" + name) as GameObject;
                    GameObject obj = GameObject.Instantiate(effect, cell.GetGameObject().transform);
                    obj.transform.name = name + "_" + time.ToString();
                    currentEffectName = obj.transform.name;
                    currentRole = null;
                    currentParent = cell;
                    parentModel = false;
                    attachOnRole = false;
                    GameExecuteManager.Instance.Wait(time, DestroyCurrent);
                }
            }
        }

        public void PlayEffectWithDir(string name,RoleController src,RoleController dst,float time)
        {
            if (src.Visible)
            {
                GameObject effect = Resources.Load("Effects/" + name) as GameObject;
                GameObject obj = GameObject.Instantiate(effect, src.GetModel().transform);
                obj.transform.name = name + "_" + time.ToString();
                obj.transform.rotation = Quaternion.LookRotation(dst.GetModel().transform.position - src.GetModel().transform.position);
                currentEffectName = obj.transform.name;
                currentRole = src;
                currentParent = src;
                parentModel = true;
                attachOnRole = true;
                GameExecuteManager.Instance.Wait(time, DestroyCurrent);
            }
        }
        private void DestroyCurrent()
        {
            if (attachOnRole)
            {
                DestroyEffect(currentEffectName, currentRole, parentModel);
            }
            else
            {
                DestroyEffect(currentEffectName, currentParent);
            }
        }

        private void DestroyEffect(string realname, ModelController model)
        {

            Transform obj = null;
            obj = model.GetGameObject().transform.Find(realname);
            
            if (obj != null)
            {
                GameObject.Destroy(obj.gameObject);
            }
        }

        private void DestroyEffect(string realname,RoleController role,bool parentModel)
        {

            Transform obj=null;
            
            if (parentModel)
            {
                obj= role.GetModel().transform.Find(realname);
            }
            else
            {
                obj = role.GetGameObject().transform.Find(realname);
            }
            if (obj != null)
            {
                GameObject.Destroy(obj.gameObject);
            }
        }



        //附着效果
        public string AttachEffect(string name,RoleController role)
        {
            GameObject effect = Resources.Load("Effects/" + name) as GameObject;
            GameObject obj= GameObject.Instantiate(effect, role.GetGameObject().transform);
            obj.transform.name = name + Time.time.ToString();
            EffectInstances.Add(obj.transform.name, obj);
            return obj.transform.name;
        }

        //跟踪效果

        public void AddTrackEffect(string name,RoleController src,RoleController target,float time)
        {
            if (target.Visible)
            {
                GameObject effect = Resources.Load("Effects/" + name) as GameObject;
                GameObject obj = GameObject.Instantiate(effect, target.GetGameObject().transform);
                currentRole = target;
                parentModel = false;
                attachOnRole = true;
                currentParent = target;
                obj.transform.position = src.GetGameObject().transform.position;
                startPos = obj.transform.position;
                currentTime = 0;
                maxTime = time;
                targetPos = target.GetModel().transform.position;
                moveObj = obj;
                currentEffectName = obj.transform.name;
                GameExecuteManager.Instance.Wait(UpdateEffect, DestroyCurrent);
            }
        }

        private bool UpdateEffect()
        {
            moveObj.transform.LookAt(targetPos);
            return Move();
        }

        //播放动画
        public void PlayAnim(string name, RoleController role)
        {
            if (role.Visible)
            {
                //获取实例
                GameObject model = role.GetModel();
                Animator animator = model.GetComponent<Animator>();
                //如果是移动状态先切到idle
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                {
                    animator.SetTrigger("Idle");
                }
                animator.SetBool("FinishAction", false);
                animator.SetTrigger(name);

                GameExecuteManager.Instance.Wait(() => { return WaitForAnim(name, animator); });

            }
        }
        /// <summary>
        /// 播放src的动画，并在dst上附加特效
        /// </summary>
        /// <param name="anim">动画名</param>
        /// <param name="eff">特效名</param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="time"></param>
        public void PlayAnimWithEffect(string anim, string eff, RoleController src, bool needDestroy)
        {
            if (src.Visible)
            {
                
                //动画部分
                //获取实例
                GameObject model = src.GetModel();
                Animator animator = model.GetComponent<Animator>();
                //如果是移动状态先切到idle
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                {
                    animator.SetTrigger("Idle");
                }
                animator.SetBool("FinishAction", false);
                animator.SetTrigger(anim);


                GameObject effect = Resources.Load("Effects/" + eff) as GameObject;
                if (effect != null)
                {
                    GameObject obj = GameObject.Instantiate(effect, src.GetModel().transform);
                    obj.transform.name = eff + "_" + Time.time.ToString();
                    currentEffectName = obj.transform.name;
                    currentRole = src;
                    parentModel = true;
                }
                else
                {
                    needDestroy = false;
                }


                if (needDestroy)
                {
                    GameExecuteManager.Instance.Wait(() => { return WaitForAnim(anim, animator); }, DestroyCurrent);
                }
                else
                {
                    GameExecuteManager.Instance.Wait(() => { return WaitForAnim(anim, animator); });
                }

            }
        }

        private bool WaitForAnim(string name,Animator animator)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            
            if ((info.IsName(name) && info.normalizedTime >= 1.0f))
            {
                //播放结束，返回true
                animator.SetBool("FinishAction", true);
                animator.ResetTrigger(name);
                return true;
            }
            return false;
        }
        #endregion

        //长久特效
        static Dictionary<string, GameObject> EffectInstances=new Dictionary<string, GameObject>();//所有的特效实例

        /// <summary>
        /// 在目标位置放置effect的特效
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="effect"></param>
        /// <returns></returns>
        public string PlaceEffect(Position pos,string effect)
        {
            CellController cell = MapManager.Instance.GetCell(pos);
            if (effect.Length > 0&&cell!=null)
            {
                GameObject instance = Resources.Load("Effects/" + effect) as GameObject;
                GameObject obj = GameObject.Instantiate(instance, cell.GetGameObject().transform);
                obj.transform.position = MapManager.Instance.GetCellWorldPosition(pos);
                obj.transform.name = effect + "_" + Time.time.ToString();

                EffectInstances.Add(obj.transform.name, obj);
                return obj.transform.name;
            }
            return "";
        }


        public void RemovePlacedEffect(string name)
        {
            if (name.Length > 0&&EffectInstances.ContainsKey(name))
            {
                GameObject target = EffectInstances[name];
                GameObject.Destroy(target);
            }
        }
        
        
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

            Animator animator = role.GetModel().GetComponent<Animator>();
            tempAnim = animator;
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Walk");
            GameExecuteManager.Instance.Wait(Move,OnWalkFinish);
        }
        Animator tempAnim=null;
        private void OnWalkFinish()
        {
            if (tempAnim != null)
            {
                tempAnim.ResetTrigger("Walk");
                tempAnim.SetTrigger("Idle");
            }
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
            if (MapManager.Instance.IsVisible(moveObj.transform.position))
            {
                moveObj.SetActive(true);
            }
            else
            {
                moveObj.SetActive(false);
            }
            if (currentTime >= maxTime)
            {
                return true;
            }
            return false;
        }
        #endregion


        #region 音效
        public void PlaySound(string name,float delay)
        {
            AudioManager.Instance.PlaySound(name,delay);
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
