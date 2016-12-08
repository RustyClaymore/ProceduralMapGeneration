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

        AddVoronoiPointsToScene();
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
        }

        foreach (Edge edge in edges)
        {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;

            Vector3 leftPoint = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0, edge.ClippedEnds[LR.LEFT].y);
            Vector3 rightPoint = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0, edge.ClippedEnds[LR.RIGHT].y);

            Instantiate(voronoiPointPrefab, leftPoint, Quaternion.identity);
        }
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

        foreach (Edge edge in edges)
        {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;

            Vector3 leftPoint = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0, edge.ClippedEnds[LR.LEFT].y);
            Vector3 rightPoint = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0, edge.ClippedEnds[LR.RIGHT].y);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(leftPoint, rightPoint);
        }
    }
}
