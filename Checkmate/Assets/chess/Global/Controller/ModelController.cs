using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Game.Controller
{
    //实体控制器
    public abstract class ModelController:BaseController
    {
        public ModelController(string extra = null) : base(extra)
        {

        }
        public abstract Position GetPosition();
        public abstract GameObject GetGameObject();
    }
}
