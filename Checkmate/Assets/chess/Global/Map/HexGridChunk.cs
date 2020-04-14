using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkmate.Game
{
    public class HexGridChunk : MonoBehaviour
    {
        public HexMesh terrain, rivers, roads, water, waterShore, estuaries;
        public HexFeatureManager features;//特征管理
        HexCell[] cells;
        Canvas gridCanvas;
        //三个颜色用于纹理混合
        static Color weights1 = new Color(1f, 0f, 0f);
        static Color weights2 = new Color(0f, 1f, 0f);
        static Color weights3 = new Color(0f, 0f, 1f);
        private void Awake()
        {
            gridCanvas = GetComponentInChildren<Canvas>();
            cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
            ShowUI(true);
        }
        // Start is called before the first frame update

        // Update is called once per frame
        void Update()
        {

        }

        public void AddCell(int idx, HexCell cell)
        {
            cells[idx] = cell;
            cell.chunk = this;
            cell.transform.SetParent(transform, false);
            cell.uiRect.SetParent(gridCanvas.transform, false);
        }

        public void ShowUI(bool visible)
        {
            gridCanvas.gameObject.SetActive(visible);
        }

        public void Refresh()
        {
            enabled = true;
        }

        private void LateUpdate()
        {
            Triangulate();
            enabled = false;
        }

        public void Triangulate()
        {
            terrain.Clear();
            rivers.Clear();
            roads.Clear();
            water.Clear();
            waterShore.Clear();
            estuaries.Clear();
            features.Clear();
            for (int i = 0; i < cells.Length; ++i)
            {
                Triangulate(cells[i]);
            }
            terrain.Apply();
            rivers.Apply();
            roads.Apply();
            water.Apply();
            waterShore.Apply();
            estuaries.Apply();
            features.Apply();
            terrain.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", HexGrid.GetTerrainTextures());
        }

        private void Triangulate(HexCell cell)
        {
            cell.firstAddFeature = true;
            for (HexDirection direction = HexDirection.NE; direction <= HexDirection.N; ++direction)
            {
                Triangulate(direction, cell);
            }
            if (!cell.IsUnderWater && !cell.HasRiver)
            {
                features.AddFeature(cell, cell.Position);
            }
            //如果该单元格存在特征但未显示
        }

        //三角化一个单元格的单个方向
        private void Triangulate(HexDirection direction, HexCell cell)
        {
            Vector3 center = cell.Position;
            EdgeVertices edge = new EdgeVertices(center + HexMetrics.GetFirstSolidCorner(direction), center + HexMetrics.GetSecondSolidCorner(direction));
            //如果存在河流
            if (cell.HasRiver)
            {
                //河流经过当前方向，降低顶点位置来生成河床
                if (cell.HasRiverThroughEdge(direction))
                {
                    edge.v3.y = cell.StreamBedY;
                    if (cell.HasRiverBeginOrEnd)
                    {
                        TriangulateWithRiverBeginOrEnd(direction, cell, center, edge);
                    }
                    else
                    {
                        TriangulateWithRiver(direction, cell, center, edge);
                    }
                }
                //河流不流经当前方向
                else
                {
                    TriangulateAdjacentToRiver(direction, cell, center, edge);
                }
            }
            else
            {
                TriangulateWithoutRiver(direction, cell, center, edge);
                if (!cell.IsUnderWater && (!cell.HasLargeFeature))
                {
                    features.AddFeature(cell, (center + edge.v1 + edge.v5) * (1f / 3f));
                }
            }


            TriangulateConnection(direction, cell, edge);

            if (cell.IsUnderWater)
            {
                TriangulateWater(direction, cell, center);
            }
        }
        //三角化单元格的连接部分
        private void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e)
        {
            HexCell neighbor = cell.GetNeighbor(direction);
            if (neighbor == null)
            {
                return;
            }
            Vector3 bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.Position.y - cell.Position.y;
            EdgeVertices e2 = new EdgeVertices(e.v1 + bridge, e.v5 + bridge);
            //降低另一边的顶点来生成河床
            if (cell.HasRiverThroughEdge(direction))
            {
                e2.v3.y = neighbor.StreamBedY;
                Vector3 indices;
                indices.x = indices.z = cell.Index;
                indices.y = neighbor.Index;
                if (!cell.IsUnderWater && !neighbor.IsUnderWater)
                {
                    //添加水面
                    TriangulateRiverQuad(e.v2, e.v4, e2.v2, e2.v4, cell.RiverSurfaceY, neighbor.RiverSurfaceY, 0.8f,
                    cell.HasIncomingRiver && cell.IncomingRiver == direction, indices);
                }
                //当前格比较高
                else if (cell.Elevation > neighbor.WaterLevel)
                {
                    TriangulateWaterfallInWater(e.v2, e.v4, e2.v2, e2.v4, cell.RiverSurfaceY, neighbor.RiverSurfaceY, neighbor.WaterSurfaceY, indices);
                }
                //邻接格比较高
                else if (!neighbor.IsUnderWater && neighbor.Elevation > cell.WaterLevel)
                {
                    TriangulateWaterfallInWater(e2.v4, e2.v2, e.v4, e.v2, neighbor.RiverSurfaceY, cell.RiverSurfaceY, cell.WaterSurfaceY, indices);
                }
            }

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            {
                TriangulateEdgeTerraces(e, cell, e2, neighbor, cell.HasRoadThroughEdge(direction));
            }
            else
            {
                TriangulateEdgeStrip(e, weights1, cell.Index, e2, weights2, neighbor.Index, cell.HasRoadThroughEdge(direction));
            }
            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
            //填充角落的三角形
            if ((direction == HexDirection.N || direction == HexDirection.NE) && nextNeighbor != null)
            {
                Vector3 v5 = e.v5 + HexMetrics.GetBridge(direction.Next());
                v5.y = nextNeighbor.Position.y;
                //检测海拔的最低值并以其作为最低点
                if (cell.Elevation <= neighbor.Elevation)
                {
                    if (cell.Elevation <= nextNeighbor.Elevation)
                    {
                        TriangulateCorner(e.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
                    }
                    else
                    {
                        TriangulateCorner(v5, nextNeighbor, e.v5, cell, e2.v5, neighbor);
                    }
                }
                else if (neighbor.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e.v5, cell);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, e.v5, cell, e2.v5, neighbor);
                }
                // AddTriangle(v2,v4,v5);
                // AddTriangleColor(cell.Color,neighbor.Color,nextNeighbor.Color);
            }
        }
        //处理地形边缘(高度不一)
        private void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell, bool hasRoad)
        {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            Color w2 = HexMetrics.TerraceLerp(weights1, weights2, 1);
            float i1 = beginCell.Index;
            float i2 = endCell.Index;
            //第一级的斜面
            TriangulateEdgeStrip(begin, weights1, i1, e2, w2, i2, hasRoad);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                EdgeVertices e1 = e2;
                Color w1 = w2;
                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                w2 = HexMetrics.TerraceLerp(weights1, weights2, i);
                TriangulateEdgeStrip(e1, w1, i1, e2, w2, i2, hasRoad);
            }
            //最后一级
            TriangulateEdgeStrip(e2, w2, i1, end, weights2, i2, hasRoad);
        }
        //填充角落的三角
        private void TriangulateCorner(Vector3 bottom, HexCell bottomCell,
                        Vector3 left, HexCell leftCell,
                        Vector3 right, HexCell rightCell)
        {
            HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);
            //两边都为斜面
            if (leftEdgeType == HexEdgeType.Slope)
            {
                if (rightEdgeType == HexEdgeType.Slope)
                {
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
                //一面为斜面
                else if (rightEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                }
                else
                {
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            }
            //一面为斜面
            else if (rightEdgeType == HexEdgeType.Slope)
            {
                if (leftEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }
                else
                {
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            }
            //两者之间为斜面
            else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                if (leftCell.Elevation < rightCell.Elevation)
                {
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }
                else
                {
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
                }
            }
            else
            {
                terrain.AddTriangle(bottom, left, right);
                Vector3 indices;
                indices.x = bottomCell.Index;
                indices.y = leftCell.Index;
                indices.z = rightCell.Index;
                terrain.AddTriangleCellData(indices, weights1, weights2, weights3);
            }
        }
        //处理斜面之间的三角的填充插值
        private void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
            Color w3 = HexMetrics.TerraceLerp(weights1, weights2, 1);
            Color w4 = HexMetrics.TerraceLerp(weights1, weights3, 1);
            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            //第一部分的填充
            terrain.AddTriangle(begin, v3, v4);
            terrain.AddTriangleCellData(indices, weights1, w3, w4);
            //中间部分的填充
            for (int i = 2; i < HexMetrics.terraceSteps; ++i)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color w1 = w3;
                Color w2 = w4;
                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                w3 = HexMetrics.TerraceLerp(weights1, weights2, i);
                w4 = HexMetrics.TerraceLerp(weights1, weights3, i);
                terrain.AddQuad(v1, v2, v3, v4);
                terrain.AddQuadCellData(indices, w1, w2, w3, w4);
            }
            //最后一部分填充
            terrain.AddQuad(v3, v4, left, right);
            terrain.AddQuadCellData(indices, w3, w4, weights2, weights3);
        }
        //右侧悬崖
        private void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            float b = 1f / (rightCell.Elevation - beginCell.Elevation);
            b = b < 0 ? -b : b;
            Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
            Color boundaryWeights = Color.Lerp(weights1, weights3, b);
            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            TriangulateBoundaryTriangle(begin, weights1, left, weights2, boundary, boundaryWeights, indices);
            //如果两者之间为斜面
            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, weights2, right, weights3, boundary, boundaryWeights, indices);
            }
            else
            {
                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeights);
            }

        }
        //左侧悬崖
        private void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
        {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            b = b < 0 ? -b : b;
            Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
            Color boundaryWeights = Color.Lerp(weights1, weights2, b);
            Vector3 indices;
            indices.x = beginCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            TriangulateBoundaryTriangle(right, weights3, begin, weights1, boundary, boundaryWeights, indices);
            //如果两者之间为斜面
            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, weights2, right, weights3, boundary, boundaryWeights, indices);
            }
            else
            {
                terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
                terrain.AddTriangleCellData(indices, weights2, weights3, boundaryWeights);
            }

        }
        //三角化悬崖与斜面交界处,不对boundary进行干扰
        private void TriangulateBoundaryTriangle(Vector3 begin, Color beginWeights, Vector3 left, Color leftWeights, Vector3 boundary, Color boundaryWeights, Vector3 indices)
        {
            //下半面（即与斜面同高的那面
            Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, 1);
            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
            terrain.AddTriangleCellData(indices, beginWeights, w2, boundaryWeights);
            //中间
            for (int i = 2; i < HexMetrics.terraceSteps; ++i)
            {
                Vector3 v1 = v2;
                Color w1 = w2;
                v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);
                terrain.AddTriangleUnperturbed(v1, v2, boundary);
                terrain.AddTriangleCellData(indices, w1, w2, boundaryWeights);
            }
            //最后一段
            terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
            terrain.AddTriangleCellData(indices, w2, leftWeights, boundaryWeights);
        }
        //三角化河流///////////////////////////////////////////
        //带有河流的三角剖分(对某一方向的剖分/带有该方向的边缘)
        private void TriangulateWithRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
        {
            Vector3 centerL;
            Vector3 centerR;
            //直线流动的河
            if (cell.HasRiverThroughEdge(direction.Opposite()))
            {
                centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
                centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
            }
            //急转弯
            else if (cell.HasRiverThroughEdge(direction.Next()))
            {
                centerL = center;
                centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
            }
            else if (cell.HasRiverThroughEdge(direction.Previous()))
            {
                centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
                centerR = center;
            }
            //缓弯
            else if (cell.HasRiverThroughEdge(direction.Next2()))
            {
                centerL = center;
                centerR = center +
                    HexMetrics.GetSolidEdgeMiddle(direction.Next()) * (0.5f * HexMetrics.innerToOuter);
            }
            else
            {
                centerL = center +
                    HexMetrics.GetSolidEdgeMiddle(direction.Previous()) * (0.5f * HexMetrics.innerToOuter);
                centerR = center;
            }
            //重设center位置
            center = Vector3.Lerp(centerL, centerR, 0.5f);
            EdgeVertices m = new EdgeVertices(
                Vector3.Lerp(centerL, e.v1, 0.5F),
                Vector3.Lerp(centerR, e.v5, 0.5f),
                1f / 6f
            );
            //降低高度为河道
            m.v3.y = center.y = e.v3.y;
            //生成中心到边缘的河道
            TriangulateEdgeStrip(m, weights1, cell.Index, e, weights1, cell.Index);
            //生成中心部分
            terrain.AddTriangle(centerL, m.v1, m.v2);
            terrain.AddQuad(centerL, center, m.v2, m.v3);
            terrain.AddQuad(center, centerR, m.v3, m.v4);
            terrain.AddTriangle(centerR, m.v4, m.v5);

            Vector3 indices;
            indices.x = indices.y = indices.z = cell.Index;
            terrain.AddTriangleCellData(indices, weights1);
            terrain.AddQuadCellData(indices, weights1);
            terrain.AddQuadCellData(indices, weights1);
            terrain.AddTriangleCellData(indices, weights1);

            //添加水面
            if (!cell.IsUnderWater)
            {
                bool reversed = cell.IncomingRiver == direction;
                TriangulateRiverQuad(centerL, centerR, m.v2, m.v4, cell.RiverSurfaceY, 0.4f, reversed, indices);
                TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed, indices);
            }
        }
        //三角化带有河流开始或结尾的部分
        private void TriangulateWithRiverBeginOrEnd(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
        {
            EdgeVertices m = new EdgeVertices(Vector3.Lerp(center, e.v1, 0.5f), Vector3.Lerp(center, e.v5, 0.5f));
            m.v3.y = e.v3.y;
            TriangulateEdgeStrip(m, weights1, cell.Index, e, weights1, cell.Index);
            TriangulateEdgeFan(center, m, cell.Index);

            //绘制水面
            if (!cell.IsUnderWater)
            {
                bool reversed = cell.HasIncomingRiver;
                Vector3 indices;
                indices.x = indices.y = indices.z = cell.Index;
                TriangulateRiverQuad(m.v2, m.v4, e.v2, e.v4, cell.RiverSurfaceY, 0.6f, reversed, indices);
                center.y = m.v2.y = m.v4.y = cell.RiverSurfaceY;
                rivers.AddTriangle(center, m.v2, m.v4);
                if (reversed)
                {
                    rivers.AddTriangleUV(
                        new Vector2(0.5f, 0.4f), new Vector2(1f, 0.2f), new Vector2(0f, 0.2f)
                    );
                }
                else
                {
                    rivers.AddTriangleUV(
                        new Vector2(0.5f, 0.4f), new Vector2(0f, .6f), new Vector2(1f, 0.6f)
                    );
                }
                rivers.AddTriangleCellData(indices, weights1);
            }
        }
        //三角化河流两侧
        private void TriangulateAdjacentToRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
        {
            if (cell.HasRoads)
            {
                TriangulateRoadAdjacentToRiver(direction, cell, center, e);
            }

            //前一方向和后一方向都存在河流时，将该方向中心点外移到河流外边缘
            if (cell.HasRiverThroughEdge(direction.Next()))
            {
                if (cell.HasRiverThroughEdge(direction.Previous()))
                {
                    center += HexMetrics.GetSolidEdgeMiddle(direction) * (HexMetrics.innerToOuter * 0.5f);
                }
                //检测是否是小弯的,是的话将中心点外移
                else if (cell.HasRiverThroughEdge(direction.Previous2()))
                {
                    center += HexMetrics.GetFirstSolidCorner(direction) * 0.25f;
                }
            }
            //另一个方向的直河
            else if (cell.HasRiverThroughEdge(direction.Previous()) &&
                cell.HasRiverThroughEdge(direction.Next2()))
            {
                center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
            }

            EdgeVertices m = new EdgeVertices(
                Vector3.Lerp(center, e.v1, 0.5f),
                Vector3.Lerp(center, e.v5, 0.5f)
            );

            //对外圈用三角扇
            TriangulateEdgeStrip(m, weights1, cell.Index, e, weights1, cell.Index);
            //对内圈使用三角环填充
            TriangulateEdgeFan(center, m, cell.Index);

            if (!cell.IsUnderWater && !cell.HasRiver && !cell.HasLargeFeature)
            {
                features.AddFeature(cell, (center + e.v1 + e.v5) * (1f / 3f));
            }
        }

        private void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y, float v, bool reversed, Vector3 indices)
        {
            TriangulateRiverQuad(v1, v2, v3, v4, y, y, v, reversed, indices);
        }

        //创建水面的四边形,需要设置当前的v坐标
        private void TriangulateRiverQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float v, bool reversed, Vector3 indices)
        {
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;
            rivers.AddQuad(v1, v2, v3, v4);
            if (reversed)
            {
                rivers.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v);
            }
            else
            {
                rivers.AddQuadUV(0f, 1f, v, v + 0.2f);
            }
            rivers.AddQuadCellData(indices, weights1, weights2);
        }
        //三角化道路//////////////////////////////////////////////////////////
        //处理河流两边有道路的情况
        private void TriangulateRoadAdjacentToRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
        {
            bool hasRoadThroughEdge = cell.HasRoadThroughEdge(direction);
            bool previousHasRiver = cell.HasRiverThroughEdge(direction.Previous());
            bool nextHasRiver = cell.HasRiverThroughEdge(direction.Next());
            Vector2 interpolators = GetRoadInterpolators(direction, cell);
            Vector3 roadCenter = center;
            //处理为河流起始或结束的情况
            if (cell.HasRiverBeginOrEnd)
            {
                roadCenter += HexMetrics.GetSolidEdgeMiddle(cell.RiverBeginOrEndDirection.Opposite()) * (1f / 3f);
            }
            //处理直线的情况
            else if (cell.IncomingRiver == cell.OutgoingRiver.Opposite())
            {
                Vector3 corner;
                if (previousHasRiver)
                {
                    if (!hasRoadThroughEdge && cell.HasRoadThroughEdge(direction.Next()))
                    {
                        return;
                    }
                    corner = HexMetrics.GetSecondSolidCorner(direction);
                }
                else
                {
                    if (!hasRoadThroughEdge && !cell.HasRoadThroughEdge(direction.Previous()))
                    {
                        return;
                    }
                    corner = HexMetrics.GetFirstSolidCorner(direction);
                }
                roadCenter += corner * 0.5f;
                center += corner * 0.25f;
            }
            //处理拐弯的情况
            else if (cell.IncomingRiver == cell.OutgoingRiver.Previous())
            {
                roadCenter -= HexMetrics.GetSecondCorner(cell.IncomingRiver) * 0.2f;
            }
            else if (cell.IncomingRiver == cell.OutgoingRiver.Next())
            {
                roadCenter -= HexMetrics.GetFirstCorner(cell.IncomingRiver) * 0.2f;
            }
            //河流在次回头的情况
            else if (previousHasRiver && nextHasRiver)
            {
                if (!hasRoadThroughEdge)
                {
                    return;
                }
                Vector3 offset = HexMetrics.GetSolidEdgeMiddle(direction) * HexMetrics.innerToOuter;
                roadCenter += offset * 0.7f;
                center += offset * 0.5f;
            }
            //在一个河流外
            else
            {
                HexDirection middle;
                if (previousHasRiver)
                {
                    middle = direction.Next();
                }
                else if (nextHasRiver)
                {
                    middle = direction.Previous();
                }
                else
                {
                    middle = direction;
                }
                //对道路进行修剪
                if (!cell.HasRoadThroughEdge(middle) &&
                    !cell.HasRoadThroughEdge(middle.Previous()) &&
                    !cell.HasRoadThroughEdge(middle.Next()))
                {
                    return;
                }
                roadCenter += HexMetrics.GetSolidEdgeMiddle(middle) * 0.25f;
            }

            Vector3 mL = Vector3.Lerp(roadCenter, e.v1, interpolators.x);
            Vector3 mR = Vector3.Lerp(roadCenter, e.v5, interpolators.y);
            TriangulateRoad(roadCenter, mL, mR, e, hasRoadThroughEdge, cell.Index);
            if (previousHasRiver)
            {
                TriangulateRoadEdge(roadCenter, center, mL, cell.Index);
            }
            if (nextHasRiver)
            {
                TriangulateRoadEdge(roadCenter, mR, center, cell.Index);
            }

        }

        private void TriangulateWithoutRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
        {
            TriangulateEdgeFan(center, e, cell.Index);
            if (cell.HasRoads)
            {
                Vector2 interpolators = GetRoadInterpolators(direction, cell);
                TriangulateRoad(center, Vector3.Lerp(center, e.v1, interpolators.x), Vector3.Lerp(center, e.v5, interpolators.y), e, cell.HasRoadThroughEdge(direction), cell.Index);
            }
        }

        //三角化道路
        private void TriangulateRoad(Vector3 center, Vector3 mL, Vector3 mR, EdgeVertices e, bool hasRoadThroughCellEdge, float index)
        {
            if (hasRoadThroughCellEdge)
            {
                Vector3 indices;
                indices.x = indices.y = indices.z = index;
                Vector3 mC = Vector3.Lerp(mL, mR, 0.5f);
                TriangulateRoadSegment(mL, mC, mR, e.v2, e.v3, e.v4, weights1, weights1, indices);
                roads.AddTriangle(center, mL, mC);
                roads.AddTriangle(center, mC, mR);
                roads.AddTriangleUV(
                    new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f)
                );
                roads.AddTriangleUV(
                    new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f)
                );
                roads.AddTriangleCellData(indices, weights1);
                roads.AddTriangleCellData(indices, weights1);
            }
            else
            {
                TriangulateRoadEdge(center, mL, mR, index);
            }
        }

        //三角化道路边缘
        private void TriangulateRoadEdge(Vector3 center, Vector3 mL, Vector3 mR, float index)
        {
            roads.AddTriangle(center, mL, mR);
            roads.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            Vector3 indices;
            indices.x = indices.y = indices.z = index;
            roads.AddTriangleCellData(indices, weights1);
        }

        //三角化道路一段（指为四边形的一段)
        private void TriangulateRoadSegment(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Color w1, Color w2, Vector3 indices)
        {
            roads.AddQuad(v1, v2, v4, v5);
            roads.AddQuad(v2, v3, v5, v6);

            roads.AddQuadUV(0f, 1f, 0f, 0f);
            roads.AddQuadUV(1f, 0f, 0f, 0f);

            roads.AddQuadCellData(indices, w1, w2);
            roads.AddQuadCellData(indices, w1, w2);
        }

        //三角化水面//////////////////////////////////
        private void TriangulateWater(HexDirection direction, HexCell cell, Vector3 center)
        {
            center.y = cell.WaterSurfaceY;

            HexCell neighbor = cell.GetNeighbor(direction);
            if (neighbor != null && !neighbor.IsUnderWater)
            {
                TriangulateWaterShore(direction, cell, neighbor, center);
            }
            else
            {
                TriangulateOpenWater(direction, cell, neighbor, center);
            }
        }
        //三角化中间的水域
        private void TriangulateOpenWater(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center)
        {
            Vector3 c1 = center + HexMetrics.GetFirstWaterCorner(direction);
            Vector3 c2 = center + HexMetrics.GetSecondWaterCorner(direction);

            water.AddTriangle(center, c1, c2);
            Vector3 indices;
            indices.x = indices.y = indices.z = cell.Index;
            water.AddTriangleCellData(indices, weights1);
            //如果邻接点也在水面下，则连接水域
            if ((direction <= HexDirection.SE || direction == HexDirection.N) && neighbor != null)
            {

                Vector3 bridge = HexMetrics.GetWaterBridge(direction);
                Vector3 e1 = c1 + bridge;
                Vector3 e2 = c2 + bridge;
                water.AddQuad(c1, c2, e1, e2);
                indices.y = neighbor.Index;
                water.AddQuadCellData(indices, weights1, weights2);
                //处理交界处的三角形
                if ((direction == HexDirection.N || direction == HexDirection.NE))
                {
                    HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
                    if (nextNeighbor == null || !nextNeighbor.IsUnderWater)
                    {
                        return;
                    }
                    water.AddTriangle(c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next()));
                    indices.z = nextNeighbor.Index;
                    water.AddTriangleCellData(indices, weights1, weights2, weights3);
                }
            }
        }

        //三角化海岸
        private void TriangulateWaterShore(HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center)
        {
            EdgeVertices e1 = new EdgeVertices(center + HexMetrics.GetFirstWaterCorner(direction),
            center + HexMetrics.GetSecondWaterCorner(direction));

            water.AddTriangle(center, e1.v1, e1.v2);
            water.AddTriangle(center, e1.v2, e1.v3);
            water.AddTriangle(center, e1.v3, e1.v4);
            water.AddTriangle(center, e1.v4, e1.v5);
            Vector3 indices;
            indices.x = indices.z = cell.Index;
            indices.y = neighbor.Index;
            water.AddTriangleCellData(indices, weights1);
            water.AddTriangleCellData(indices, weights1);
            water.AddTriangleCellData(indices, weights1);
            water.AddTriangleCellData(indices, weights1);

            Vector3 center2 = neighbor.Position;
            center2.y = center.y;
            EdgeVertices e2 = new EdgeVertices(
                center2 + HexMetrics.GetSecondSolidCorner(direction.Opposite()),
                center2 + HexMetrics.GetFirstSolidCorner(direction.Opposite())
            );
            //该方向有河流，则使用河口来过渡
            if (cell.HasRiverThroughEdge(direction))
            {
                TriangulateEstuary(e1, e2, cell.IncomingRiver == direction, indices);
            }
            //否则直接用water材质来填充连接
            else
            {
                waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
                waterShore.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
                waterShore.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
                waterShore.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);
                waterShore.AddQuadUV(0f, 0f, 0f, 1f);

                waterShore.AddQuadCellData(indices, weights1, weights2);
                waterShore.AddQuadCellData(indices, weights1, weights2);
                waterShore.AddQuadCellData(indices, weights1, weights2);
                waterShore.AddQuadCellData(indices, weights1, weights2);
            }
            //填充中间的三角形
            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
            if (nextNeighbor != null)
            {
                //根据是否是否是水来判断是用泡沫还是普通的过度
                Vector3 v3 = nextNeighbor.Position + (nextNeighbor.IsUnderWater ?
                    HexMetrics.GetFirstWaterCorner(direction.Previous()) :
                    HexMetrics.GetFirstSolidCorner(direction.Previous()));
                v3.y = center.y;
                waterShore.AddTriangle(e1.v5, e2.v5, v3);
                waterShore.AddTriangleUV(
                    new Vector2(0f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, nextNeighbor.IsUnderWater ? 0f : 1f)
                );
                indices.z = nextNeighbor.Index;
                waterShore.AddTriangleCellData(
                    indices, weights1, weights2, weights3
                );
            }
        }

        private void TriangulateWaterfallInWater(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float y1, float y2, float waterY, Vector3 indices)
        {
            v1.y = v2.y = y1;
            v3.y = v4.y = y2;

            v1 = HexMetrics.Perturb(v1);
            v2 = HexMetrics.Perturb(v2);
            v3 = HexMetrics.Perturb(v3);
            v4 = HexMetrics.Perturb(v4);
            //向上移动底部顶点，以将河流停止于水面上
            float t = (waterY - y2) / (y1 - y2);
            v3 = Vector3.Lerp(v3, v1, t);
            v4 = Vector3.Lerp(v4, v2, t);
            rivers.AddQuadUnperturbed(v1, v2, v3, v4);
            rivers.AddQuadUV(0f, 1f, 0.8f, 1f);
            rivers.AddQuadCellData(indices, weights1, weights2);
        }

        //三角化入河口
        private void TriangulateEstuary(EdgeVertices e1, EdgeVertices e2, bool incomingRiver, Vector3 indices)
        {
            //两侧按照泡沫
            waterShore.AddTriangle(e2.v1, e1.v2, e1.v1);
            waterShore.AddTriangle(e2.v5, e1.v5, e1.v4);
            waterShore.AddTriangleUV(
                new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f)
            );
            waterShore.AddTriangleUV(
                new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f)
            );
            waterShore.AddTriangleCellData(indices, weights2, weights1, weights1);
            waterShore.AddTriangleCellData(indices, weights2, weights1, weights1);
            //河口的效果/////////////////
            //中心进入部分
            estuaries.AddQuad(e2.v1, e1.v2, e2.v2, e1.v3);
            estuaries.AddTriangle(e1.v3, e2.v2, e2.v4);
            estuaries.AddQuad(e1.v3, e1.v4, e2.v4, e2.v5);
            estuaries.AddQuadUV(
                new Vector2(0f, 1f), new Vector2(0f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 0f)
            );
            estuaries.AddTriangleUV(new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            estuaries.AddQuadUV(
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f)
            );

            estuaries.AddQuadCellData(
                indices, weights2, weights1, weights2, weights1
            );
            estuaries.AddTriangleCellData(indices, weights1, weights2, weights2);
            estuaries.AddQuadCellData(indices, weights1, weights2);

            //添加河口的流动UV动画 
            if (incomingRiver)
            {
                estuaries.AddQuadUV2(
                    new Vector2(1.5f, 1f), new Vector2(0.7f, 1.15f),
                    new Vector2(1f, 0.8f), new Vector2(0.5f, 1.1f)
                );
                estuaries.AddTriangleUV2(
                    new Vector2(0.5f, 1.1f), new Vector2(1f, 0.8f), new Vector2(0f, 0.8f)
                );
                estuaries.AddQuadUV2(
                    new Vector2(0.5f, 1.1f), new Vector2(0.3f, 1.15f),
                    new Vector2(0f, 0.8f), new Vector2(-0.5f, 1f)
                );
            }
            else
            {
                estuaries.AddQuadUV2(
                    new Vector2(-0.5f, -0.2f), new Vector2(0.3f, -0.35f),
                    new Vector2(0f, 0f), new Vector2(0.5f, -0.3f)
                );
                estuaries.AddTriangleUV2(
                    new Vector2(0.5f, -0.3f),
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f)
                );
                estuaries.AddQuadUV2(
                    new Vector2(0.5f, -0.3f), new Vector2(0.7f, -0.35f),
                    new Vector2(1f, 0f), new Vector2(1.5f, -0.2f)
                );
            }

        }

        //基础函数//////////////////////////////////////
        //在中心与边缘创建三角扇
        private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, float index)
        {
            terrain.AddTriangle(center, edge.v1, edge.v2);
            terrain.AddTriangle(center, edge.v2, edge.v3);
            terrain.AddTriangle(center, edge.v3, edge.v4);
            terrain.AddTriangle(center, edge.v4, edge.v5);

            Vector3 indices;
            indices.x = indices.y = indices.z = index;
            terrain.AddTriangleCellData(indices, weights1);
            terrain.AddTriangleCellData(indices, weights1);
            terrain.AddTriangleCellData(indices, weights1);
            terrain.AddTriangleCellData(indices, weights1);
        }

        //对两个边缘之间的四边形三角化
        private void TriangulateEdgeStrip(EdgeVertices e1, Color w1, float index1, EdgeVertices e2, Color w2, float index2, bool hasRoad = false)
        {
            terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);

            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;
            terrain.AddQuadCellData(indices, w1, w2);
            terrain.AddQuadCellData(indices, w1, w2);
            terrain.AddQuadCellData(indices, w1, w2);
            terrain.AddQuadCellData(indices, w1, w2);
            if (hasRoad)
            {
                TriangulateRoadSegment(e1.v2, e1.v3, e1.v4, e2.v2, e2.v3, e2.v4, w1, w2, indices);
            }
        }

        //获取road的插值点（防止拐角处突出）
        private Vector2 GetRoadInterpolators(HexDirection direction, HexCell cell)
        {
            Vector2 interpolators;
            if (cell.HasRoadThroughEdge(direction))
            {
                interpolators.x = interpolators.y = 0.5f;
            }
            else
            {
                interpolators.x = cell.HasRoadThroughEdge(direction.Previous()) ? 0.5f : 0.25f;
                interpolators.y = cell.HasRoadThroughEdge(direction.Next()) ? 0.5f : 0.25f;
            }
            return interpolators;
        }
    }
}