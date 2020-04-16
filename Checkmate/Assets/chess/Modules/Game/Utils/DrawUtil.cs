﻿using Checkmate.Game;
using Checkmate.Game.Controller;
using Checkmate.Modules.Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Modules.Game.Utils
{
    //地图绘制工具类
    public static class DrawUtil
    {

        //已画的位置
        private static List<Position> mDrawed;

        private static MapManager mMap;

        private readonly static List<Color> styles = new List<Color>{
        Color.white,Color.red,Color.blue,Color.yellow
    };

        public static void Init(MapManager map)
        {
            mMap = map;
            mDrawed = new List<Position>();
        }

        public static void DrawSingle(Position position, int style)
        {
            CellController cell = mMap.GetCell(position);
            if (cell != null)
            {
                //已有颜色则进行混合
                if (mDrawed.Contains(position))
                {
                    Color cur = Color.Lerp(cell.Cell.GetHighlightColor(), styles[style], 0.8f);
                    cell.Cell.EnableHighlight(cur);
                }
                else
                {
                    cell.Cell.EnableHighlight(styles[style]);
                    mDrawed.Add(position);
                }
            }
        }

        public static void DrawList(List<Position> list, int style)
        {
            list.ForEach(item => {
                DrawSingle(item, style);
            });
        }
    }
}
