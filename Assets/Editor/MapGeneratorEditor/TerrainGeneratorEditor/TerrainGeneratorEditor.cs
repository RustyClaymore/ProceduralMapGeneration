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
        EditorGUILayout.LabelField("Terrain Size Properties");

        // Show a list of a possible width values
        // possible width values are a (power of 2) + 1
        int[] possibleWidths = new int[3];
        possibleWidths[0] = 513; possibleWidths[1] = 1025; possibleWidths[2] = 2049;
        string[] possibleWidthsString = new string[3];
        for (int i = 0; i < possibleWidths.Length; i++)
        {
            possibleWidthsString[i] = (possibleWidths[i]-1).ToString();
        }
        terrainWidth = EditorGUILayout.IntPopup("Terrain Width:", terrainWidth, possibleWidthsString, possibleWidths);

        terrainMaxHeight = EditorGUILayout.IntSlider("Terrain MaxHeight:", terrainMaxHeight, 100, 600);

        int[] possibleHeightMapWidths = new int[3];
        possibleHeightMapWidths[0] = 513; possibleHeightMapWidths[1] = 1025; possibleHeightMapWidths[2] = 2049;
        string[] possibleHeightMapWidthsString = new string[3];
        for (int i = 0; i < possibleHeightMapWidths.Length; i++)
        {
            possibleHeightMapWidthsString[i] = (possibleHeightMapWidths[i] - 1).ToString();
        }
        heightMapWidth = EditorGUILayout.IntPopup("Heightmap Width:", heightMapWidth, possibleHeightMapWidthsString, possibleHeightMapWidths);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Terrain Height Map Properties");
        seed = EditorGUILayout.IntField("Seed:", seed);
        EditorGUILayout.Separator();
        octaves = EditorGUILayout.IntSlider("Octaves:", octaves, 1, 10);
        scale = EditorGUILayout.Slider("Scale:", scale, 50, 1000);
        persistance = EditorGUILayout.Slider("Persistance:", persistance, 0, 1);
        lacunarity = EditorGUILayout.Slider("Lacunarity:", lacunarity, 0, 3);
        offset = EditorGUILayout.Vector2Field("Offset:", offset);

        smoothTerrainHeight = EditorGUILayout.Toggle("Smooth Terrain Height", smoothTerrainHeight);
        terrainHeightCurve = EditorGUILayout.CurveField("Terrain Height Curve:", terrainHeightCurve);

        useFalloffMap = EditorGUILayout.Toggle("Use Falloff Map", useFalloffMap);
        falloffStrength = EditorGUILayout.Slider("Falloff Strength", falloffStrength, 0, 10);
        falloffSpeed = EditorGUILayout.Slider("Falloff Speed", falloffSpeed, 0, 10);
        
        EditorGUILayout.Separator();
    }
}
