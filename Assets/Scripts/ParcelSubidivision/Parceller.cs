using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parceller
{

    public Parcel parcel;

    // List of sub parcels after subdivision
    private List<Parcel> subParcels;

    // List of intersection points between parcel and its obb
    private List<Vector3> intersectionPoints;

    public Parceller(Parcel parcel)
    {
        intersectionPoints = new List<Vector3>();
        subParcels = new List<Parcel>();

        parcel.obb.FindObbLTTS(parcel);

        this.parcel = parcel;
    }

    public void SubdivideParcel(int iter)
    {
        FindIntersectionPoints();
        GenerateSubParcels(iter);
    }

    private void GenerateSubParcels(int iter)
    {
        //Horizontal Cut
        if (parcel.obb.CheckCutAxis())
        {
            subParcels.Add(FindSubParcelStartingFromPoint(parcel.topPosMax, 0, iter));
            subParcels.Add(FindSubParcelStartingFromPoint(parcel.botPosMax, 1, iter));
        }
        // Vertical
        else
        {
            subParcels.Add(FindSubParcelStartingFromPoint(parcel.rightPosMax, 0, iter));
            subParcels.Add(FindSubParcelStartingFromPoint(parcel.leftPosMax, 1, iter));
        }
        parcel.subParcels = subParcels;
    }

    // Find sub parcel starting from extrema point (top most, bot most, right most, left most)
    private Parcel FindSubParcelStartingFromPoint(Vector3 point, int subParcelNumber, int iter)
    {
        // Generate Sub Parcel
        Parcel subParcel = new Parcel(parcel.name + "-" + subParcelNumber.ToString(), iter);

        bool pointFound = false;
        int pointIndex = 0;

        // Find extrema point in parcel points and keep track of its index
        // We will use that index to start iterating through the parcel points
        for (int i = 0; i < parcel.parcelPoints.Count; i++)
        {
            if (Vector3.Distance(point, parcel.parcelPoints[i]) <= 0.001f)
            {
                pointFound = true;
                pointIndex = i;
            }
        }

        // Counter for parcel points iteration
        int counter = 0;

        Vector3 firstIntersectionPoint = Vector3.zero;
        bool lookingForSecondIntersectionPoint = false;

        if (pointFound)
        {
            while (counter < parcel.parcelPoints.Count)
            {
                // The current point and the point after it form the segment for the intersection check
                int nextPointIndex = (pointIndex + 1) % parcel.parcelPoints.Count;

                Vector3 nextPoint = parcel.parcelPoints[nextPointIndex];

                // Temporary segment for intersection check
                Segment tempSeg;
                if (counter == 0)
                {
                    // First segment created, we use the point of reference (extrema)
                    tempSeg = new Segment(point, nextPoint);
                }
                else
                {
                    tempSeg = new Segment(parcel.parcelPoints[pointIndex], nextPoint);
                }

                if (!lookingForSecondIntersectionPoint)
                {
                    for (int i = 0; i < intersectionPoints.Count; i++)
                    {
                        // If intersection point is included in current temporary segment
                        if (tempSeg.PointInSegment(intersectionPoints[i], tempSeg))
                        {
                            // The end position for the temporary segment becomes the intersection point
                            tempSeg.finalPos = intersectionPoints[i];

                            // Store the intersection point  
                            // To be used with next intersection point to create the new segment for sub parcel
                            firstIntersectionPoint = intersectionPoints[i];
                            lookingForSecondIntersectionPoint = true;

                            break;
                        }
                    }

                    subParcel.parcelSegments.Add(tempSeg);
                }
                else
                {
                    for (int i = 0; i < intersectionPoints.Count; i++)
                    {
                        if (tempSeg.PointInSegment(intersectionPoints[i], tempSeg))
                        {
                            lookingForSecondIntersectionPoint = false;

                            // Add the segment between the two intersection points
                            Segment intersectSegment = new Segment(firstIntersectionPoint, intersectionPoints[i]);
                            subParcel.parcelSegments.Add(intersectSegment);

                            // Add the continuity segment starting from the second intersection point
                            Segment continuitySegment = new Segment(intersectionPoints[i], nextPoint);
                            subParcel.parcelSegments.Add(continuitySegment);
                        }
                    }
                }

                pointIndex = (pointIndex + 1) % parcel.parcelPoints.Count;
                counter++;
            }
        }
        else
        {
            Debug.Log("Point: " + point + " Not Found in " + parcel.name + "-" + subParcelNumber.ToString());
        }

        subParcel.UpdateParcelPointsFromSegments();
        return subParcel;
    }

    private void FindIntersectionPoints()
    {
        parcel.obb.CheckCutAxis();
        Segment halfObbSegment = new Segment(parcel.obb.cutStartPoint, parcel.obb.cutEndPoint);

        for (int i = 0; i < parcel.parcelSegments.Count; i++)
        {
            if (halfObbSegment.DoIntersectWithSegment(parcel.parcelSegments[i]))
            {
                Vector3 intersectionPoint = halfObbSegment.GetIntersectionPointWithSegment(parcel.parcelSegments[i]);
                intersectionPoints.Add(intersectionPoint);
            }
        }

        //Debug.Log("intersection count : " + intersectionPoints.Count);
    }
}
