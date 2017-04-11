using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParcelGenerationController : MonoBehaviour
{
    // *************** Publics **********************
    [Range(20, 200)]
    public float range;
    [Range(3, 10)]
    public int numPoints;
    [Range(0, 4)]
    public int numSubdivisions;

    //public bool showPlot;
    //public bool showConvexHull;

    public GameObject pointPrefab;
    public GameObject roadLineRenderer;
    public GameObject subParcelLineRenderer;
    public GameObject obbLineRenderer;
    [HideInInspector]
    public List<Vector3> centroidsLocations;

    // **************** Privates ********************
    private Parcel parcel = null;
    private GameObject[] plotPoints;

    private GameObject plotHolder;
    private GameObject plotPointsHolder;
    private GameObject convexHullHolder;
    private GameObject plotsHolder;
    private GameObject centroidsHolder;
    private GameObject obbRect;

    private bool obbRectIsShown = false;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Gizmos.DrawSphere(transform.position, range);
    }

    public void GenerateParcel()
    {
        centroidsLocations = new List<Vector3>();

        //if (plotHolder == null)
        //{
        //    plotHolder = new GameObject("Plot Holder");
        //    plotHolder.tag = "PlotHolder";
        //    plotHolder.transform.parent = this.transform;
        //}

        if (plotsHolder == null)
        {
            plotsHolder = new GameObject("Plots Holder");
            plotsHolder.tag = "PlotsHolder";
            plotsHolder.transform.parent = this.transform;
        }

        if (centroidsHolder == null)
        {
            centroidsHolder = new GameObject("Centroids Holder");
            centroidsHolder.tag = "CentroidsHolder";
            centroidsHolder.transform.parent = this.transform;
        }

        if (parcel == null)
        {
            List<Vector3> parcelPoints = new List<Vector3>();
            parcelPoints = GeneratePolygon();

            parcel = CreateParcelFromPoints(parcelPoints);
            parcel.obb.FindObbLTTS(parcel);
            GeneratePlot();
            GenerateConvexHull();

            ShowPlot();
            ShowConvexHull();
        }
        else
        {
            ClearData();
            GenerateParcel();
        }
    }

    public void UpdateParcel()
    {
        if (plotPointsHolder)
        {
            List<Vector3> updatedPointsLocations = new List<Vector3>();
            for (int i = 0; i < plotPointsHolder.transform.childCount; i++)
            {
                updatedPointsLocations.Add(transform.FindChild("Plot Points Holder").GetChild(i).position);
            }

            Debug.Log("num convex hull points: " + parcel.obb.convexHullPoints.Count);

            parcel = CreateParcelFromPoints(updatedPointsLocations);
            parcel.obb.FindObbLTTS(parcel);

            foreach (Vector3 pos in parcel.obb.convexHullPoints)
            {
                Debug.Log("pos: " + pos);
            }
            Debug.Log("num convex hull points: " + parcel.obb.convexHullPoints.Count);

            ClearConvexHull();
            GenerateConvexHull();

            if (obbRectIsShown)
            {
                HideOBBRect();
                ShowOBBRect();
            }

            obbRect = null;
        }
        Debug.Log("Parcel Points Updated!");
    }

    public void ClearData()
    {
        ClearPlotPoints();
        ClearPlot();
        ClearConvexHull();

        plotsHolder = null;
        Transform plotsHolderToClear = transform.FindChild("Plots Holder");
        if (plotsHolderToClear)
            DestroyImmediate(plotsHolderToClear.gameObject);

        centroidsHolder = null;
        Transform centroidsHolderToClear = transform.FindChild("Centroids Holder");
        if (centroidsHolderToClear)
            DestroyImmediate(centroidsHolderToClear.gameObject);

        HideOBBRect();

        centroidsLocations = new List<Vector3>();

        parcel = null;
    }

    public void GenerateSubParcels()
    {
        RecursiveParceller(parcel, numSubdivisions);
    }

    public void ShowOBBRect()
    {
        if (obbRect == null)
        {
            obbRectIsShown = true;
            obbRect = new GameObject("OBB Rect");
            obbRect.transform.parent = this.transform;

            Rectangle rect = parcel.obb.obbRect;

            GameObject lineTop = (GameObject)Instantiate(obbLineRenderer);
            lineTop.transform.parent = obbRect.transform;
            GameObject lineBot = (GameObject)Instantiate(obbLineRenderer);
            lineBot.transform.parent = obbRect.transform;
            GameObject lineLeft = (GameObject)Instantiate(obbLineRenderer);
            lineLeft.transform.parent = obbRect.transform;
            GameObject lineRight = (GameObject)Instantiate(obbLineRenderer);
            lineRight.transform.parent = obbRect.transform;

            lineTop.GetComponent<LineRenderer>().SetPosition(0, rect.topSegment.startPos);
            lineTop.GetComponent<LineRenderer>().SetPosition(1, rect.topSegment.finalPos);

            lineBot.GetComponent<LineRenderer>().SetPosition(0, rect.botSegment.startPos);
            lineBot.GetComponent<LineRenderer>().SetPosition(1, rect.botSegment.finalPos);

            lineLeft.GetComponent<LineRenderer>().SetPosition(0, rect.leftSegment.startPos);
            lineLeft.GetComponent<LineRenderer>().SetPosition(1, rect.leftSegment.finalPos);

            lineRight.GetComponent<LineRenderer>().SetPosition(0, rect.rightSegment.startPos);
            lineRight.GetComponent<LineRenderer>().SetPosition(1, rect.rightSegment.finalPos);
        }
    }

    public void HideOBBRect()
    {
        obbRect = null;
        Transform obbRectToClear = transform.FindChild("OBB Rect");
        if (obbRectToClear)
            DestroyImmediate(obbRectToClear.gameObject);
        obbRectIsShown = false;
    }

    private void RecursiveParceller(Parcel parcel, int iter)
    {
        List<Parcel> subParcels = new List<Parcel>();
        subParcels = UseParceller(parcel, iter);

        if (parcel.obb.obbRect.area > 0)
        {
            if (iter > 0)
            {
                for (int i = 0; i < subParcels.Count; i++)
                {
                    RecursiveParceller(subParcels[i], iter - 1);
                }
            }
        }
    }

    private List<Parcel> UseParceller(Parcel parcel, int iter)
    {
        Parceller parceller = new Parceller(parcel);
        parceller.SubdivideParcel(iter);
        DrawSubParcels(parceller.parcel);

        return parceller.parcel.subParcels;
    }

    private void DrawSubParcels(Parcel parcel)
    {
        for (int j = 0; j < parcel.subParcels.Count; j++)
        {
            if (parcel.subParcels[j].parcelSegments.Count != 0)
            {
                GameObject subParcel = new GameObject("Sub Parcel " + parcel.subParcels[j].name);
                subParcel.transform.parent = plotsHolder.transform;
                for (int i = 0; i < parcel.subParcels[j].parcelSegments.Count; i++)
                {
                    GameObject line = Instantiate(subParcelLineRenderer, parcel.subParcels[j].parcelSegments[i].startPos, Quaternion.identity);
                    line.GetComponent<LineRenderer>().SetPosition(0, parcel.subParcels[j].parcelSegments[i].startPos);
                    line.GetComponent<LineRenderer>().SetPosition(1, parcel.subParcels[j].parcelSegments[i].finalPos);
                    line.transform.parent = subParcel.transform;
                }

                if (parcel.subParcels[j].iteration == 0)
                {
                    GameObject centroid = Instantiate(pointPrefab, parcel.subParcels[j].FindParcelCentroid(), Quaternion.identity);
                    centroid.transform.parent = subParcel.transform;
                    centroidsLocations.Add(centroid.transform.position);
                }
            }
        }
    }

    private Parcel CreateParcelFromPoints(List<Vector3> points)
    {
        parcel = new Parcel((0).ToString(), 0);
        Debug.Log("POINTS COUNT : " + points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            int nextIndex = (i + 1) % points.Count;
            Segment seg = new Segment(points[i], points[nextIndex]);
            parcel.parcelSegments.Add(seg);
        }
        parcel.UpdateParcelPointsFromSegments();

        return parcel;
    }

    public void GenerateConvexHull()
    {
        if (convexHullHolder == null)
        {
            //Debug.Log("convex hull points count: " + parcel.obb.convexHullPoints.Count);
            //Debug.Log("convex hull segments count: " + parcel.obb.convexHullSegments.Count);

            convexHullHolder = new GameObject("Convex Hull Holder");
            convexHullHolder.transform.parent = this.transform;

            foreach (Segment seg in parcel.obb.convexHullSegments)
            {
                GameObject line = (GameObject)Instantiate(roadLineRenderer);
                line.transform.parent = convexHullHolder.transform;
                line.GetComponent<LineRenderer>().SetPosition(0, seg.startPos);
                line.GetComponent<LineRenderer>().SetPosition(1, seg.finalPos);

                line.GetComponent<RoadParameters>().enabled = false;
            }
        }
    }

    public void ClearConvexHull()
    {
        convexHullHolder = null;
        Transform convexHullHolderToClear = transform.FindChild("Convex Hull Holder");
        if (convexHullHolderToClear)
            DestroyImmediate(convexHullHolderToClear.gameObject);
    }

    public void ShowConvexHull()
    {
        convexHullHolder.SetActive(true);
    }

    public void HideConvexHull()
    {
        convexHullHolder.SetActive(false);
    }


    public void GeneratePlot()
    {
        if (plotHolder == null)
        {
            Debug.Log("PLOT HOLDER: " + plotHolder);

            plotHolder = new GameObject("Plot Holder");
            plotHolder.tag = "PlotHolder";
            plotHolder.transform.parent = this.transform;
        }

        int i = 0;
        foreach (Segment seg in parcel.obb.convexHullSegments)
        {
            GameObject line = (GameObject)Instantiate(roadLineRenderer);
            line.transform.parent = plotHolder.transform;

            RoadParameters roadParams = line.GetComponent<RoadParameters>();
            roadParams.SetInitialPoint(plotPoints[i]);
            roadParams.SetNextPoint(plotPoints[(i + 1) % numPoints]);

            i++;
        }
    }

    public void ShowPlot()
    {
        plotHolder.SetActive(true);
    }

    public void HidePlot()
    {
        plotHolder.SetActive(false);
    }

    public void ClearPlot()
    {
        plotHolder = null;
        Transform plotHolderToClear = transform.FindChild("Plot Holder");
        if (plotHolderToClear)
            DestroyImmediate(plotHolderToClear.gameObject);
    }

    public void ClearPlotPoints()
    {
        plotPointsHolder = null;
        Transform plotPointsHolderToClear = transform.FindChild("Plot Points Holder");
        if (plotPointsHolderToClear)
            DestroyImmediate(plotPointsHolderToClear.gameObject);
    }

    private List<Vector3> GeneratePolygon()
    {
        plotPoints = new GameObject[numPoints];

        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < numPoints; i++)
        {
            points.Add(new Vector3(0, 0, 0));
        }

        float angle = 0f;
        float distance = 0f;

        plotPointsHolder = new GameObject("Plot Points Holder");
        plotPointsHolder.transform.parent = this.transform;

        for (int i = 0; i < points.Count; i++)
        {
            distance = Random.Range(range - range / 3, range + range / 3);
            angle = angle + Random.Range((360f / numPoints) - 5f, (360f / numPoints) + 5f) * Mathf.PI / 180f;

            float newX = Mathf.Cos(angle);
            float newZ = Mathf.Sin(angle);

            points[i] = this.transform.position + new Vector3(newX, 0, newZ) * distance;

            plotPoints[i] = Instantiate(pointPrefab, points[i], Quaternion.identity);
            plotPoints[i].name = "Plot Point " + i.ToString();
            plotPoints[i].transform.parent = plotPointsHolder.transform;
        }

        return points;
    }

}
