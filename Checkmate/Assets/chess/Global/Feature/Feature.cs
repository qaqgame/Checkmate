using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkmate.Game.Feature
{
    public abstract class Feature : MonoBehaviour
    {

        public void Visible()
        {

        }
        //使其不可见
        public void InVisible()
        {

        }
        //用于控制物体的显示
        public abstract void OnBuild(HexCell cell);
        //用于控制feature的效果
        public abstract void OnAttach(HexCell cell);
    }
}