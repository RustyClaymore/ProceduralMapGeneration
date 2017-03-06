using UnityEngine;

public class LotBuildingGenerator : MonoBehaviour {

    public GameObject buildingPrefab;

    private int currentWidth;
    private int currentLength;
    private int currentFloorCounter;
    private bool currentHasAttic;

    private int buildingsCount = 0;
    private GameObject buildingsHolder;

	void Start()
	{
		buildingsHolder = GameObject.FindGameObjectWithTag("BuildingsHolder");
	}

	public void GenerateBuildings()
	{
		buildingsCount = 0;

		PlotsGeneration plotsGenerator = this.GetComponent<PlotsGeneration>();

		if (!plotsGenerator.centroidsHolder)
		{
			Debug.LogError("Couldn't find subplot centers! Generate roads and subplots first!");
			return;
		}

        Transform[] transforms = new Transform[plotsGenerator.centroidsHolder.transform.childCount];
        transforms = plotsGenerator.centroidsHolder.GetComponentsInChildren<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            CreateBuilding(transforms[i].position);
        }
    }

    public void CreateBuilding(Vector3 position)
    {
        if (buildingsCount == 0)
        {
            buildingsHolder = new GameObject("Buildings Holder");
			buildingsHolder.tag = "BuildingsHolder";
        }
        buildingsCount++;
        GenerateRandomBuildingParams();
        GameObject building = (GameObject)Instantiate(buildingPrefab, position, Quaternion.identity, buildingsHolder.transform);
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
        buildingsCount = 0;
    }
}
