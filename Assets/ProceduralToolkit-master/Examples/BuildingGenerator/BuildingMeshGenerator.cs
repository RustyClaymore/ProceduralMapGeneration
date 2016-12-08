using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BuildingMeshGenerator : MonoBehaviour {

    public MeshFilter buildingMeshFilter;

    [Space]
    [Range(minWidth, maxWidth)]
    public int width = 15;
    [Range(minLength, maxLength)]
    public int length = 30;
    [Range(minFloorCount, maxFloorCount)]
    public int floorCount = 5;
    public bool hasAttic = true;

    private const int minWidth = 10;
    private const int maxWidth = 30;
    private const int minLength = 10;
    private const int maxLength = 60;
    private const int minFloorCount = 1;
    private const int maxFloorCount = 10;
    
    // Use this for initialization
    void Start () {
        buildingMeshFilter = GetComponent<MeshFilter>();
        Generate();
	}
    
    public void Generate()
    {
        //targetPalette = RandomE.TetradicPalette(0.25f, 0.75f);
        //targetPalette.Add(ColorHSV.Lerp(targetPalette[2], targetPalette[3], 0.5f));

        var buildingDraft = ProceduralToolkit.Examples.BuildingGenerator.BuildingDraft(width, length, floorCount, hasAttic, Random.ColorHSV(), false);

        var buildingMesh = buildingDraft.ToMesh();
        buildingMesh.RecalculateBounds();
        buildingMeshFilter.mesh = buildingMesh;

        float buildingRadius = Mathf.Sqrt(length / 2f * length / 2f + width / 2f * width / 2f);
    }
    
}
