using Checkmate.Global.Data;
using QGF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Game.Feature
{
    //此类用于管理单元格的特征
    public class FeatureManager
    {
        List<FeatureData> mUsedFeatures;//所有在当前地图中使用的feature
        DictionarySafe<int, IFeature> mLoadedFeatures;//所有要加载的feature
        public bool Init(List<FeatureData> features)
        {
            mUsedFeatures = features;
            mLoadedFeatures = new DictionarySafe<int, IFeature>();
            //加载feature
            foreach(var f in mUsedFeatures)
            {
                mLoadedFeatures.Add(f.id, LoadFeature(f.file));
            }
            return true;
        }

        //根据当前的features索引获取对应的featuer的id
        public int GetFeatureId(int idx)
        {
            return mUsedFeatures[idx].id;
        }

        //根据id来获取feature
        public IFeature GetFeature(int id)
        {
            return mLoadedFeatures[id];
        }

        private IFeature LoadFeature(string file)
        {
            return new TestFeature();
        }
    }
}
