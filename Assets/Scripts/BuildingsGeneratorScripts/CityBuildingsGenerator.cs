using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityBuildingsGenerator : MonoBehaviour {

    public float neighborhoodRows;
    public float buildingsNumberByRow;
    public float distanceBetweenBuildingsLine = 3f;
    public float distanceBetweenBuildingsRow = 60f;

    public GameObject buildingPrefab;

    [Space]
    public float waitingTime;

    private int currentWidth;
    private int currentLength;
    private int currentFloorCounter;
    private bool currentHasAttic;

    private Vector3 currentOffset;

    private Vector3 currentBuildingPosition = Vector3.zero;
    private Vector3 nextBuildingPosition = Vector3.zero;

	// Use this for initialization
	IEnumerator Start () {
        for (int i = 0; i < neighborhoodRows; i++)
        {
            nextBuildingPosition = Vector3.zero + i * Vector3.forward * distanceBetweenBuildingsRow;
            for (int j = 0; j < buildingsNumberByRow; j++)
            {
                yield return CreateNewBuilding();
                nextBuildingPosition += Vector3.right * currentLength + Vector3.right * distanceBetweenBuildingsLine;
            }
        }	
	}
	
    IEnumerator CreateNewBuilding()
    {
        GenerateRandomBuildingParams();
        GameObject building = (GameObject)Instantiate(buildingPrefab, nextBuildingPosition, Quaternion.identity);
        yield return new WaitForSeconds(waitingTime);
    }

    void GenerateRandomBuildingParams()
    {
        currentWidth = Random.Range(10, 31);
        currentLength = Random.Range(10, 61);

        currentFloorCounter = Random.Range(1, 11);
        currentHasAttic = RandomBool();

        buildingPrefab.GetComponent<BuildingMeshGenerator>().width = currentWidth;
        buildingPrefab.GetComponent<BuildingMeshGenerator>().length = currentLength;
        buildingPrefab.GetComponent<BuildingMeshGenerator>().floorCount = currentFloorCounter;
        buildingPrefab.GetComponent<BuildingMeshGenerator>().hasAttic = currentHasAttic;
    }

    bool RandomBool()
    {
        return (Random.value > 0.5f);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
