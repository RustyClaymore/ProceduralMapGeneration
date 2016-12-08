using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BuildingInEditorGeneration : MonoBehaviour {

    public float distanceBetweenBuildingsLine = 3f;
    public float distanceBetweenBuildingsRow = 60f;

    public GameObject buildingPrefab;
    
    private int currentWidth;
    private int currentLength;
    private int currentFloorCounter;
    private bool currentHasAttic;

    private Vector3 currentOffset;
    private int rowCounter = 0;

    private Vector3 currentBuildingPosition = Vector3.zero;
    private Vector3 nextBuildingPosition = Vector3.zero;

    private int buildingsCount = 0;
    private GameObject buildingsHolder;

    // Use this for initialization
    void Start()
    {

    }
    
    public void CreateNewBuilding()
    {
        if(buildingsCount == 0)
        {
            buildingsHolder = new GameObject("Buildings Holder");
        }
        buildingsCount++;
        GenerateRandomBuildingParams();
        GameObject building = (GameObject)Instantiate(buildingPrefab, nextBuildingPosition, Quaternion.identity, buildingsHolder.transform);
        nextBuildingPosition += Vector3.right * currentLength + Vector3.right * distanceBetweenBuildingsLine;
    }

    public void GoToNextRow()
    {
        rowCounter++;
        nextBuildingPosition = Vector3.zero + rowCounter * Vector3.forward * distanceBetweenBuildingsRow;
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

    public void DeleteBuildings()
    {
        DestroyImmediate(buildingsHolder);
        nextBuildingPosition = Vector3.zero;
        buildingsCount = 0;
        rowCounter = 0;
    }
}
