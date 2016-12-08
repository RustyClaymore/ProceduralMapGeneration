using UnityEngine;
using System.Collections;

public class GenerateRoadMap : MonoBehaviour {

    public int seed;

    public GameObject[] cityTiles;
    public GameObject[] buildings;
    public GameObject[] cornerBuildings;
    public GameObject[] trees;

    public int mapWidth;
    public int mapLength;

    public int maxStreetX;
    public int maxStreetY;

    public int xRoadsRandomMinRange;
    public int xRoadsRandomMaxRange;
    public int yRoadsRandomMinRange;
    public int yRoadsRandomMaxRange;

    public float coroutineDelay = 0.02f;

    private int[,] mapGridInfo;

	// Use this for initialization
	void Start () {
        Random.InitState(seed);
        StartCoroutine(GenerateMap());
    }
	
	// Update is called once per frame
	void Update () {
    }

    IEnumerator GenerateMap()
    {
        GenerateMapData();

        #region without coroutine
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapLength; j++)
            {
                if (mapGridInfo[i, j] == -1)
                {
                    InstantiateRoadX(i, j);
                }
                else if (mapGridInfo[i, j] == -3)
                {
                    InstantiateCrossroad(i, j);
                }
            }
        }

        for (int j = 0; j < mapWidth; j++)
        {
            for (int i = 0; i < mapLength; i++)
            {
                if (mapGridInfo[i, j] == -2)
                {
                    InstantiateRoadY(i, j);
                }
            }
        }

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapLength; j++)
            {
                if (mapGridInfo[i, j] == 0)
                {
                    //Instantiate(cityTiles[0], new Vector3(i, 0, j) * 3, cityTiles[0].transform.rotation);
                    if ((i < (mapWidth - 1)) && (i > 0) && (j < (mapLength - 1)) && (j > 0))
                    {
                        // Instantiate buildings, grass and trees near roads
                        if ((mapGridInfo[i + 1, j] < 0) || (mapGridInfo[i - 1, j] < 0) ||
                            (mapGridInfo[i, j + 1] < 0) || (mapGridInfo[i, j - 1] < 0))
                        {

                            float tileRand = Random.Range(0.0f, 100.0f);
                            if (tileRand > 70)
                            {
                                InstantiateGrass(i, j);
                                InstantiateTree(i, j);
                            }
                            else if (tileRand > 20 && tileRand <= 70)
                            {
                                InstantiateConcrete(i, j);
                                if (mapGridInfo[i - 1, j - 1] == -3)
                                {
                                    InstantiateCornerBuilding(i, j, 90);
                                }
                                else if(mapGridInfo[i - 1, j + 1] == -3)
                                {
                                    InstantiateCornerBuilding(i, j, 180);
                                }
                                else if (mapGridInfo[i + 1, j - 1] == -3)
                                {
                                    InstantiateCornerBuilding(i, j, 0);
                                }
                                else if (mapGridInfo[i + 1, j + 1] == -3)
                                {
                                    InstantiateCornerBuilding(i, j, -90);
                                }
                                else
                                {
                                    InstantiateBuilding(i, j);
                                }
                                yield return new WaitForSeconds(coroutineDelay);
                            }
                            else
                            {
                                InstantiateGrass(i, j);
                            }
                        }
                        else
                        {
                            InstantiateWater(i, j);
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(coroutineDelay);
        #endregion

        //for (int i = 0; i < mapWidth; i++)
        //{
        //    for (int j = 0; j < mapLength; j++)
        //    {
        //        if (mapGridInfo[i, j] == -1)
        //        {
        //            InstantiateRoadX(i, j);
        //            yield return new WaitForSeconds(coroutineDelay);
        //        }
        //        else if (mapGridInfo[i, j] == -3)
        //        {
        //            InstantiateCrossroad(i, j);
        //            yield return new WaitForSeconds(coroutineDelay);
        //        }
        //    }
        //}

        //for (int j = 0; j < mapWidth; j++)
        //{
        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        if (mapGridInfo[i, j] == -2)
        //        {
        //            InstantiateRoadY(i, j);
        //            yield return new WaitForSeconds(coroutineDelay);
        //        }
        //    }
        //}

        //for (int i = 0; i < mapWidth; i++)
        //{
        //    for (int j = 0; j < mapLength; j++)
        //    {
        //        if (mapGridInfo[i, j] == 0)
        //        {
        //            //Instantiate(cityTiles[0], new Vector3(i, 0, j) * 3, cityTiles[0].transform.rotation);
        //            if ((i < (mapWidth - 1)) && (i > 0) && (j < (mapLength - 1)) && (j > 0))
        //            {
        //                // Instantiate grass and trees near roads
        //                if ((mapGridInfo[i + 1, j] < 0) || (mapGridInfo[i - 1, j] < 0) ||
        //                    (mapGridInfo[i, j + 1] < 0) || (mapGridInfo[i, j - 1] < 0))
        //                {
        //                    InstantiateGrass(i, j);
        //                    yield return new WaitForSeconds(coroutineDelay);

        //                    float treeRand = Random.Range(0.0f, 100.0f);
        //                    if (treeRand > 40)
        //                    {
        //                        InstantiateTree(i, j);
        //                        yield return new WaitForSeconds(coroutineDelay);
        //                    }
        //                }
        //                else
        //                {
        //                    InstantiateWater(i, j);
        //                    yield return new WaitForSeconds(coroutineDelay);
        //                }
        //            }
        //        }
        //    }
        //}
    }

    void GenerateMapData()
    {
        mapGridInfo = new int[mapWidth, mapLength];

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapLength; j++)
            {
                mapGridInfo[i, j] = 0;
            }
        }

        int streetCounterX = 0;
        for (int i = 0; i < maxStreetX; i++)
        {
            for (int j = 0; j < mapLength; j++)
            {
                mapGridInfo[streetCounterX, j] = -1;
            }

            streetCounterX += Random.Range(xRoadsRandomMinRange, xRoadsRandomMaxRange);
            if (streetCounterX >= mapLength)
                break;
        }

        int streetCounterY = 0;
        for (int i = 0; i < maxStreetY; i++)
        {
            for (int j = 0; j < mapLength; j++)
            {
                mapGridInfo[j, streetCounterY] -= 2;
            }

            streetCounterY += Random.Range(yRoadsRandomMinRange, yRoadsRandomMaxRange);
            if (streetCounterY >= mapLength)
                break;
        }
    }

    void InstantiateRoadX(int i, int j)
    {
        // Instantiate road X
        Instantiate(cityTiles[1], new Vector3(i, 0, j) * 3, cityTiles[1].transform.rotation);
    }
    void InstantiateRoadY(int i, int j)
    {
        // Insantiate road y
        Instantiate(cityTiles[2], new Vector3(i, 0, j) * 3, cityTiles[2].transform.rotation);
    }
    void InstantiateCrossroad(int i, int j)
    {
        // Instantiate crossroad
        Instantiate(cityTiles[3], new Vector3(i, 0, j) * 3, cityTiles[3].transform.rotation);
    }

    void InstantiateGrass(int i, int j)
    {
        // Instantiate grass
        Instantiate(cityTiles[0], new Vector3(i, 0, j) * 3, cityTiles[0].transform.rotation);
    }

    void InstantiateConcrete(int i, int j)
    {
        // Instantiate concrete
        Instantiate(cityTiles[5], new Vector3(i, 0, j) * 3, cityTiles[0].transform.rotation);
    }

    void InstantiateTree(int i, int j)
    {
        // Instantiate trees 
        int treeRandomIndex = Random.Range(0, trees.Length);
        GameObject tree = (GameObject)Instantiate(trees[treeRandomIndex], new Vector3(i, 0, j) * 3, trees[treeRandomIndex].transform.rotation);
        float randomTreeScale = Random.Range(0.9f, 1.1f);
        tree.transform.localScale = new Vector3(1, randomTreeScale, 1);
    }

    void InstantiateBuilding(int i, int j)
    {
        // Instantiate buildings
        int buildingRandomIndex = Random.Range(0, buildings.Length);
        GameObject building = (GameObject)Instantiate(buildings[buildingRandomIndex], new Vector3(1.5f + (i - 1) * 3, 0, 1.5f + (j - 1) * 3), Quaternion.identity);

        float scaleRand = Random.Range(0.8f, 1.2f);
        building.transform.GetChild(0).transform.localScale = new Vector3(1, scaleRand, 1);
    }

    void InstantiateCornerBuilding(int i, int j, float angle)
    {
        // Instantiate corner buildings
        int buildingRandomIndex = Random.Range(0, cornerBuildings.Length);
        GameObject building = (GameObject) Instantiate(cornerBuildings[buildingRandomIndex], 
                                                      new Vector3(1.5f + (i - 1) * 3, 0, 1.5f + (j - 1) * 3),
                                                      Quaternion.Euler(0, angle, 0));

        float scaleRand = Random.Range(0.8f, 1.2f);
        building.transform.GetChild(0).transform.localScale = new Vector3(1, scaleRand, 1);
    }

    void InstantiateWater(int i, int j)
    {
        // Instantite water
        Instantiate(cityTiles[4], new Vector3(i, 0, j) * 3, cityTiles[0].transform.rotation);
    }

}
