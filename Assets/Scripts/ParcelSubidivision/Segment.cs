using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment
{
    public Vector3 startPos;
    public Vector3 finalPos;

    private float a;
    private float b;
    private float c;

    public Segment()
    {
        startPos = Vector3.zero;
        finalPos = Vector3.zero;
    }

    public Segment(Vector3 stPos, Vector3 endPos)
    {
        startPos = stPos;
        finalPos = endPos;

        UpdateABC();
    }

    public void UpdateABC()
    {
        SetA();
        SetB();
        SetC();
    }

    public Vector3 GetIntersectionPointWithSegment(Segment otherSegment)
    {
        return GetIntersectionPointWithLine(otherSegment);
    }

    public bool DoIntersectWithSegment(Segment otherSegment)
    {
        if (DoIntersectWithLine(otherSegment))
        {
            Vector3 intersectionPoint = GetIntersectionPointWithLine(otherSegment);

            if (PointInSegment(intersectionPoint, otherSegment))
            {
                return true;
            }

            return false;
        }

        return false;
    }

    public float Length()
    {
        return Vector3.Distance(startPos, finalPos);
    }

    private bool DoIntersectWithLine(Segment otherSegment)
    {
        float determinant = a * otherSegment.GetB() - otherSegment.GetA() * b;

		if (determinant == 0)
		{
			Debug.Log("parralel segments");
			return false;
		}

		return true;
    }

    private Vector3 GetIntersectionPointWithLine(Segment otherSegment)
    {
        float determinant = a * otherSegment.GetB() - otherSegment.GetA() * b;

        float x = (otherSegment.GetB() * c - b * otherSegment.GetC()) / determinant;
        float z = (a * otherSegment.GetC() - otherSegment.GetA() * c) / determinant;

        Vector3 intersectionPoint = new Vector3(x, 0, z);

        return intersectionPoint;
    }

    public bool PointInSegment(Vector3 point, Segment seg)
    {
        float minX = Mathf.Min(seg.startPos.x, seg.finalPos.x);
        float minZ = Mathf.Min(seg.startPos.z, seg.finalPos.z);
        float maxX = Mathf.Max(seg.startPos.x, seg.finalPos.x);
        float maxZ = Mathf.Max(seg.startPos.z, seg.finalPos.z);

        if (minX <= point.x && point.x <= maxX && minZ <= point.z && point.z <= maxZ)
            return true;

        return false;
    }

    public Vector3 ProjectPointOnLine(Vector3 p)
    {
        Vector3 a = startPos;
        Vector3 b = finalPos;

        Vector3 ap = a - p;
        Vector3 ab = a - b;

        Vector3 projectedPoint = Vector3.Project(p - a, b - a) + a;

        return projectedPoint;
    }

    #region GettersSetters
    private void SetA()
    {
        a = finalPos.z - startPos.z;
    }

    private void SetB()
    {
        b = startPos.x - finalPos.x;
    }

    private void SetC()
    {
        c = (a * startPos.x + b * startPos.z);
    }

    private float GetA()
    {
        return finalPos.z - startPos.z;
    }

    private float GetB()
    {
        return startPos.x - finalPos.x;
    }

    private float GetC()
    {
        return (a * startPos.x + b * startPos.z);
    }
    #endregion
}
