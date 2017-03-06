using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBB
{
    public Rectangle obbRect;

    //public Vector3 topLeftCorner, topRightCorner, botLeftCorner, botRightCorner;
    //public Segment topSegment, botSegment, leftSegment, rightSegment;

    public List<Vector3> convexHullPoints;
    public List<Segment> convexHullSegments;

    public Vector3 cutStartPoint, cutEndPoint;

    // Split parcel following horizontal or vertical axis
    private bool cutHorizontalAxis;

    public OBB()
    {
        obbRect = new Rectangle();

        convexHullPoints = new List<Vector3>();
        convexHullSegments = new List<Segment>();

        cutHorizontalAxis = true;
        cutStartPoint = Vector3.zero;
        cutEndPoint = Vector3.zero;
    }

    public bool CheckCutAxis()
    {
        if (obbRect.topSegment.Length() >= obbRect.rightSegment.Length())
        {
            cutHorizontalAxis = false;
            cutStartPoint = (obbRect.leftTop + obbRect.rightTop) / 2;
            cutEndPoint = (obbRect.rightBot + obbRect.leftBot) / 2;
        }
        else
        {
            cutHorizontalAxis = true;
            cutStartPoint = (obbRect.leftBot + obbRect.leftTop) / 2;
            cutEndPoint = (obbRect.rightTop + obbRect.rightBot) / 2;
        }

        return cutHorizontalAxis;
    }

    public void SwapCutAxis()
    {
        cutHorizontalAxis = !cutHorizontalAxis;
        if (!cutHorizontalAxis)
        {
            cutStartPoint = (obbRect.leftTop + obbRect.rightTop) / 2;
            cutEndPoint = (obbRect.rightBot + obbRect.leftBot) / 2;
        }
        else
        {
            cutStartPoint = (obbRect.leftBot + obbRect.leftTop) / 2;
            cutEndPoint = (obbRect.rightTop + obbRect.rightBot) / 2;
        }
    }

    // Long live the square algorithm (LLTS)
    public Rectangle FindObbLTTS(Parcel parcel)
    {
        FindConvexHull(parcel);

        if (convexHullPoints.Count < 3)
            return null;

        Rectangle minBox = new Rectangle(new Vector3(float.MinValue, 0, float.MinValue), new Vector3(float.MaxValue, 0, float.MaxValue));
        minBox.UpdateSegments();

        float minAngle = 0f;

        for (int i = 0; i < convexHullPoints.Count; i++)
        {
            Segment segment = convexHullSegments[i];

            // min & max points
            float top = float.MinValue;
            float bot = float.MaxValue;
            float right = float.MinValue;
            float left = float.MaxValue;

            // get angle of segment to x axis
            float angle = AngleToXAxis(segment);

            Vector3 rotatedTopMostPoint = RotateToXAxis(parcel.topPosMax, angle);
            Vector3 rotatedBotMostPoint = RotateToXAxis(parcel.botPosMax, angle);
            Vector3 rotatedRightMostPoint = RotateToXAxis(parcel.rightPosMax, angle);
            Vector3 rotatedLeftMostPoint = RotateToXAxis(parcel.leftPosMax, angle);

            // rotate every point in the hull and get min and max values for each direction
            foreach (Vector3 p in convexHullPoints)
            {
                Vector3 rotatedPoint = RotateToXAxis(p, angle);

                top = Mathf.Max(top, rotatedPoint.z);
                bot = Mathf.Min(bot, rotatedPoint.z);
                right = Mathf.Max(right, rotatedPoint.x);
                left = Mathf.Min(left, rotatedPoint.x);

                rotatedTopMostPoint = (rotatedTopMostPoint.z >= rotatedPoint.z) ? rotatedTopMostPoint : rotatedPoint;
                rotatedBotMostPoint = (rotatedBotMostPoint.z < rotatedPoint.z) ? rotatedBotMostPoint : rotatedPoint;
                rotatedRightMostPoint = (rotatedRightMostPoint.x >= rotatedPoint.x) ? rotatedRightMostPoint : rotatedPoint;
                rotatedLeftMostPoint = (rotatedLeftMostPoint.x < rotatedPoint.x) ? rotatedLeftMostPoint : rotatedPoint;
            }

            // create axis aligned bounding box
            Rectangle rect = new Rectangle(new Vector3(left, 0, bot), new Vector3(right, 0, top));
            rect.UpdateSegments();

            if (minBox.GetArea() > rect.GetArea())
            {
                minBox = rect;
                minAngle = angle;

                parcel.topPosMax = RotateToXAxis(rotatedTopMostPoint, -angle);
                parcel.botPosMax = RotateToXAxis(rotatedBotMostPoint, -angle);
                parcel.rightPosMax = RotateToXAxis(rotatedRightMostPoint, -angle);
                parcel.leftPosMax = RotateToXAxis(rotatedLeftMostPoint, -angle);
            }
        }

        // rotate axis aligned box back
        minBox.leftBot = RotateToXAxis(minBox.leftBot, -minAngle);
        minBox.leftTop = RotateToXAxis(minBox.leftTop, -minAngle);
        minBox.rightBot = RotateToXAxis(minBox.rightBot, -minAngle);
        minBox.rightTop = RotateToXAxis(minBox.rightTop, -minAngle);

        minBox.UpdateSegments();

        obbRect = minBox;
        CheckCutAxis();

        return minBox;
    }

    private void FindConvexHull(Parcel parcel)
    {
        int n = parcel.parcelPoints.Count;
        if (n < 3)
            return;

        // Initialize result
        int[] next = new int[n];
        for (int i = 0; i < n; i++)
        {
            next[i] = -1;
        }

        // Find the left most point
        int l = 0;
        for (int i = l; i < n; i++)
        {
            if (parcel.parcelPoints[i].x < parcel.parcelPoints[l].x)
            {
                l = i;
            }
        }

        // Start from leftmost point, keep moving counterclockwise
        // until reach the start point again
        int p = l;
        int q;
		
        do
        {
            // Search for a point 'q' such that orientation(p, i, q) is
            // counterclockwise for all points 'i'
            q = (p + 1) % n;
            for (int i = 0; i < n; i++)
                if (orientation(parcel.parcelPoints[p], parcel.parcelPoints[i], parcel.parcelPoints[q]) == 2)
					q = i;

            next[p] = q; // Add q to result as a next point of p
            p = q; // Set p as q for next iteration
        }
        while (p != l);

        for (int i = 0; i < n; i++)
        {
            if (next[i] != -1)
            {
                convexHullPoints.Add(parcel.parcelPoints[i]);
            }
        }

        for (int i = 0; i < parcel.obb.convexHullPoints.Count; i++)
        {
            Segment seg;
            if (i != parcel.obb.convexHullPoints.Count - 1)
            {
                seg = new Segment(parcel.obb.convexHullPoints[i], parcel.obb.convexHullPoints[i + 1]);
            }
            else
            {
                seg = new Segment(parcel.obb.convexHullPoints[i], parcel.obb.convexHullPoints[0]);
            }
            convexHullSegments.Add(seg);
        }
    }

    private float AngleToXAxis(Segment s)
    {
        Vector3 delta = s.startPos - s.finalPos;

        return -Mathf.Atan(delta.z / delta.x);
    }

    private Vector3 RotateToXAxis(Vector3 v, float angle)
    {
        float newX = v.x * Mathf.Cos(angle) - v.z * Mathf.Sin(angle);
        float newZ = v.x * Mathf.Sin(angle) + v.z * Mathf.Cos(angle);

        return new Vector3(newX, 0, newZ);
    }

    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are colinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    private int orientation(Vector3 p, Vector3 q, Vector3 r)
    {
        int val = (int)((q.z - p.z) * (r.x - q.x) - (q.x - p.x) * (r.z - q.z));

        if (val == 0)
            return 0; // colinear
        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }
}
