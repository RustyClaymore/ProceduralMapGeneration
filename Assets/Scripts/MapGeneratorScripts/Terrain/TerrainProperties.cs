/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description: Terrain Properties class holder
*/

using UnityEngine;
using System.IO;

[System.Serializable]
public class TerrainProperties {

    #region Variables
    // Json File variables
    private string jsonString;

    // Terrain data 
    [SerializeField]
    private int width;
    [SerializeField]
    private int maxHeight;

    [SerializeField]
    private int heightMapWidth;

    [SerializeField]
    private static TerrainProperties instance = null;

    // Noise data 
    [SerializeField]
    private int seed;
    [SerializeField]
    private float scale;
    [SerializeField]
    private int octaves;
    [SerializeField]
    private float persistance;
    [SerializeField]
    private float lacunarity;
    [SerializeField]
    private Vector2 offset;

    [SerializeField]
    private bool smoothTerrainHeight;
    [SerializeField]
    private AnimationCurve terrainHeightCurve;

    [SerializeField]
    private bool useFalloffMap;
    [SerializeField]
    private float falloffStrength;
    [SerializeField]
    private float falloffSpeed;

    #endregion

    public TerrainProperties()
    {
        Width = 1000;
        MaxHeight = 600;
    }

    public string SaveToString(string path)
    {
        jsonString = JsonUtility.ToJson(this);
        File.WriteAllText(path, jsonString);

        return JsonUtility.ToJson(this);        
    }

    public TerrainProperties CreateFromJSON(string path)
    {
        string jsonString = File.ReadAllText(path);
        TerrainProperties tp = new TerrainProperties();
        tp = JsonUtility.FromJson<TerrainProperties>(jsonString);
        CopyData(tp);

        return JsonUtility.FromJson<TerrainProperties>(jsonString);
    }

    public static TerrainProperties Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TerrainProperties();
            }
            return instance;
        }
    }

    private void CopyData(TerrainProperties tp)
    {
        // Terrain size data
        Instance.Width = tp.Width;
        Instance.MaxHeight = tp.MaxHeight;

        // Height Map size data
        Instance.HeightMapWidth = tp.HeightMapWidth;

        // Terrain noise map data
        Instance.Scale = tp.Scale;
        Instance.Octaves = tp.Octaves;
        Instance.Seed = tp.Seed;
        Instance.Persistance = tp.Persistance;
        Instance.Lacunarity = tp.Lacunarity;
        Instance.Offset = tp.Offset;

        Instance.SmoothTerrainHeight = tp.SmoothTerrainHeight;
        Instance.TerrainHeightCurve = tp.TerrainHeightCurve;

        Instance.UseFalloffMap = tp.UseFalloffMap;
        Instance.FalloffStrength = tp.FalloffStrength;
        Instance.falloffSpeed = tp.FalloffSpeed;
    }

    public int Width
    {
        get
        {
            return width;
        }

        set
        {
            width = value;
        }
    }

    public int MaxHeight
    {
        get
        {
            return maxHeight;
        }

        set
        {
            maxHeight = value;
        }
    }

    public float Scale
    {
        get
        {
            return scale;
        }

        set
        {
            scale = value;
        }
    }

    public int Octaves
    {
        get
        {
            return octaves;
        }

        set
        {
            octaves = value;
        }
    }

    public float Persistance
    {
        get
        {
            return persistance;
        }

        set
        {
            persistance = value;
        }
    }

    public float Lacunarity
    {
        get
        {
            return lacunarity;
        }

        set
        {
            lacunarity = value;
        }
    }

    public int Seed
    {
        get
        {
            return seed;
        }

        set
        {
            seed = value;
        }
    }

    public Vector2 Offset
    {
        get
        {
            return offset;
        }

        set
        {
            offset = value;
        }
    }

    public AnimationCurve TerrainHeightCurve
    {
        get
        {
            return terrainHeightCurve;
        }

        set
        {
            terrainHeightCurve = value;
        }
    }

    public bool UseFalloffMap
    {
        get
        {
            return useFalloffMap;
        }

        set
        {
            useFalloffMap = value;
        }
    }

    public bool SmoothTerrainHeight
    {
        get
        {
            return smoothTerrainHeight;
        }

        set
        {
            smoothTerrainHeight = value;
        }
    }

    public float FalloffStrength
    {
        get
        {
            return falloffStrength;
        }

        set
        {
            falloffStrength = value;
        }
    }

    public float FalloffSpeed
    {
        get
        {
            return falloffSpeed;
        }

        set
        {
            falloffSpeed = value;
        }
    }

    public int HeightMapWidth
    {
        get
        {
            return heightMapWidth;
        }

        set
        {
            heightMapWidth = value;
        }
    }
}
