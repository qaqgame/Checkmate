using Checkmate.Game.Controller;
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
        }
    }
}
