/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description:
*/

using UnityEngine;
using UnityEditor;

public class TerrainGeneratorEditor : EditorWindow
{
    #region variables
    private TerrainGenerator terrainGen;

    private int terrainWidth = 513;
    private int terrainMaxHeight = 600;

    private int heightMapWidth = 513;

    private TerrainProperties.Noises noiseType;

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

    // ***** Custom editor window params *******
    // Custom window enabled check
    private bool isEnabled = false;

    Vector2 scrollPos;
    private bool showAdvancedOptions = false;

    GUIContent guiContent;
    #endregion

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
        SetDefaultGUIColor();

        // Begin scroll view 
        // This shows a vertical scroll bar if the custom window can't show all the information
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // Show the tool logo first
        ShowLogoLabel();

        // Show the terrain properties
        ShowTerrainProperties();

        SetGenerateTerrainGUIColor();
        guiContent = new GUIContent("Generate Terrain", "Generates the terrain");
        if (GUILayout.Button(guiContent, GUILayout.Height(40)))
        {
            GenerateTerrain();
            terrainGen.PaintTerrainBasedOnHeight();
        }

        SetDefaultGUIColor();
        if (GUILayout.Button(showAdvancedOptions ? "Hide Advanced Options" : "Show Advanced Options"))
        {
            showAdvancedOptions = !showAdvancedOptions;
        }

        bool beginFadeGroup = EditorGUILayout.BeginFadeGroup(showAdvancedOptions ? 1 : 0);
        if (beginFadeGroup)
        {
            // ***** Details Properties *****
            EditorGUILayout.LabelField("Details Generation", EditorStyles.boldLabel);
            // Show terrain properties related to textures and details
            ShowTerrainTexturesProperties();

            GUILayout.BeginHorizontal();
            // Texture Terrain Button
            SetGenerateGUIColor();
            guiContent = new GUIContent("Generate Terrain Textures", "Generates Textures on the terrain based on height");
            if (GUILayout.Button(guiContent))
            {
                SaveTerrainTextureLimitsProperties();
                terrainGen = FindObjectOfType<TerrainGenerator>();
                terrainGen.PaintTerrainBasedOnHeight();
            }

            // Clear Details Button
            SetClearGUIColor();
            guiContent = new GUIContent("Clear Terrain Textures", "Clears all the textures from the terrain");
            if (GUILayout.Button(guiContent))
            {
                terrainGen = FindObjectOfType<TerrainGenerator>();
                terrainGen.ClearTerrainTextures();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            // Generate Details Button
            SetGenerateGUIColor();
            guiContent = new GUIContent("Generate Terrain Details", "Generates details (i.e. grass) on terrain based on height");
            if (GUILayout.Button(guiContent))
            {
                terrainGen = FindObjectOfType<TerrainGenerator>();
                terrainGen.GenerateTerrainDetails();
            }

            // Clear Details Button
            SetClearGUIColor();
            guiContent = new GUIContent("Clear Terrain Details", "Clears all the details from the terrain");
            if (GUILayout.Button(guiContent))
            {
                terrainGen = FindObjectOfType<TerrainGenerator>();
                terrainGen.ClearTerrainDetails();
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // ***** Trees Properties *****
            SetDefaultGUIColor();
            EditorGUILayout.LabelField("Trees Generation", EditorStyles.boldLabel);
            ShowTerrainTreesProperties();
            GUILayout.BeginHorizontal();

            // Generate Trees Button
            SetGenerateGUIColor();
            guiContent = new GUIContent("Generate Trees", "Generates trees on terrain based on height");
            if (GUILayout.Button(guiContent))
            {
                SaveTerrainTreesProperties();
                terrainGen = FindObjectOfType<TerrainGenerator>();
                terrainGen.GenerateTerrainTrees();
            }

            // Clear Trees Button
            SetClearGUIColor();
            guiContent = new GUIContent("Clear Trees", "Clears all the trees currently on the terrain");
            if (GUILayout.Button(guiContent))
            {
                terrainGen = FindObjectOfType<TerrainGenerator>();
                terrainGen.ClearTerrainTrees();
            }
            GUILayout.EndHorizontal();
        }
        SetDefaultGUIColor();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Save and Load Terrain Data", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        // Save Terrain Properties to Json File
        guiContent = new GUIContent("Save Terrain to JSON", "Saves all terrain parameters into a json file");
        if (GUILayout.Button(guiContent))
        {
            SaveTerrainToJson();
        }

        // Load Terrain Properties from Json File
        guiContent = new GUIContent("Load Terrain from JSON", "Load a saved terrain from a json file");
        if (GUILayout.Button(guiContent))
        {
            LoadTerrainFromJson();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
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

        TerrainProperties.Instance.NoiseType = (int)noiseType;

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
        SaveTerrainTextureLimitsProperties();

        TerrainProperties.Instance.TreesDensity = treesDensity;
    }

    void LoadTerrainProperties()
    {
        // Terrain size data
        terrainWidth = TerrainProperties.Instance.Width;
        terrainMaxHeight = TerrainProperties.Instance.MaxHeight;

        // Heightmap size data
        heightMapWidth = TerrainProperties.Instance.HeightMapWidth;

        noiseType = (TerrainProperties.Noises)TerrainProperties.Instance.NoiseType;

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
            possibleWidthsString[i] = (possibleWidths[i] - 1).ToString();
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
        guiContent = new GUIContent("Noise Type:", "Choose type of noise for the height map generation.");
        noiseType = (TerrainProperties.Noises) EditorGUILayout.EnumPopup(guiContent, noiseType);

        guiContent = new GUIContent("Generation Seed:", "Terrain generation seed. \nMakes sure that the random functions will generate the same result for the same seed. \nUseful to generate the same terrain again.");
        seed = EditorGUILayout.IntField(guiContent, seed);

        EditorGUILayout.Separator();
        if (noiseType == (TerrainProperties.Noises)TerrainProperties.Noises.Perlin)
        {
            scale = EditorGUILayout.Slider("Map Scale:", scale, 50, 1000);
            octaves = EditorGUILayout.IntSlider("Noise Octaves:", octaves, 1, 10);
            persistance = EditorGUILayout.Slider("Noise Persistance:", persistance, 0, 1);
        }
        lacunarity = EditorGUILayout.Slider("Noise Lacunarity:", lacunarity, 0, 3);

        if (noiseType == (TerrainProperties.Noises)TerrainProperties.Noises.Perlin)
            offset = EditorGUILayout.Vector2Field("Noise Offset:", offset);

        EditorGUILayout.Separator();

        smoothTerrainHeight = EditorGUILayout.BeginToggleGroup("Smoothen Terrain", smoothTerrainHeight);
        if (EditorGUILayout.BeginFadeGroup(smoothTerrainHeight ? 1 : 0))
        {
            terrainHeightCurve = EditorGUILayout.CurveField("Terrain Height Curve:", terrainHeightCurve);
        }
        EditorGUILayout.EndToggleGroup();

        useFalloffMap = EditorGUILayout.BeginToggleGroup("Falloff Terrain", useFalloffMap);
        if (EditorGUILayout.BeginFadeGroup(useFalloffMap ? 1 : 0))
        {
            falloffStrength = EditorGUILayout.Slider("Falloff Strength", falloffStrength, 0, 10);
            falloffSpeed = EditorGUILayout.Slider("Falloff Speed", falloffSpeed, 0, 10);
        }
        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.Separator();
    }

    void ShowTerrainTexturesProperties()
    {
        guiContent = new GUIContent("Texture Blending - Upper Limits", "Controls how terrain textures are blended based on the terrain height map (Range from 0 to 1)");
        EditorGUILayout.LabelField(guiContent);

        guiContent = new GUIContent("Sand Limit:", "Height limit below which terrain will have sand texture");
        sandLimit = EditorGUILayout.Slider(guiContent, sandLimit, 0, 1);
        guiContent = new GUIContent("Grass Limit:", "Height limit below which terrain will have grass texture");
        grassLimit = EditorGUILayout.Slider(guiContent, grassLimit, 0, 1);
        guiContent = new GUIContent("Stone Limit:", "Height limit below which terrain will have stone texture. If this value is below 1, all heights above the stone limit will have a snow texture");
        stoneLimit = EditorGUILayout.Slider(guiContent, stoneLimit, 0, 1);
    }

    void ShowTerrainTreesProperties()
    {
        guiContent = new GUIContent("Trees Density", "Controls the density of the trees on the terrain");
        treesDensity = EditorGUILayout.Slider(guiContent, treesDensity, 0, 100);
    }

    void SaveTerrainTextureLimitsProperties()
    {
        TerrainProperties.Instance.SandLimit = sandLimit;
        TerrainProperties.Instance.GrassLimit = grassLimit;
        TerrainProperties.Instance.StoneLimit = stoneLimit;
        TerrainProperties.Instance.SnowLimit = snowLimit;
    }

    void SaveTerrainTreesProperties()
    {
        TerrainProperties.Instance.TreesDensity = Mathf.Clamp(treesDensity, 0f, 95f);
    }
    
    // Setting GUI Colors
    void SetDefaultGUIColor()
    {
        GUI.backgroundColor = Color.white;
    }

    void SetGenerateTerrainGUIColor()
    {
        GUI.backgroundColor = new Color(0.5f, 0.9f, 0.9f);
    }

    void SetGenerateGUIColor()
    {
        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
    }

    void SetClearGUIColor()
    {
        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
    }
}
