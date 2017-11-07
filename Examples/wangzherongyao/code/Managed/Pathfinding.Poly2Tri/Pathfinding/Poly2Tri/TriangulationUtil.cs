namespace Pathfinding.Poly2Tri
{
    using System;

    public class TriangulationUtil
    {
        public static double EPSILON = 1E-12;

        public static bool InScanArea(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc, TriangulationPoint pd)
        {
            double x = pd.X;
            double y = pd.Y;
            double num3 = pa.X - x;
            double num4 = pa.Y - y;
            double num5 = pb.X - x;
            double num6 = pb.Y - y;
            double num7 = num3 * num6;
            double num8 = num5 * num4;
            double num9 = num7 - num8;
            if (num9 <= 0.0)
            {
                return false;
            }
            double num10 = pc.X - x;
            double num11 = pc.Y - y;
            double num12 = num10 * num4;
            double num13 = num3 * num11;
            double num14 = num12 - num13;
            if (num14 <= 0.0)
            {
                return false;
            }
            return true;
        }

        public static Orientation Orient2d(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc)
        {
            double num = (pa.X - pc.X) * (pb.Y - pc.Y);
            double num2 = (pa.Y - pc.Y) * (pb.X - pc.X);
            double num3 = num - num2;
            if ((num3 > -EPSILON) && (num3 < EPSILON))
            {
                return Orientation.Collinear;
            }
            if (num3 > 0.0)
            {
                return Orientation.CCW;
            }
            return Orientation.CW;
        }

        public static bool SmartIncircle(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc, TriangulationPoint pd)
        {
            double x = pd.X;
            double y = pd.Y;
            double num3 = pa.X - x;
            double num4 = pa.Y - y;
            double num5 = pb.X - x;
            double num6 = pb.Y - y;
            double num7 = num3 * num6;
            double num8 = num5 * num4;
            double num9 = num7 - num8;
            if (num9 <= 0.0)
            {
                return false;
            }
            double num10 = pc.X - x;
            double num11 = pc.Y - y;
            double num12 = num10 * num4;
            double num13 = num3 * num11;
            double num14 = num12 - num13;
            if (num14 <= 0.0)
            {
                return false;
            }
            double num15 = num5 * num11;
            double num16 = num10 * num6;
            double num17 = (num3 * num3) + (num4 * num4);
            double num18 = (num5 * num5) + (num6 * num6);
            double num19 = (num10 * num10) + (num11 * num11);
            double num20 = ((num17 * (num15 - num16)) + (num18 * num14)) + (num19 * num9);
            return (num20 > 0.0);
        }
    }
}

