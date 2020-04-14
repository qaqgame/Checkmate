using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Checkmate.Game
{
    [System.Serializable]
    public class HexPosition
    {
        [SerializeField]
        private int x, z;
        public int X
        {
            get
            {
                return x;
            }
        }
        public int Z
        {
            get
            {
                return z;
            }
        }
        public int Y
        {
            get
            {
                return -X - Z;
            }
        }

        public HexPosition(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public static HexPosition FromOffsetToHex(int x, int z)
        {
            return new HexPosition(x, z - (x + (x & 1)) / 2);
        }

        public static HexPosition FromWorldPosition(Vector3 position)
        {
            float q = (2.0f / 3 * position.x) / HexMetrics.outerRadius;
            float r = (-1.0f / 3 * position.x + (Mathf.Sqrt(3) / 3) * position.z) / HexMetrics.outerRadius;
            return Round(new Vector3(q, -q - r, r));
        }
        //获取与float形式的hex坐标最相近的hex坐标
        public static HexPosition Round(Vector3 hex)
        {
            int rx = Mathf.RoundToInt(hex.x);
            int ry = Mathf.RoundToInt(hex.y);
            int rz = Mathf.RoundToInt(hex.z);

            float xDiff = Mathf.Abs(rx - hex.x);
            float yDiff = Mathf.Abs(ry - hex.y);
            float zDiff = Mathf.Abs(rz - hex.z);

            if (xDiff > yDiff && xDiff > zDiff)
            {
                rx = -ry - rz;
            }
            else if (yDiff > zDiff)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }
            return new HexPosition(rx, rz);
        }

        public override string ToString()
        {
            return "(" + X.ToString() + "," + Y.ToString() + "," + Z.ToString() + ")";
        }

        public string ToStringOnSeparateLines()
        {
            return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
        }

        public Position ToPosition()
        {
            return new Position(X, Y, Z);
        }
    }

    public class Position
    {
        public int x;
        public int y;
        public int z;
        public Position() { }
        public Position(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Position(Position r)
        {
            x = r.x;
            y = r.y;
            z = r.z;
        }
        public Position Add(int a, int b, int c)
        {
            return new Position(x + a, y + b, z + c);
        }

        public static Position Parse(string value)
        {
            value = value.Substring(1, value.Length - 2);
            string[] vs = value.Split(',');
            if (vs.Length != 3)
            {
                throw new System.Exception("parse position error:" + value);
            }

            int x = int.Parse(vs[0]);
            int y = int.Parse(vs[1]);
            int z = int.Parse(vs[2]);
            return new Position(x, y, z);
        }

        public override string ToString()
        {
            return "(" + x.ToString() + "," + y.ToString() + "," + z.ToString() + ")";
        }

        public string ToSeparateString()
        {
            return x.ToString() + "\n" + y.ToString() + "\n" + z.ToString();
        }

        public static Position operator +(Position l, Position r)
        {
            return new Position(l.x + r.x, l.y + r.y, l.z + r.z);
        }

        public static Position operator -(Position l, Position r)
        {
            return new Position(l.x - r.x, l.y - r.y, l.z - r.z);
        }

        public static Position operator *(Position l, int t)
        {
            return new Position(l.x * t, l.y * t, l.z * t);
        }

        public static string PositionToStringPos(Position _position)
        {
            return _position.x.ToString() + "X" + _position.y.ToString() + "X" + _position.z.ToString();
        }

        public static int operator *(Position l, Position r)
        {
            return l.x * r.x + l.y * r.y + l.z * r.z;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            Position temp = (Position)obj;
            return this.x == temp.x && this.y == temp.y && this.z == temp.z;
        }

    }

    public class PositionCompare : IEqualityComparer<Position>
    {
        public bool Equals(Position l, Position r)
        {
            return l.x == r.x && l.y == r.y && l.z == r.z;
        }

        public int GetHashCode(Position obj)
        {
            return obj.x.GetHashCode() ^ obj.y.GetHashCode() ^ obj.z.GetHashCode();
        }
    }


    // [SerializeField]
    // public struct HexCoordinate{
    //     public int x,y,z;

    //     public int height;
    //     public HexCoordinate(int x,int y,int z,int height=0){
    //         this.x=x;
    //         this.y=y;
    //         this.z=z;
    //         this.height=height;
    //     }

    //     public HexCoordinate Add(HexCoordinate offset){
    //         return new HexCoordinate(x+offset.x,y+offset.y,z+offset.z);
    //     }

    //     public Position ToPosition(){
    //         return new Position(x,y,z);
    //     }


    // }

    public class HexMapUtil
    {
        //获取两个Position之间的最短距离
        public static int GetDistance(Position a, Position b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y), Mathf.Abs(a.z - b.z));
        }

        //获取两点之间的Position
        public static List<Position> GetLine(Position a, Position b, int limitNum = -1)
        {
            List<Position> result = new List<Position>();
            int n = GetDistance(a, b);
            if (limitNum >= 0)
            {
                n = limitNum < n ? limitNum : n;
            }
            for (int i = 0; i <= n; ++i)
            {
                Position temp = Lerp(a, b, (1f / n) * i);
                if (!result.Contains(temp))
                {
                    result.Add(temp);
                }
            }
            return result;
        }

        //获取范围内的position
        public static List<Position> GetRange(Position center, int range)
        {
            List<Position> result = new List<Position>();
            range = range < 0 ? 0 : range;
            for (int x = -range; x <= range; ++x)
            {
                for (int y = Mathf.Max(-range, -x - range); y <= Mathf.Min(range, -x + range); ++y)
                {
                    int z = -x - y;
                    result.Add(new Position(x + center.x, y + center.y, z + center.z));
                }
            }
            return result;
        }
        //获取两个范围的交界
        public static List<Position> GetIntersectRange(Position center1, int range1, Position center2, int range2)
        {
            List<Position> result = new List<Position>();
            int xmin = Mathf.Max(center1.x - range1, center2.x - range2);
            int xmax = Mathf.Min(center1.x + range1, center2.x + range2);
            int ymin = Mathf.Max(center1.y - range1, center2.y - range2);
            int ymax = Mathf.Min(center1.y + range1, center1.y + range2);
            int zmin = Mathf.Max(center1.z - range1, center2.z - range2);
            int zmax = Mathf.Min(center1.z + range1, center2.z + range2);

            for (int x = xmin; x <= xmax; ++x)
            {
                for (int y = Mathf.Max(ymin, -x - zmax); y <= Mathf.Min(ymax, -x - zmin); ++y)
                {
                    int z = -x - y;
                    result.Add(new Position(x, y, z));
                }
            }
            return result;
        }
        //获取六边形六个角上的点
        public static List<Position> GetCorner(Position corner, Position center)
        {
            List<Position> result = new List<Position>();
            Position offset = corner - center;
            result.Add(corner);
            result.Add(new Position(center.x - offset.y, center.y - offset.z, center.z - offset.x));
            result.Add(new Position(center.x + offset.z, center.y + offset.x, center.z + offset.y));
            result.Add(new Position(center.x - offset.x, center.y - offset.y, center.z - offset.z));
            result.Add(new Position(center.x + offset.y, center.y + offset.z, center.z + offset.x));
            result.Add(new Position(center.x - offset.z, center.y - offset.x, center.z - offset.y));
            return result;
        }

        //获取单个环
        public static List<Position> GetSingleRing(Position center, int radius)
        {
            List<Position> result = new List<Position>();
            if (radius == 0)
            {
                result.Add(center);
                return result;
            }
            Position start = center + HexDirection.NW.ToPosition() * radius;
            for (int i = 0; i < 6; ++i)
            {
                for (int j = 0; j < radius; ++j)
                {
                    result.Add(start);
                    start = GetNeighbor(start, (HexDirection)i);
                }
            }
            return result;
        }

        //获取相邻的position
        public static Position GetNeighbor(Position pos, HexDirection direction)
        {
            Position result = pos + direction.ToPosition();
            return result;
        }

        //Position插值
        public static Position Lerp(Position a, Position b, float t)
        {
            Vector3 v0 = new Vector3(a.x, a.y, a.z);
            Vector3 v1 = new Vector3(b.x, b.y, b.z);

            return GetRoundPosition(Vector3.Lerp(v0, v1, t));
        }

        //获取相近的坐标
        public static Position GetRoundPosition(Vector3 pos)
        {
            return HexPosition.Round(pos).ToPosition();
        }
        //获取最接近的方向
        public static void GetClosedDirection(Position dir, out int direction, out int distance)
        {
            int result = 0;
            int temp = 0;
            //即找到点乘最大的
            for (int i = 0; i < 6; ++i)
            {
                Position td = ((HexDirection)i).ToPosition();
                if (dir * td > temp)
                {
                    result = i;
                    temp = dir * td;
                }
            }

            direction = result;
            float d = (float)temp / Mathf.Sqrt(2);
            distance = Mathf.RoundToInt(d);
        }


        // public HexCoordinate OffsetToHexCoordinate(int col,int row){
        //     int x=col;
        //     int z=row-(col-(col&1))/2;

        //     int y=-x-z;
        //     return new HexCoordinate(x,y,z);
        // 



    }

}
