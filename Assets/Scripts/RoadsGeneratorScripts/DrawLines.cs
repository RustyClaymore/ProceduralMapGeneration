using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class DrawLines : MonoBehaviour
{
    // ---------PUBLICS------------ 

    public TerrainSurfaceSmoothing surfaceSmoothingScript;

    public int seed;

    [Header("Road Points Parameters")]
    [Range(3,20)]
    public int circleRoadPointsCount;
    [Range(1, 4)]
    public int circlePointsCountMultiplier;
    [Range(1f, 40f)]
    public float circleRoadRadius;
    
    public Vector3 startingPointPos;
    public int roadPointsCount;

    public float angleVariance;
    public float roadScale;

    public bool concurrentRoadsGeneration;

    [Header("Sub Road Parameters")]
    public int subRoadsIterations = 3;
    public float subRoadsPercent = 20f;
    [Range(1,8)]
    public int subRoadsCountDivisionFactor;
    [Range(1, 8)]
    public int subRoadsScaleDivisionFactor;
    public bool generateSubRoads = false;
    public bool slowSubroadGeneration = false;

    [Header("Roads Prefabs")]
    public GameObject primaryStreetLinePrefab;
    public GameObject secondaryStreetLinePrefab;

    [Header("Others")]
    public float yieldDuration = 0.2f;

    [Header("Tests")]
    public bool multipleCirclesTest = false;
    public bool flattenTerrainTest = false;

    // ---------PRIVATES------------ 
    private Vector3[] roadPoints;
    private Vector3[] circleRoadPoints;

    private GameObject[] roadPointsSpheres;
    private GameObject[] circleRoadPointsSpheres;

    private float angleOffset;

    // Use this for initialization
    IEnumerator Start()
    {
        startingPointPos = startingPointPos + new Vector3(500, 0, 500);
        yield return StartCoroutine(GenerateRoads(startingPointPos));

        //startingPointPos = startingPointPos + new Vector3(-500, 0, -500);
        //yield return (GenerateRoads(startingPointPos));
    }

    public IEnumerator GenerateRoads(Vector3 startingPos)
    {
        Random.InitState(seed);
        angleOffset = 360 / (circleRoadPointsCount * circlePointsCountMultiplier);

        //StartCoroutine(GenerateRoadMapCircular(circleRoadRadius));
        yield return StartCoroutine(GenerateRoadMapCircular(startingPos, circleRoadRadius));

        if (multipleCirclesTest)
        {
            for (int i = 1; i < 4; i++)
            {
                yield return StartCoroutine(GenerateRoadMapCircular(startingPos, circleRoadRadius * (i + 1)));
            }
        }

        if (concurrentRoadsGeneration)
        {
            for (int i = 0; i < circleRoadPointsCount * circlePointsCountMultiplier; i += circlePointsCountMultiplier)
            {
                StartCoroutine(GenerateRoadMap(circleRoadPoints[i], roadPointsCount, i * angleOffset, roadScale, subRoadsIterations));
            }
        }
        else
        {
            for (int i = 0; i < circleRoadPointsCount * circlePointsCountMultiplier; i += circlePointsCountMultiplier)
            {
                yield return StartCoroutine(GenerateRoadMap(circleRoadPoints[i], roadPointsCount, i * angleOffset, roadScale, subRoadsIterations));
            }
        }
    }

    IEnumerator GenerateRoadMapCircular(Vector3 centerPoint, float radius)
    {
        circleRoadPoints = new Vector3[circleRoadPointsCount * circlePointsCountMultiplier];
        circleRoadPointsSpheres = new GameObject[circleRoadPointsCount * circlePointsCountMultiplier];
        
        float slice = 2 * Mathf.PI / (circleRoadPointsCount * circlePointsCountMultiplier);

        for (int i = 0; i < circleRoadPointsCount * circlePointsCountMultiplier; i++)
        {
            float angle = slice * i;

            float newX = centerPoint.x + radius * Mathf.Cos(angle);
            float newZ = centerPoint.z + radius * Mathf.Sin(angle);

            circleRoadPoints[i] = new Vector3(newX, centerPoint.y, newZ);
            circleRoadPointsSpheres[i] = (GameObject)Instantiate(primaryStreetLinePrefab, circleRoadPoints[i], Quaternion.identity);

            RaycastHit hit;
            if (Physics.Raycast(circleRoadPoints[i], Vector3.down, out hit, startingPointPos.y * 1.3f))
            {
                if (hit.transform.tag == "Terrain")
                {
                    Debug.Log(hit.point);
                    circleRoadPoints[i] = hit.point;
                    circleRoadPointsSpheres[i].transform.position = hit.point;
                }
            }

            if(i > 0)
            {
                circleRoadPointsSpheres[i].GetComponent<LineRenderer>().SetPosition(0, circleRoadPoints[i]);
                circleRoadPointsSpheres[i].GetComponent<LineRenderer>().SetPosition(1, circleRoadPoints[i - 1]);
            }
            else
            {
                circleRoadPointsSpheres[0].GetComponent<LineRenderer>().enabled = false;
            }

            yield return new WaitForSeconds(yieldDuration);
        }
        circleRoadPointsSpheres[0].GetComponent<LineRenderer>().enabled = true;
        circleRoadPointsSpheres[0].GetComponent<LineRenderer>().SetPosition(0, circleRoadPoints[0]);
        circleRoadPointsSpheres[0].GetComponent<LineRenderer>().SetPosition(1, circleRoadPoints[circleRoadPointsSpheres.Length - 1]);
    }

    IEnumerator GenerateRoadMap(Vector3 startingPoint, int localRoadPointsCount, float angle, float scale, int iter)
    {
        if(iter > 1)
        {
            Vector3[] roadPoints = new Vector3[localRoadPointsCount];
            GameObject[] roadPointsSpheres = new GameObject[localRoadPointsCount];
            
            roadPoints[0] = startingPoint;

            for (int i = 1; i < localRoadPointsCount - 1; i++)
            {
                float randomAngle = Random.Range(angle - angleVariance, angle + angleVariance);
            
                float randX = Mathf.Cos(randomAngle * Mathf.Deg2Rad);
                float randZ = Mathf.Sin(randomAngle * Mathf.Deg2Rad);
                Vector3 nextPoint = roadPoints[i - 1] + new Vector3(randX, 0, randZ) * scale ;
                roadPoints[i] = nextPoint;
                roadPointsSpheres[i - 1] = (GameObject)Instantiate(secondaryStreetLinePrefab, roadPoints[i], Quaternion.identity);

                RaycastHit hit;
                if (Physics.Raycast(roadPoints[i] + Vector3.up * startingPointPos.y, Vector3.down, out hit, startingPointPos.y * 1.3f))
                {
                    if (hit.transform.tag == "Terrain")
                    {
                        Debug.Log(hit.point);
                        roadPoints[i] = hit.point;
                        roadPointsSpheres[i - 1].transform.position = hit.point;

                        if(flattenTerrainTest)
                            surfaceSmoothingScript.SmoothTerrainSurfaceAtPoint(hit.point, 10, 10);
                    }
                }
                
                roadPointsSpheres[i - 1].GetComponent<LineRenderer>().SetPosition(1, roadPoints[i]);
                roadPointsSpheres[i - 1].GetComponent<LineRenderer>().SetPosition(0, roadPoints[i - 1]);

                yield return new WaitForSeconds(yieldDuration);

                float randGenerateSubRoads = Random.Range(0f, 100f);
                if (generateSubRoads && (randGenerateSubRoads > (100f - subRoadsPercent)))
                {
                    float randAngle = Random.Range(0, 2) * 180 - 90;
                    if (slowSubroadGeneration)
                    {
                        yield return StartCoroutine(GenerateRoadMap(roadPoints[i], localRoadPointsCount / 2, angle + randAngle, scale / 3f, iter - 1));
                    }
                    else
                    {
                        StartCoroutine(GenerateRoadMap(roadPoints[i], localRoadPointsCount / 2, angle + randAngle, scale / 3f, iter - 1));
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    void DestroyAllSpheres()
    {
        for (int i = 0; i < roadPointsCount - 1; i++)
        {
            Destroy(roadPointsSpheres[i].gameObject);
        }
    }
}
