/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description: Terrain Properties class holder
*/

using UnityEngine;

[System.Serializable]
public class TerrainProperties {

    #region Variables

    // Terrain data 
    private int width;
    private int maxHeight;

    private static TerrainProperties instance = null;
    private static readonly object plock = new object();

    // Noise data 
    private int seed;
    private float scale;
    private int octaves;
    private float persistance;
    private float lacunarity;
    private Vector2 offset;

    private bool smoothTerrainHeight;
    private AnimationCurve terrainHeightCurve;

    private bool useFaloffMap;
    private float faloffStrength;
    private float faloffSpeed;

    #endregion

    public TerrainProperties()
    {
        Width = 1000;
        MaxHeight = 600;
    }

    public string SaveToString()
    {
        Debug.Log(JsonUtility.ToJson(this));
        return JsonUtility.ToJson(this);        
    }

    public static TerrainProperties CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<TerrainProperties>(jsonString);
    }

    public static TerrainProperties Instance
    {
        get
        {
            lock (plock)
            {
                if (instance == null)
                {
                    instance = new TerrainProperties();
                }
                return instance;
            }
        }
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

    public bool UseFaloffMap
    {
        get
        {
            return useFaloffMap;
        }

        set
        {
            useFaloffMap = value;
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

    public float FaloffStrength
    {
        get
        {
            return faloffStrength;
        }

        set
        {
            faloffStrength = value;
        }
    }

    public float FaloffSpeed
    {
        get
        {
            return faloffSpeed;
        }

        set
        {
            faloffSpeed = value;
        }
    }
}
