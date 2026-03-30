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

    public struct Rect
    {
        public double MinX, MinY;
        public double MaxX, MaxY;

        public Rect(double minX, double minY, double maxX, double maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public bool Contains(double x, double y)
        {
            return (x >= MinX && x <= MaxX && y >= MinY && y <= MaxY);
        }
    }
    
    public static class EdgeCellIntersection
    {
        public static bool SegmentsIntersect(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            double d = (x2 - x1) * (y4 - y3) - (y2 - y1) * (x4 - x3);
            if (Abs(d) < 1e-12)
            {
                return false; // parallel
            }

            double u = ((x3 - x1) * (y4 - y3) - (y3 - y1) * (x4 - x3)) / d;
            double v = ((x3 - x1) * (y2 - y1) - (y3 - y1) * (x2 - x1)) / d;

            return (u >= 0.0 && u <= 1.0 && v >= 0.0 && v <= 1.0);
        }

        public static bool LineIntersectsRect(double x1, double y1, double x2, double y2, Rect r)
        {
            // Quick reject
            if ((x1 < r.MinX && x2 < r.MinX) ||
                (x1 > r.MaxX && x2 > r.MaxX) ||
                (y1 < r.MinY && y2 < r.MinY) ||
                (y1 > r.MaxY && y2 > r.MaxY))
            {
                return false;
            }

            // If any endpoint is inside rectangle
            if (r.Contains(x1, y1) || r.Contains(x2, y2))
            {
                return true;
            }

            // Check against each edge of the rectangle
            if (SegmentsIntersect(x1, y1, x2, y2, r.MinX, r.MinY, r.MaxX, r.MinY)) return true;
            if (SegmentsIntersect(x1, y1, x2, y2, r.MaxX, r.MinY, r.MaxX, r.MaxY)) return true;
            if (SegmentsIntersect(x1, y1, x2, y2, r.MaxX, r.MaxY, r.MinX, r.MaxY)) return true;
            if (SegmentsIntersect(x1, y1, x2, y2, r.MinX, r.MaxY, r.MinX, r.MinY)) return true;

            return false;
        }

        public static Dictionary<CellIndex, List<SumoEdge>> SortEdgesIntoCells(Dictionary<string, SumoEdge> edges, double minXPos, double minYPos, double cellW, double cellH, int xDim, int yDim)
        {
            Dictionary<CellIndex, List<SumoEdge>> grid = new Dictionary<CellIndex, List<SumoEdge>>();

            foreach (KeyValuePair<string, SumoEdge> pair in edges)
            {
                SumoEdge edge = pair.Value;

                if (edge.Shape.Count < 2)
                {
                    continue; // cannot compute geometry
                }

                // Compute bounding box for the edge shape
                double xs = edge.Shape[0].x;
                double ys = edge.Shape[0].y;
                double xe = xs;
                double ye = ys;

                int i = 1;
                while (i < edge.Shape.Count)
                {
                    double x = edge.Shape[i].x;
                    double y = edge.Shape[i].y;

                    if (x < xs) xs = x;
                    if (x > xe) xe = x;
                    if (y < ys) ys = y;
                    if (y > ye) ye = y;

                    i++;
                }

                int cellX0 = (int)Floor((xs - minXPos) / cellW);
                int cellY0 = (int)Floor((ys - minYPos) / cellH);
                int cellX1 = (int)Floor((xe - minXPos) / cellW);
                int cellY1 = (int)Floor((ye - minYPos) / cellH);

                //check if some part is inside, else loop
                int outside = 0;
                if(cellX0 < 0 || cellX0 > xDim - 1) ++outside;
                if(cellX1 < 0 || cellX1 > xDim - 1) ++outside;
                if(cellY0 < 0 || cellY0 > yDim - 1) ++outside;
                if(cellY1 < 0 || cellY1 > yDim - 1) ++outside;
                if(outside == 4)
                {
                    continue;
                }

                int cx = cellX0;
                while (cx <= cellX1)
                {
                    int cy = cellY0;
                    while (cy <= cellY1)
                    {
                        Rect rect = new Rect(
                            minXPos + cx * cellW,
                            minYPos + cy * cellH,
                            minXPos + (cx + 1) * cellW,
                            minYPos + (cy + 1) * cellH
                        );

                        bool touches = false;

                        int s = 0;
                        while (s < edge.Shape.Count - 1)
                        {
                            double x1 = edge.Shape[s].x;
                            double y1 = edge.Shape[s].y;
                            double x2 = edge.Shape[s + 1].x;
                            double y2 = edge.Shape[s + 1].y;

                            if (LineIntersectsRect(x1, y1, x2, y2, rect))
                            {
                                touches = true;
                                break;
                            }
                            s++;
                        }

                        if (touches)
                        {
                            CellIndex idx = new CellIndex(cx, cy);

                            if (!grid.ContainsKey(idx))
                            {
                                grid[idx] = new List<SumoEdge>();
                            }
                            grid[idx].Add(edge);
                        }

                        cy++;
                    }
                    cx++;
                }
            }

            return grid;
        }
    }
}  
