using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QGF.Module;
using QGF;
namespace Assets.chess.Modules.ExampleA
{
    class ExampleModule:GeneralModule
    {
        public override void Create(object args = null)
        {
            base.Create(args);
            Debuger.Log("created!");
        }

        protected override void Show(object arg)
        {
            base.Show(arg);
        }
    }
}
