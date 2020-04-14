using Checkmate.Global.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkmate.Game
{
    public class MapUtils
    {
        public static List<FeatureData> ParseFeatures(string path)
        {
            string content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<FeatureData>>(content);
        }

        public static List<TerrainData> ParseTerrains(string path)
        {
            string content = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<TerrainData>>(content);
        }
    }
}
