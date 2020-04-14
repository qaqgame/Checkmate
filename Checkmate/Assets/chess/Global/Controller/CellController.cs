using Checkmate.Game.Controller;
using Checkmate.Global.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Game.Controller
{
    public class CellController :BaseController
    {
        private HexCell mCell;

        private string mRole=null;//当前的角色

        public string Role
        {
            get { return mRole; }
        }

        public CellController(int cost,bool available,HexCell cell,string extra=null):base(extra)
        {
            _cost = cost;
            _available = available;
            mCell = cell;
        }

        //控制器类别：地板
        public override int Type { get { return 1; } }

        
        private int _cost;//移动花费
        private bool _available;//可站立

        [GetProperty]
        public int Cost
        {
            get { return _cost; }
        }

        [GetProperty]
        public bool Available
        {
            get { return _available; }
        }




        public override GameObject GetGameObject()
        {
            return mCell.gameObject;
        }

        public override Position GetPosition()
        {
            return mCell.coordinates.ToPosition();
        }
    }
}
