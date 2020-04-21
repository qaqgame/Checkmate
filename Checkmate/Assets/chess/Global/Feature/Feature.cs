using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkmate.Game.Feature
{
    public interface IFeature
    {
        bool OverwriteTerrain { get; }//是否覆盖地形的效果
    }

    public class TestFeature : IFeature
    {
        public bool OverwriteTerrain
        {
            get { return true; }
        }
    }
}