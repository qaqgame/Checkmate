using Checkmate.Game.Controller;
using Checkmate.Game.Role;
using Checkmate.Game.Utils;
using QGF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Standard
{
    public class Standard
    {
        public void Init()
        {
            
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

        public void Damage(RoleController controller,int damage)
        {
            if (controller != null)
            {
                controller.Temp.Hp -= damage;
            }
        }

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
