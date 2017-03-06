using UnityEngine;
using System.Collections.Generic;

using csDelaunay;

public class VoronoiDiagram3D : MonoBehaviour
{

    // The number of polygons/sites we want
    public int polygonNumber = 200;

    public float minBoundPos;
    public float maxBoundPos;

    public int lloydRelaxationCount;

    public GameObject pointPrefab;
    public GameObject siteCenterPrefab;
    public GameObject voronoiPointPrefab;

    public bool useGridLayout;
    public float gridPointsSeparationDistance;

    // This is where we will store the resulting data
    private Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;

    // List of parcels (sites)
    private List<Parcel> parcels;
    public GameObject lineRendererSubParcels;

    void Start()
    {
        // Create your sites (lets call that the center of your polygons)
        List<Vector2f> points;
        if (useGridLayout)
        {
            points = CreateGridPoints();
        }
        else
        {
            points = CreateRandomPoint();
        }
        AddPointsToScene(points);

        // Create the bounds of the voronoi diagram
        // Use Rectf instead of Rect; it's a struct just like Rect and does pretty much the same,
        // but like that it allows you to run the delaunay library outside of unity (which mean also in another tread)
        Rectf bounds = new Rectf(minBoundPos, minBoundPos, maxBoundPos * 2, maxBoundPos * 2);

        // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
        // Here I used it with 2 iterations of the lloyd relaxation
        Voronoi voronoi = new Voronoi(points, bounds, lloydRelaxationCount);

        // But you could also create it without lloyd relaxtion and call that function later if you want
        //Voronoi voronoi = new Voronoi(points,bounds);
        //voronoi.LloydRelaxation(lloydRelaxationCount);

        // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;
        
        parcels = new List<Parcel>();
        CreateParcelsFromSites(sites);

        int i = 0;
        foreach(KeyValuePair<Vector2f, Site> site in sites)
        {
            print("site edges : " + site.Value.Edges.Count);
            print("parcel segments : " + parcels[i].parcelSegments.Count);
            i++;
        }

        AddVoronoiPointsToScene();

        foreach (Parcel parcel in parcels)
        {
            RecursiveParceller(parcel, 2);
        }
    }

    private void CreateParcelsFromSites(Dictionary<Vector2f, Site> vSites)
    {
        foreach(KeyValuePair<Vector2f, Site> vs in vSites)
        {
            Parcel parcel = new Parcel("");
            foreach (Edge edge in vs.Value.Edges)
            {
                //// if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
                if (edge.ClippedEnds == null) continue;

                Vector3 leftPoint = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0, edge.ClippedEnds[LR.LEFT].y);
                Vector3 rightPoint = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0, edge.ClippedEnds[LR.RIGHT].y);
                parcel.parcelPoints.Add(leftPoint);

                Segment seg = new Segment(leftPoint, rightPoint);
                parcel.parcelSegments.Add(seg);
            }

            parcel.UpdateParcelPointsFromSegments();
            parcels.Add(parcel);
        }
    }

    private void RecursiveParceller(Parcel parcel, int iter)
    {
        List<Parcel> subParcels = new List<Parcel>();
        subParcels = UseParceller(parcel);

        if (iter > 0)
        {
            for (int i = 0; i < subParcels.Count; i++)
            {
                RecursiveParceller(subParcels[i], iter - 1);
            }
        }
    }

    List<Parcel> UseParceller(Parcel parcel)
    {
        Parceller parceller = new Parceller(parcel);
        parceller.SubdivideParcel();

        ShowConvexHull(parcel);
        //ShowRect(parceller.parcel.obb.obbRect);

        return parcel.subParcels;
    }

    private List<Vector2f> CreateRandomPoint()
    {
        // Use Vector2f, instead of Vector2
        // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < polygonNumber; i++)
        {
            points.Add(new Vector2f(Random.Range(minBoundPos, maxBoundPos), Random.Range(minBoundPos, maxBoundPos)));
        }

        return points;
    }

    private List<Vector2f> CreateGridPoints()
    {
        List<Vector2f> points = new List<Vector2f>();
        for (float i = minBoundPos; i < maxBoundPos; i += ((Mathf.Abs(minBoundPos) + maxBoundPos)/2f)/polygonNumber*10)
        {
            for (float j = minBoundPos; j < maxBoundPos; j += ((Mathf.Abs(minBoundPos) + maxBoundPos) / 2f)/polygonNumber*10)
            {
                points.Add(new Vector2f(i + Random.Range(-gridPointsSeparationDistance, gridPointsSeparationDistance), j + Random.Range(-gridPointsSeparationDistance, gridPointsSeparationDistance)));
            }
        }

        return points;
    }

    private void AddPointsToScene(List<Vector2f> points)
    {
        foreach(Vector2f p in points)
        {
            Instantiate(pointPrefab, new Vector3(p.x, 0, p.y), Quaternion.identity);
        }
    }

    private void AddVoronoiPointsToScene()
    {
        foreach (KeyValuePair<Vector2f, Site> kv in sites)
        {
            Instantiate(siteCenterPrefab, new Vector3(kv.Key.x, 0, kv.Key.y), Quaternion.identity);

            foreach(Edge edge in kv.Value.Edges)
            {
                // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
                if (edge.ClippedEnds == null) continue;

                Vector3 leftPoint = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0, edge.ClippedEnds[LR.LEFT].y);
                Vector3 rightPoint = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0, edge.ClippedEnds[LR.RIGHT].y);

                Instantiate(voronoiPointPrefab, leftPoint, Quaternion.identity);
            }
        }

        //foreach (Edge edge in edges)
        //{
        //    // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
        //    if (edge.ClippedEnds == null) continue;

        //    Vector3 leftPoint = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0, edge.ClippedEnds[LR.LEFT].y);
        //    Vector3 rightPoint = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0, edge.ClippedEnds[LR.RIGHT].y);

        //    Instantiate(voronoiPointPrefab, leftPoint, Quaternion.identity);
        //}
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        //Vector3 previousPos = Vector3.zero;

        //foreach (KeyValuePair<Vector2f, Site> site in sites)
        //{
        //    Vector3 sitePos = new Vector3(site.Key.x, 0, site.Key.y);

        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawLine(previousPos, sitePos);

        //    previousPos = sitePos;
        //}

        //foreach (Edge edge in edges)
        //{
        //    // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
        //    if (edge.ClippedEnds == null) continue;

        //    Vector3 leftPoint = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0, edge.ClippedEnds[LR.LEFT].y);
        //    Vector3 rightPoint = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0, edge.ClippedEnds[LR.RIGHT].y);

        //    Gizmos.color = Color.green;
        //    Gizmos.DrawLine(leftPoint, rightPoint);
        //}

        //for (int i = 0; i < parcels.Count/2; i++)
        //{
        //    foreach(Segment seg in parcels[i].parcelSegments)
        foreach(Parcel parcel in parcels)
        {
            foreach(Segment seg in parcel.parcelSegments)
            {
                Vector3 leftPoint = seg.startPos;
                Vector3 rightPoint = seg.finalPos;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(leftPoint, rightPoint);
            }
        }
    }

    void ShowConvexHull(Parcel parcel)
    {
        GameObject go = new GameObject("ConvexHullHolder");
        for (int i = 0; i < parcel.obb.convexHullPoints.Count; i++)
        {
            GameObject line = Instantiate(lineRendererSubParcels);
            line.transform.parent = go.transform;
            if (i != parcel.obb.convexHullPoints.Count - 1)
            {
                line.GetComponent<LineRenderer>().SetPosition(0, parcel.obb.convexHullPoints[i]);
                line.GetComponent<LineRenderer>().SetPosition(1, parcel.obb.convexHullPoints[i + 1]);
            }
            else
            {
                line.GetComponent<LineRenderer>().SetPosition(0, parcel.obb.convexHullPoints[i]);
                line.GetComponent<LineRenderer>().SetPosition(1, parcel.obb.convexHullPoints[0]);
            }
        }
    }
}
