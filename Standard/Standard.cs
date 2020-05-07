using Checkmate.Game.Controller;
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
                    GameEnv.Instance.Current.Main.Temp.AddTrack(track);
                }
            }
            else
            {
                role.CurrentMap.AddTrack(track);
                if (!persistent)
                {
                    GameEnv.Instance.Current.Main.Current.AddTrack(track);
                }
            }
            
        }
        //======================

        public void Damage(RoleController controller,int damage)
        {
            if (controller != null)
            {
                controller.Current.Hp -= damage;
            }
        }









        //=========================
        string currentEffectName;//当前name
        RoleController currentRole;//当前角色
        //控制流
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




        public void AttachEffect(string name,RoleController role)
        {
            GameObject effect = Resources.Load("Effects/" + name) as GameObject;
            GameObject.Instantiate(effect, role.GetGameObject().transform);
        }








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
