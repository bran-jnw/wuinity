using static System.Math;
using System.Collections.Generic;

namespace PREACT.Traffic
{
    public struct CellIndex
    {
        public int X;
        public int Y;

        public CellIndex(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    
    public static class EdgeCellIntersection
    {
        //https://github.com/cgyurgyik/fast-voxel-traversal-algorithm/blob/master/overview/FastVoxelTraversalOverview.md
        public static int TraverseCells(double x0, double y0, double x1, double y1, double cellW, double cellH, double minX, double minY, int[] outXs, int[] outYs)
        {
            int ix = (int)Floor((x0 - minX) / cellW);
            int iy = (int)Floor((y0 - minY) / cellH);

            int ixEnd = (int)Floor((x1 - minX) / cellW);
            int iyEnd = (int)Floor((y1 - minY) / cellH);

            double dx = x1 - x0;
            double dy = y1 - y0;

            int stepX = dx > 0.0 ? 1 : -1;
            int stepY = dy > 0.0 ? 1 : -1;

            double tMaxX;
            double tMaxY;
            double tDeltaX;
            double tDeltaY;

            if (dx != 0.0)
            {
                double nextBoundaryX = minX + (ix + (stepX > 0 ? 1 : 0)) * cellW;
                tMaxX = (nextBoundaryX - x0) / dx;
                tDeltaX = cellW / Abs(dx);
            }
            else
            {
                tMaxX = double.PositiveInfinity;
                tDeltaX = double.PositiveInfinity;
            }

            if (dy != 0.0)
            {
                double nextBoundaryY = minY + (iy + (stepY > 0 ? 1 : 0)) * cellH;
                tMaxY = (nextBoundaryY - y0) / dy;
                tDeltaY = cellH / Abs(dy);
            }
            else
            {
                tMaxY = double.PositiveInfinity;
                tDeltaY = double.PositiveInfinity;
            }

            int count = 0;
            outXs[count] = ix;
            outYs[count] = iy;
            count++;

            while (ix != ixEnd || iy != iyEnd)
            {
                if (tMaxX < tMaxY)
                {
                    ix += stepX;
                    tMaxX += tDeltaX;
                }
                else
                {
                    iy += stepY;
                    tMaxY += tDeltaY;
                }

                outXs[count] = ix;
                outYs[count] = iy;
                count++;
            }

            return count;
        }

        public static Dictionary<CellIndex, HashSet<SumoEdge>> SortEdgesIntoCells(Dictionary<string, SumoEdge> edges, double minXPos, double minYPos, double cellSizeX, double cellSizeY, int xDim, int yDim)
        {
            Dictionary<CellIndex, HashSet<SumoEdge>> grid = new Dictionary<CellIndex, HashSet<SumoEdge>>();

            const int maxCells = 4096;
            int[] xs = new int[maxCells];
            int[] ys = new int[maxCells];

            double maxXPos = minXPos + xDim * cellSizeX;
            double maxYPos = minYPos + yDim * cellSizeY;

            foreach (KeyValuePair<string, SumoEdge> kv in edges)
            {
                SumoEdge edge = kv.Value;

                int i = 0;
                while (i < edge.Shape.Count - 1)
                {
                    double x0 = edge.Shape[i].x;
                    double y0 = edge.Shape[i].y;
                    double x1 = edge.Shape[i + 1].x;
                    double y1 = edge.Shape[i + 1].y;


                    // segment bounding box
                    double sMinX = x0 < x1 ? x0 : x1;
                    double sMaxX = x0 > x1 ? x0 : x1;
                    double sMinY = y0 < y1 ? y0 : y1;
                    double sMaxY = y0 > y1 ? y0 : y1;

                    if (sMaxX < minXPos || sMinX > maxXPos || sMaxY < minYPos || sMinY > maxYPos)
                    {
                        i++;
                        continue;
                    }

                    int visited = TraverseCells(x0, y0, x1, y1, cellSizeX, cellSizeY, minXPos, minYPos, xs, ys);

                    int c = 0;
                    while (c < visited)
                    {
                        CellIndex ci = new CellIndex(xs[c], ys[c]);

                        HashSet<SumoEdge> set;
                        if (!grid.TryGetValue(ci, out set))
                        {
                            set = new HashSet<SumoEdge>();
                            grid[ci] = set;
                        }

                        set.Add(edge);
                        c++;
                    }

                    i++;
                }
            }

            return grid;
        }
    }
}  
