using M3D.Spooling.Common;
using System;
using System.Collections.Generic;

namespace M3D.Graphics.Widgets2D
{
  internal class BoundrySurface
  {
    public LinkedList<Vector3D> Generate(Vector2D boxLowerMin, Vector2D boxLowerMax, Vector2D boxUpperMin, Vector2D boxUpperMax, float z)
    {
      var linkedList = (LinkedList<Vector3D>) null;
      var sq1 = new BoundrySurface.Square(boxLowerMin, boxLowerMax);
      var sq2 = new BoundrySurface.Square(boxUpperMin, boxUpperMax);
      var flag1 = sq1.contains(sq2);
      var flag2 = sq2.contains(sq1);
      if (flag1 & flag2 || !flag1 && !flag2)
      {
        return (LinkedList<Vector3D>) null;
      }

      if (flag1 | flag2)
      {
        if (flag2)
        {
          BoundrySurface.Square square = sq1;
          sq1 = sq2;
          sq2 = square;
        }
        BoundrySurface.Square[] sqrs = new BoundrySurface.Square[8]{ new BoundrySurface.Square(sq1.min, sq2.min), new BoundrySurface.Square(new Vector2D(sq2.min.x, sq2.min.y), new Vector2D(sq2.max.x, sq1.max.y)), new BoundrySurface.Square(new Vector2D(sq2.max.x, sq1.min.y), new Vector2D(sq2.max.x, sq1.min.y)), new BoundrySurface.Square(new Vector2D(sq1.max.x, sq1.min.y), new Vector2D(sq2.max.x, sq1.max.y)), new BoundrySurface.Square(sq1.max, sq2.max), new BoundrySurface.Square(new Vector2D(sq2.min.x, sq2.max.y), new Vector2D(sq2.max.x, sq1.max.y)), new BoundrySurface.Square(new Vector2D(sq1.min.x, sq2.max.y), new Vector2D(sq2.min.x, sq1.max.y)), new BoundrySurface.Square(new Vector2D(sq1.min.x, sq1.max.y), new Vector2D(sq1.min.x, sq2.max.y)) };
        var list = new LinkedList<Vector3D>();
        AppendTrianglesToList(ref list, sqrs, z);
        return list;
      }
      if (!sq1.intercepts(sq2, out List<Square.SegmentIntersection> segmentIntersections))
      {
        return (LinkedList<Vector3D>)null;
      }

      if (segmentIntersections.Count == 2)
      {
        switch (Math.Abs(segmentIntersections[0].index - segmentIntersections[1].index))
        {
        }
      }
      else
      {
        var count = segmentIntersections.Count;
      }
      return linkedList;
    }

    private void AppendTrianglesToList(ref LinkedList<Vector3D> list, BoundrySurface.Square[] sqrs, float z)
    {
      foreach (BoundrySurface.Square sqr in sqrs)
      {
        AppendTrianglesToList(ref list, sqr, z);
      }
    }

    private void AppendTrianglesToList(ref LinkedList<Vector3D> list, BoundrySurface.Square s, float z)
    {
      list.AddLast(new Vector3D(s.min.x, s.min.y, z));
      list.AddLast(new Vector3D(s.max.x, s.min.y, z));
      list.AddLast(new Vector3D(s.min.x, s.max.y, z));
      list.AddLast(new Vector3D(s.min.x, s.max.y, z));
      list.AddLast(new Vector3D(s.max.x, s.min.y, z));
      list.AddLast(new Vector3D(s.max.x, s.max.y, z));
    }

    private class Square
    {
      public Vector2D min;
      public Vector2D max;
      public BoundrySurface.Segment[] Sides;

      public Square(Vector2D min, Vector2D max)
      {
        this.min = min;
        this.max = max;
        Sides = new BoundrySurface.Segment[4]
        {
          new BoundrySurface.Segment(new Vector2D(min.x, min.y), new Vector2D(max.x, min.y)),
          new BoundrySurface.Segment(new Vector2D(max.x, min.y), new Vector2D(max.x, max.y)),
          new BoundrySurface.Segment(new Vector2D(max.x, max.y), new Vector2D(min.x, max.y)),
          new BoundrySurface.Segment(new Vector2D(min.x, max.y), new Vector2D(min.x, min.y))
        };
      }

      public bool intercepts(BoundrySurface.Square sq, out List<BoundrySurface.Square.SegmentIntersection> segmentIntersections)
      {
        segmentIntersections = new List<BoundrySurface.Square.SegmentIntersection>();
        for (var index1 = 0; index1 < Sides.Length; ++index1)
        {
          for (var index2 = 0; index2 < sq.Sides.Length; ++index2)
          {
            if (Sides[index1].intercepts(sq.Sides[index2], out Vector2D? pt))
            {
              segmentIntersections.Add(new BoundrySurface.Square.SegmentIntersection(index1, pt.Value));
            }
          }
        }
        return (uint) segmentIntersections.Count > 0U;
      }

      public bool contains(BoundrySurface.Square sq)
      {
        if ((double)min.x <= (double) sq.min.x && (double)min.y <= (double) sq.min.y && (double)max.x >= (double) sq.max.x)
        {
          return (double)max.y >= (double) sq.max.y;
        }

        return false;
      }

      public class SegmentIntersection
      {
        public int index;
        public Vector2D intersection;

        public SegmentIntersection(int index, Vector2D intersection)
        {
          this.index = index;
          this.intersection = intersection;
        }
      }
    }

    private class Segment
    {
      public Vector2D min;
      public Vector2D max;

      public Segment(Vector2D min, Vector2D max)
      {
        this.min = min;
        this.max = max;
      }

      public bool intercepts(BoundrySurface.Segment s, out Vector2D? pt)
      {
        pt = new Vector2D?();
        var num1 = max.x - min.x;
        var num2 = max.y - min.y;
        var num3 = s.min.x - min.x;
        var num4 = s.min.y - min.y;
        var num5 = s.max.x - s.min.x;
        var num6 = s.max.y - s.min.y;
        var num7 = (double) num1 * (double) num4 - (double) num3 * (double) num2;
        var num8 = (float) ((double) num5 * (double) num2 - (double) num1 * (double) num6);
        var num9 = (double) num8;
        var num10 = (float) (num7 / num9);
        pt = (double) num8 >= 1.40129846432482E-45 || (double) num8 <= -1.40129846432482E-45 ? ((double) num10 >= 1.40129846432482E-45 || (double) num10 <= -1.40129846432482E-45 ? ((double) num10 >= 1.0 || (double) num10 <= 1.0 ? ((double) num10 >= 1.0 || (double) num10 <= 0.0 ? new Vector2D?() : new Vector2D?(new Vector2D(min.x + num1 * num10, min.y + num2 * num10))) : new Vector2D?(new Vector2D(max))) : new Vector2D?(new Vector2D(min))) : new Vector2D?();
        return pt.HasValue;
      }
    }
  }
}
