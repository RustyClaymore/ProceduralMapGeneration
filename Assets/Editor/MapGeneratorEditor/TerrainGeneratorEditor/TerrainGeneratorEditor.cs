/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description:
*/

using UnityEngine;
using UnityEditor;

public class TerrainGeneratorEditor : EditorWindow {

    private TerrainGenerator terrainGen;
    
    private int terrainWidth = 513;
    private int terrainMaxHeight = 600;

    private int heightMapWidth = 513;

    private float scale;

    private int octaves;
    private float persistance;
    private float lacunarity;

    private int seed;
    private Vector2 offset;

    // Smoothing
    private bool smoothTerrainHeight;
    private AnimationCurve terrainHeightCurve;
    
    // FaloffProperties
    private bool useFalloffMap;
    private float falloffStrength;
    private float falloffSpeed;

    // Texture Blending Properties
    private float sandLimit;
    private float grassLimit;
    private float stoneLimit;
    private float snowLimit;

    // Trees Generation
    private float treesDensity;

    // Image for the editor window
    private Texture2D terrainGenerationLogo = null;

    // Custom window enabled check
    private bool isEnabled = false;

    [MenuItem("City Generator/Terrain Generator")]
    static void Init()
    {
        TerrainGeneratorEditor window = (TerrainGeneratorEditor)EditorWindow.GetWindow(typeof(TerrainGeneratorEditor));
    }

    private void OnEnable()
    {
        terrainGenerationLogo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/EditorImages/TerrainGenerationLogo.png", typeof(Texture2D));
        terrainHeightCurve = new AnimationCurve();

        LoadTerrainFromJson();

        isEnabled = true;
    }
    
    private void OnDisable()
    {
        isEnabled = false;
        SaveTerrainToJson();
    }

    void OnGUI()
    {
        // Show the tool logo first
        ShowLogoLabel();
        // Show the terrain properties
        ShowTerrainProperties();
        
        if (GUILayout.Button("Generate terrain"))
        {
            GenerateTerrain();
        }

        EditorGUILayout.LabelField("Terrain Details", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Terrain Details"))
        {
            terrainGen.GenerateTerrainDetails();
        }

        if (GUILayout.Button("Clear Terrain Details"))
        {
            terrainGen = FindObjectOfType<TerrainGenerator>();
            terrainGen.ClearTerrainDetails();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Terrain Trees", EditorStyles.boldLabel);

        ShowTerrainTreesProperties();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Trees"))
        {
            SaveTerrainTreesProperties();
            terrainGen.GenerateTerrainTrees();
        }

        if (GUILayout.Button("Clear Trees"))
        {
            terrainGen = FindObjectOfType<TerrainGenerator>();
            terrainGen.ClearTerrainTrees();
        }
        GUILayout.EndHorizontal();
        
        EditorGUILayout.LabelField("Save and Load Terrain Data", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        // Save Terrain Properties to Json File
        if(GUILayout.Button("Save Terrain to JSON"))
        {
            SaveTerrainToJson();
        }

        // Load Terrain Properties from Json File
        if (GUILayout.Button("Load Terrain from JSON"))
        {
            LoadTerrainFromJson();   
        }
        GUILayout.EndHorizontal();
    }

    void GenerateTerrain()
    {
        SaveTerrainProperties();

        terrainGen = FindObjectOfType<TerrainGenerator>();
        terrainGen.GenerateUnityTerrain();
    }

    void SaveTerrainToJson()
    {
        string jsonPath = Application.streamingAssetsPath + "/terrainJson.json";
        SaveTerrainProperties();
        TerrainProperties.Instance.SaveToString(jsonPath);
    }

    void LoadTerrainFromJson()
    {
        string jsonPath = Application.streamingAssetsPath + "/terrainJson.json";
        TerrainProperties.Instance.CreateFromJSON(jsonPath);
        LoadTerrainProperties();

        if (isEnabled)
        {
            GenerateTerrain();
        }
    }

    void SaveTerrainProperties()
    {
        // Terrain size data
        TerrainProperties.Instance.Width = terrainWidth;
        TerrainProperties.Instance.MaxHeight = terrainMaxHeight;

        // Height Map size data
        TerrainProperties.Instance.HeightMapWidth = heightMapWidth;

        // Terrain noise map data
        TerrainProperties.Instance.Scale = scale;
        TerrainProperties.Instance.Octaves = octaves;
        TerrainProperties.Instance.Seed = seed;
        TerrainProperties.Instance.Persistance = persistance;
        TerrainProperties.Instance.Lacunarity = lacunarity;
        TerrainProperties.Instance.Offset = offset;

        TerrainProperties.Instance.SmoothTerrainHeight = smoothTerrainHeight;
        TerrainProperties.Instance.TerrainHeightCurve = terrainHeightCurve;

        TerrainProperties.Instance.UseFalloffMap = useFalloffMap;
        TerrainProperties.Instance.FalloffStrength = falloffStrength;
        TerrainProperties.Instance.FalloffSpeed = falloffSpeed;

        // Texture Blending Properties
        TerrainProperties.Instance.SandLimit = sandLimit;
        TerrainProperties.Instance.GrassLimit = grassLimit;
        TerrainProperties.Instance.StoneLimit = stoneLimit;
        TerrainProperties.Instance.SnowLimit = snowLimit;

        TerrainProperties.Instance.TreesDensity = treesDensity;
    }

    void LoadTerrainProperties()
    {
        // Terrain size data
        terrainWidth = TerrainProperties.Instance.Width;
        terrainMaxHeight = TerrainProperties.Instance.MaxHeight;

        // Heightmap size data
        heightMapWidth = TerrainProperties.Instance.HeightMapWidth;

        // Terrain noise map data
        scale = TerrainProperties.Instance.Scale;
        octaves = TerrainProperties.Instance.Octaves;
        seed = TerrainProperties.Instance.Seed;
        persistance = TerrainProperties.Instance.Persistance;
        lacunarity = TerrainProperties.Instance.Lacunarity;
        offset = TerrainProperties.Instance.Offset;

        smoothTerrainHeight = TerrainProperties.Instance.SmoothTerrainHeight;
        terrainHeightCurve = TerrainProperties.Instance.TerrainHeightCurve;

        useFalloffMap = TerrainProperties.Instance.UseFalloffMap;
        falloffStrength = TerrainProperties.Instance.FalloffStrength;
        falloffSpeed = TerrainProperties.Instance.FalloffSpeed;

        sandLimit = TerrainProperties.Instance.SandLimit;
        grassLimit = TerrainProperties.Instance.GrassLimit;
        stoneLimit = TerrainProperties.Instance.StoneLimit;
        snowLimit = TerrainProperties.Instance.SnowLimit;

        treesDensity = TerrainProperties.Instance.TreesDensity;
    }
    
    void ShowLogoLabel()
    {
        var centeredStyle = GUI.skin.label;
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUILayout.Label(terrainGenerationLogo, centeredStyle);
    }

    void ShowTerrainProperties()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Terrain Size Properties", EditorStyles.boldLabel);

        // Show a list of a possible terrain width values
        // possible width values are a (power of 2) + 1
        int[] possibleWidths = new int[3];
        possibleWidths[0] = 513; possibleWidths[1] = 1025; possibleWidths[2] = 2049;
        string[] possibleWidthsString = new string[3];
        for (int i = 0; i < possibleWidths.Length; i++)
        {
            possibleWidthsString[i] = (possibleWidths[i]-1).ToString();
        }
        terrainWidth = EditorGUILayout.IntPopup("Terrain Width:", terrainWidth, possibleWidthsString, possibleWidths);

        // Show a list of a possible heightmap width values
        // possible width values are a (power of 2) + 1
        int[] possibleHeightMapWidths = new int[3];
        possibleHeightMapWidths[0] = 513; possibleHeightMapWidths[1] = 1025; possibleHeightMapWidths[2] = 2049;
        string[] possibleHeightMapWidthsString = new string[3];
        for (int i = 0; i < possibleHeightMapWidths.Length; i++)
        {
            possibleHeightMapWidthsString[i] = (possibleHeightMapWidths[i] - 1).ToString();
        }
        heightMapWidth = EditorGUILayout.IntPopup("Heightmap Width:", heightMapWidth, possibleHeightMapWidthsString, possibleHeightMapWidths);

        terrainMaxHeight = EditorGUILayout.IntSlider("Terrain MaxHeight:", terrainMaxHeight, 100, 600);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Terrain Height Map Properties", EditorStyles.boldLabel);
        GUIContent seedContent = new GUIContent("Generation Seed:", "Terrain generation seed. \nMakes sure that the random functions will generate the same result for the same seed. \nUseful to generate the same terrain again.");
        seed = EditorGUILayout.IntField(seedContent, seed);

        EditorGUILayout.Separator();
        scale = EditorGUILayout.Slider("Map Scale:", scale, 50, 1000);
        octaves = EditorGUILayout.IntSlider("Noise Octaves:", octaves, 1, 10);
        persistance = EditorGUILayout.Slider("Noise Persistance:", persistance, 0, 1);
        lacunarity = EditorGUILayout.Slider("Noise Lacunarity:", lacunarity, 0, 3);
        offset = EditorGUILayout.Vector2Field("Noise Offset:", offset);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Smoothen Terrain", EditorStyles.boldLabel);
        smoothTerrainHeight = EditorGUILayout.Toggle("Smooth Terrain Height", smoothTerrainHeight);
        terrainHeightCurve = EditorGUILayout.CurveField("Terrain Height Curve:", terrainHeightCurve);

        EditorGUILayout.LabelField("Falloff Terrain", EditorStyles.boldLabel);
        useFalloffMap = EditorGUILayout.Toggle("Use Falloff Map", useFalloffMap);
        falloffStrength = EditorGUILayout.Slider("Falloff Strength", falloffStrength, 0, 10);
        falloffSpeed = EditorGUILayout.Slider("Falloff Speed", falloffSpeed, 0, 10);


        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Texture Blending - Upper Limits", EditorStyles.boldLabel);
        sandLimit = EditorGUILayout.Slider("Sand Limit:", sandLimit, 0, 1);
        grassLimit = EditorGUILayout.Slider("Grass Limit:", grassLimit, 0, 1);
        stoneLimit = EditorGUILayout.Slider("Stone Limit:", stoneLimit, 0, 1);
        //snowLimit = EditorGUILayout.Slider("Snow Limit:", snowLimit, 0, 1);
        
        EditorGUILayout.Separator();
    }

    void ShowTerrainTreesProperties()
    {
        treesDensity = EditorGUILayout.Slider("Trees Density", treesDensity, 0, 100);
    }

    void SaveTerrainTreesProperties()
    {
        TerrainProperties.Instance.TreesDensity = Mathf.Clamp(treesDensity, 0f, 95f);
    }
}
