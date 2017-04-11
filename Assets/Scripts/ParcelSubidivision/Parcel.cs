using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parcel
{
    public string name;
    public int iteration;

    public List<Vector3> parcelPoints;
    public List<Segment> parcelSegments;

    public Vector3 topPosMax;
    public Vector3 botPosMax;
    public Vector3 leftPosMax;
    public Vector3 rightPosMax;

    public Vector3 centroid;

    public OBB obb;
    public List<Parcel> subParcels;

    public Parcel(string nam, int iter)
    {
        name = nam;
        iteration = iter;

        parcelPoints = new List<Vector3>();
        parcelSegments = new List<Segment>();

        topPosMax = Vector3.zero;
        botPosMax = Vector3.zero;
        leftPosMax = Vector3.zero;
        rightPosMax = Vector3.zero;

        centroid = Vector3.zero;

        obb = new OBB();
        subParcels = new List<Parcel>();
    }

    public Vector3 FindParcelCentroid()
    {
        obb.FindObbLTTS(this);

        centroid = Vector3.zero;
        if (obb.convexHullPoints.Count == 0)
        {
            Debug.Log("Convex Hull Null");
            return centroid;
        }
        
        for (int i = 0; i < obb.convexHullPoints.Count; i++)
        {
            centroid += obb.convexHullPoints[i];
        }

        centroid = centroid / obb.convexHullPoints.Count;

        return centroid;
    }

    public void UpdateParcelPointsFromSegments()
    {
        if (parcelPoints.Count <= 0)
        {
            for (int i = 0; i < parcelSegments.Count; i++)
            {
                parcelPoints.Add(parcelSegments[i].startPos);
            }
        }
        else
        {
            for (int i = 0; i < parcelSegments.Count; i++)
            {
                parcelPoints[i] = parcelSegments[i].startPos;
            }
        }
    }
}
