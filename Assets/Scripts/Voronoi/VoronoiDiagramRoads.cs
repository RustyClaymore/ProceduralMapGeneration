using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using csDelaunay;

public class VoronoiDiagramRoads : MonoBehaviour
{

    // The number of polygons/sites we want
    public int polygonNumber = 200;

    public float minBoundPos;
    public float maxBoundPos;

    public int lloydRelaxationCount;

    public GameObject pointPrefab;
    public GameObject siteCenterPrefab;
    public GameObject voronoiPointPrefab;
    public GameObject voronoiRoad;

    public float yieldDuration;

    // This is where we will store the resulting data
    private Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;

    public EdgeData[] edgesData;

    IEnumerator Start()
    {
        // Create your sites (lets call that the center of your polygons)
        List<Vector2f> points = CreateRandomPoint();
        yield return StartCoroutine(AddPointsToScene(points));

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
        CopyEdgeToEdgeData();

        yield return StartCoroutine(AddVoronoiPointsToScene());
        yield return StartCoroutine(AddRoadsFromEdgeData());
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

    private IEnumerator AddPointsToScene(List<Vector2f> points)
    {
        foreach (Vector2f p in points)
        {
            Instantiate(pointPrefab, new Vector3(p.x, 0, p.y), Quaternion.identity);

            yield return new WaitForSeconds(yieldDuration);
        }
    }

    private IEnumerator AddVoronoiPointsToScene()
    {
        foreach (KeyValuePair<Vector2f, Site> kv in sites)
        {
            Instantiate(siteCenterPrefab, new Vector3(kv.Key.x, 0, kv.Key.y), Quaternion.identity);

            yield return new WaitForSeconds(yieldDuration);
        }

        foreach (Edge edge in edges)
        {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;

            Vector3 leftPoint = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0, edge.ClippedEnds[LR.LEFT].y);
            Vector3 rightPoint = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0, edge.ClippedEnds[LR.RIGHT].y);

            Instantiate(voronoiPointPrefab, leftPoint, Quaternion.identity);


            yield return new WaitForSeconds(yieldDuration);
        }
    }

    IEnumerator AddRoads()
    {
        foreach (Edge edge in edges)
        {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;

            Vector3 leftPoint = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0, edge.ClippedEnds[LR.LEFT].y);
            Vector3 rightPoint = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0, edge.ClippedEnds[LR.RIGHT].y);

            GameObject road = (GameObject)Instantiate(voronoiRoad, leftPoint, Quaternion.identity);
            road.GetComponent<LineRenderer>().SetPosition(0, leftPoint);
            road.GetComponent<LineRenderer>().SetPosition(1, rightPoint);

            yield return new WaitForSeconds(yieldDuration);
        }
    }

    IEnumerator AddRoadsFromEdgeData()
    {
        Debug.Log(edgesData.Length);
        for (int i = 0; i < edgesData.Length; i++)
        {
            GameObject road = (GameObject)Instantiate(voronoiRoad, edgesData[i].m_initialPoint, Quaternion.identity);
            road.GetComponent<LineRenderer>().SetPosition(0, edgesData[i].m_initialPoint);
            road.GetComponent<LineRenderer>().SetPosition(1, edgesData[i].m_finalPoint);

            yield return new WaitForSeconds(yieldDuration);
        }
    }

    void CopyEdgeToEdgeData()
    {
        int edgesDataSize = 0;

        foreach (Edge edge in edges)
        {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;

            edgesDataSize++;
        }

        edgesData = new EdgeData[edgesDataSize];
        int counter = 0;

        foreach (Edge edge in edges)
        {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;

            Vector3 leftPoint = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0, edge.ClippedEnds[LR.LEFT].y);
            Vector3 rightPoint = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0, edge.ClippedEnds[LR.RIGHT].y);

            edgesData[counter] = new EdgeData(leftPoint, rightPoint, 1);
            counter++;
        }
    }
}
