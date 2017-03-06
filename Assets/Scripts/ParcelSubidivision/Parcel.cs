using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parcel
{
    public string name;

    public List<Vector3> parcelPoints;
    public List<Segment> parcelSegments;

    public Vector3 topPosMax;
    public Vector3 botPosMax;
    public Vector3 leftPosMax;
    public Vector3 rightPosMax;

    public Vector3 centroid;

    public OBB obb;
    public List<Parcel> subParcels;

    public Parcel(string nam)
    {
        name = nam;
        Debug.Log(name);

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
        if(obb.convexHullPoints.Count == 0)
        {
            Debug.Log("Convex Hull Null");
            return new Vector3(0, 0, 0);
        }

        float centroidX = 0;
        float centroidY = 0;
        float centroidZ = 0;

        for (int i = 0; i < obb.convexHullPoints.Count; i++)
        {
            centroidX += obb.convexHullPoints[i].x;
            centroidY += obb.convexHullPoints[i].y;
            centroidZ += obb.convexHullPoints[i].z;
        }
        //for (int i = 0; i < obb.convexHullSegments.Count; i++)
        //{
        //    centroidX += obb.convexHullSegments[i].startPos.x;
        //    centroidY += obb.convexHullSegments[i].startPos.y;
        //    centroidZ += obb.convexHullSegments[i].startPos.z;
        //}

        centroidX = centroidX / obb.convexHullSegments.Count;
        centroidY = centroidY / obb.convexHullSegments.Count;
        centroidZ = centroidZ / obb.convexHullSegments.Count;

        centroid = new Vector3(centroidX, centroidY, centroidZ);

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
