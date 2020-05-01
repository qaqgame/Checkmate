using Checkmate.Game.Controller;
using Checkmate.Game.Role;
using Checkmate.Game.Utils;
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

        public void Damage(RoleController controller,int damage)
        {
            if (controller != null)
            {
                controller.Current.Hp -= damage;
            }
            if (GameEnv.Instance.Current.Src.Type == (int)ControllerType.Role)
            {
                RoleController src = GameEnv.Instance.Current.Src as RoleController;
                float scale = 1-src.Current.PhysicalIgnore;
                int currentPhy = (int)scale * controller.Current.PhysicalRes;

            }
        }
    }
}
