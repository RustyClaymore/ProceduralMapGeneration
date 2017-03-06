/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description: Generator for the unity terrain
*/

using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    
    public Terrain terrain;

    TerrainProperties terProps = TerrainProperties.Instance;

    private void Start()
    {
        terrain.terrainData.size = new Vector3(terProps.Width, terProps.MaxHeight, terProps.Width);
    }

    public void GenerateUnityTerrain()
    {
        terrain.terrainData.heightmapResolution = terProps.Width;
        terrain.terrainData.size = new Vector3(terProps.Width, terProps.MaxHeight, terProps.Width);

        // Generate the height map using Perlin Noise
        float[,] heightMap = PerlinNoise.GenerateNoiseMap(terProps.Width, terProps.Width, terProps.Seed, terProps.Scale,
                                                        terProps.Octaves, terProps.Persistance, terProps.Lacunarity, terProps.Offset);

        // Smooth the height map using an animation curve
        if(terProps.SmoothTerrainHeight)
            SmoothHeightMapWithCurve(ref heightMap);

        // If we're using the faloff map, substract the faloff map from the height map
        if (terProps.UseFaloffMap)
            SubstractFaloffMap(ref heightMap, 1);
        
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }
    
    void SubstractFaloffMap(ref float[,] heightMap, float strength)
    {
        float[,] faloffMap = FalloffMapGenerator.GenerateFalloffMap(terProps.Width);

        for (int i = 0; i < terProps.Width; i++)
        {
            for (int j = 0; j < terProps.Width; j++)
            {
                heightMap[i, j] = Mathf.Clamp01(heightMap[i, j] - faloffMap[i, j] / strength);
            }
        }
    }

    void SmoothHeightMapWithCurve(ref float[, ] heightMap)
    {
        int width = terrain.terrainData.heightmapWidth;
        
        for (int i = 0; i < terrain.terrainData.size.x; i++)
        {
            for (int j = 0; j < terrain.terrainData.size.x; j++)
            {
                heightMap[i,j] = terProps.TerrainHeightCurve.Evaluate(heightMap[i, j]);
            }
        }
    }
}
