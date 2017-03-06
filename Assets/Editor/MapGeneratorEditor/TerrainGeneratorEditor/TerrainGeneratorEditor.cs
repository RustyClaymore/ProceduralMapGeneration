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

    private TerrainProperties terrainProperties = TerrainProperties.Instance;

    private int terrainWidth = 513;
    //private int terrainLength = 500;
    private int terrainMaxHeight = 600;

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
    private bool useFaloffMap;
    private float faloffStrength;
    private float faloffSpeed;

    // Image for the editor window
    private Texture2D terrainGenerationLogo = null;

    [MenuItem("City Generator/Terrain Generator")]
    static void Init()
    {
        TerrainGeneratorEditor window = (TerrainGeneratorEditor)EditorWindow.GetWindow(typeof(TerrainGeneratorEditor));
    }

    private void OnEnable()
    {
        terrainGenerationLogo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/EditorImages/TerrainGenerationLogo.png", typeof(Texture2D));
        terrainHeightCurve = new AnimationCurve();
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
    }

    void GenerateTerrain()
    {
        SaveTerrainProperties();

        terrainGen = FindObjectOfType<TerrainGenerator>();
        terrainGen.GenerateUnityTerrain();
    }

    void SaveTerrainProperties()
    {
        // Terrain size data
        terrainProperties.Width = terrainWidth;
        terrainProperties.MaxHeight = terrainMaxHeight;

        // Terrain noise map data
        terrainProperties.Scale = scale;
        terrainProperties.Octaves = octaves;
        terrainProperties.Seed = seed;
        terrainProperties.Persistance = persistance;
        terrainProperties.Lacunarity = lacunarity;
        terrainProperties.Offset = offset;

        terrainProperties.SmoothTerrainHeight = smoothTerrainHeight;
        terrainProperties.TerrainHeightCurve = terrainHeightCurve;

        terrainProperties.UseFaloffMap = useFaloffMap;
        terrainProperties.FaloffStrength = faloffStrength;
        terrainProperties.FaloffSpeed = faloffSpeed;
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
            possibleWidthsString[i] = possibleWidths[i].ToString();
        }
        terrainWidth = EditorGUILayout.IntPopup("Terrain Width:", terrainWidth, possibleWidthsString, possibleWidths);

        terrainMaxHeight = EditorGUILayout.IntSlider("Terrain MaxHeight:", terrainMaxHeight, 100, 600);

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

        useFaloffMap = EditorGUILayout.Toggle("Use Faloff Map", useFaloffMap);
        faloffStrength = EditorGUILayout.Slider("Faloff Strength", faloffStrength, 0, 10);
        faloffSpeed = EditorGUILayout.Slider("Faloff Speed", faloffSpeed, 0, 10);
        
        EditorGUILayout.Separator();
    }

    //public override void OnInspectorGUI()
    //{
    //    TerrainGenerator terGen = (TerrainGenerator)target;

    //    if (DrawDefaultInspector())
    //    {
    //        if (terGen.autoUpdate)
    //        {
    //            terGen.GenerateUnityTerrain();
    //        }
    //    }

    //    if (GUILayout.Button("Generate"))
    //    {
    //        terGen.GenerateUnityTerrain();
    //    }
    //}
}
