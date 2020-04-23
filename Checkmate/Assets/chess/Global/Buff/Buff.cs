using Checkmate.Game.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Game.Buff
{
    public class Buff:BaseController
    {

        public override int Type { get { return -1; } }

        //剩余回合
        [GetProperty]
        [SetProperty]
        public int ReserveTurn { get; set; }

        //剩余次数
        [GetProperty]
        [SetProperty]
        public int ReserveTime { get; set; }

        
    }
}
