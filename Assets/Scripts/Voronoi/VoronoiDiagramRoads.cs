using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using csDelaunay;

public class VoronoiDiagramRoads : MonoBehaviour
{
    // ********************************** PUBLICS *****************************

    // The number of polygons/sites we want
    public int polygonNumber = 200;

    public float minBoundPos;
    public float maxBoundPos;

    public int lloydRelaxationCount;

    public GameObject pointPrefab;
    public GameObject siteCenterPrefab;
    public GameObject voronoiPointPrefab;

    // LineRenderer Without Script
    public GameObject voronoiRoad;
    // LineRenderer with Script
    public GameObject voronoiRoadWithScript;

    public GameObject roadPoint;

    [Range(1, 16)]
    public int voronoiEdgeSubdivions;

    public float yieldDuration;
    public bool projectOnTerrain;

    // ********************************** PRIVATES *****************************

    // This is where we will store the resulting data
    private Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;

    private EdgeData[] edgesData;

    private GameObject[] roadPoints;

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
        //yield return StartCoroutine(AddRoadsFromEdgeData());
        yield return StartCoroutine(AddRoadsLineRenderers());


        if(projectOnTerrain)
            yield return StartCoroutine(ProjectRoadPointsOnTerrain());
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

    // Add random points to scene (red points)
    private IEnumerator AddPointsToScene(List<Vector2f> points)
    {
        foreach (Vector2f p in points)
        {
            Instantiate(pointPrefab, new Vector3(p.x, 0, p.y), Quaternion.identity);

            yield return new WaitForSeconds(yieldDuration);
        }
    }

    // Add Voronoi Points To Scene (blue)
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

    // Add Roads using Edge ( old version )
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

    // Return the number of road points (with duplicates)
    int FindNumberOfRoadPoints()
    {
        int numberOfPoints = 0;

        for (int i = 0; i < edgesData.Length; i++)
        {
            if (edgesData[i].GetSegmentsCount() == 1)
            {
                numberOfPoints++;
            }
            else
            {
                for (int j = 0; j < edgesData[i].GetSegmentsCount(); j++)
                {
                    numberOfPoints++;
                }
            }
        }

        return numberOfPoints;
    }

    // Add Road points (purple and green)
    IEnumerator AddRoadPoints()
    {
        roadPoints = new GameObject[FindNumberOfRoadPoints() * 2];
        
        int counter = 0;

        for (int i = 0; i < roadPoints.Length / (voronoiEdgeSubdivions * 2); i++)
        {
            if (edgesData[i].GetSegmentsCount() == 1)
            {
                roadPoints[i * 2] = (GameObject)Instantiate(roadPoint, edgesData[i].GetInitialPoint(), Quaternion.identity);
                roadPoints[(i * 2) + 1] = (GameObject)Instantiate(roadPoint, edgesData[i].GetFinalPoint(), Quaternion.identity);
            }
            else
            {
                for (int j = 0; j < edgesData[i].GetSegmentsCount(); j++)
                {
                    roadPoints[counter] = (GameObject)Instantiate(roadPoint, edgesData[i].m_edgeSegments[j].GetInitialPoint(), Quaternion.identity);
                    roadPoints[counter + 1] = (GameObject)Instantiate(roadPoint, edgesData[i].m_edgeSegments[j].GetFinalPoint(), Quaternion.identity);
                    counter += 2;
                }
            }
        }

        yield return new WaitForSeconds(yieldDuration);
    }

    // Project road points on the terrain
    IEnumerator ProjectRoadPointsOnTerrain()
    {
        //GameObject[] points = GameObject.FindGameObjectsWithTag("VoronoiPoint");

        //foreach (GameObject po in points)
        //{
        //    RaycastHit hit;
        //    if (Physics.Raycast(po.transform.position, Vector3.down, out hit, 300))
        //    {
        //        if (hit.transform.tag == "Terrain")
        //        {
        //            Debug.Log(hit.point);
        //            po.transform.position = hit.point;
        //        }
        //    }
        //}
        for (int i = 0; i < roadPoints.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(roadPoints[i].transform.position, Vector3.down, out hit, 300))
            {
                if (hit.transform.tag == "Terrain")
                {
                    Debug.Log(hit.point);
                    roadPoints[i].transform.position = hit.point;
                }
            }
            yield return new WaitForSeconds(yieldDuration);
        }
        yield return new WaitForSeconds(yieldDuration);
    }

    // Add roads ( line renderers ) connecting road points
    // Enables real time modification
    IEnumerator AddRoadsLineRenderers()
    {
        yield return AddRoadPoints();

        for (int i = 0; i < roadPoints.Length; i+=2)
        {
            GameObject road = (GameObject)Instantiate(voronoiRoadWithScript, roadPoints[i].transform.position, Quaternion.identity);
            road.GetComponent<SetRoadNextPoint>().SetInitialPoint(roadPoints[i]);
            road.GetComponent<SetRoadNextPoint>().SetNextPoint(roadPoints[i + 1]);

            yield return new WaitForSeconds(yieldDuration);
        }
    }

    // Add roads directly from EdgeData
    // No real time modification
    IEnumerator AddRoadsFromEdgeData()
    {
        for (int i = 0; i < edgesData.Length; i++)
        {
            if (edgesData[i].GetSegmentsCount() == 1)
            {
                GameObject road = (GameObject)Instantiate(voronoiRoad, edgesData[i].GetInitialPoint(), Quaternion.identity);
                road.GetComponent<LineRenderer>().SetPosition(0, edgesData[i].GetInitialPoint());
                road.GetComponent<LineRenderer>().SetPosition(1, edgesData[i].GetFinalPoint());
            }
            else
            {
                Debug.Log(edgesData[i].GetSegmentsCount());
                for (int j = 0; j < edgesData[i].GetSegmentsCount(); j++)
                {
                    Debug.Log(j);
                    GameObject road = (GameObject)Instantiate(voronoiRoad, edgesData[i].m_edgeSegments[j].GetInitialPoint(), Quaternion.identity);
                    road.GetComponent<LineRenderer>().SetPosition(0, edgesData[i].m_edgeSegments[j].GetInitialPoint());
                    road.GetComponent<LineRenderer>().SetPosition(1, edgesData[i].m_edgeSegments[j].GetFinalPoint());
                    yield return new WaitForSeconds(yieldDuration);
                }
            }
            yield return new WaitForSeconds(yieldDuration);
        }
    }

    // Copy Edge to EdgeData (enables subdivision) 
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
            edgesData[counter].DivideEdgeIntoSegments(voronoiEdgeSubdivions);

            counter++;
        }
    }
}
