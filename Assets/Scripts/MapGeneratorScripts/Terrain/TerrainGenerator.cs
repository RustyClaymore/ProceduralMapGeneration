/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description: Generator for the unity terrain
*/

using System.Linq;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    
    public Terrain terrain;

    private TerrainProperties terProps = TerrainProperties.Instance;
    private int currentTerrainWidth;

    private void Start()
    {
        terrain.terrainData.heightmapResolution = terProps.Width;
        terrain.terrainData.size = new Vector3(terProps.Width, terProps.MaxHeight, terProps.Width);
        currentTerrainWidth = terProps.Width;
    }

    public void GenerateUnityTerrain()
    {
        // Generate the height map using Perlin Noise
        // Assigning heightmap size needs to be done before assigning terrain size
        terrain.terrainData.heightmapResolution = terProps.HeightMapWidth;
        terrain.terrainData.alphamapResolution = terProps.HeightMapWidth;

        float[,] heightMap = PerlinNoise.GenerateNoiseMap(terProps.HeightMapWidth, terProps.HeightMapWidth, terProps.Seed, terProps.Scale,
                                                        terProps.Octaves, terProps.Persistance, terProps.Lacunarity, terProps.Offset);

        terrain.terrainData.size = new Vector3(terProps.Width, terProps.MaxHeight, terProps.Width);
        currentTerrainWidth = terProps.Width;

        // Smooth the height map using an animation curve
        if (terProps.SmoothTerrainHeight)
            SmoothHeightMapWithCurve(ref heightMap);

        // If we're using the faloff map, substract the faloff map from the height map
        if (terProps.UseFalloffMap)
            SubstractFaloffMap(ref heightMap, 1);
        
        terrain.terrainData.SetHeights(0, 0, heightMap);
        PaintTerrainBasedOnHeight();
    }

    void PaintTerrainBasedOnHeight()
    {
        float[,,] splatmapData = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapLayers];
        
        Debug.Log(terrain.terrainData.GetHeight(0, 0) / terProps.MaxHeight);

        for (int y = 0; y < terrain.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrain.terrainData.alphamapWidth; x++)
            {
                // Normalize x and y
                float norX = (float)x / terrain.terrainData.alphamapWidth;
                float norY = (float)y / terrain.terrainData.alphamapHeight;

                // read the height at this location
                float height = terrain.terrainData.GetHeight(Mathf.RoundToInt(norY * terrain.terrainData.alphamapHeight),
                                                             Mathf.RoundToInt(norX * terrain.terrainData.alphamapWidth)) / terProps.MaxHeight;

                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                Vector3 normal = terrain.terrainData.GetInterpolatedNormal(norY, norX);

                // Calculate the steepness of the terrain
                float steepness = terrain.terrainData.GetSteepness(norY, norX);

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrain.terrainData.alphamapLayers];

                if (height < 0.25f)
                {
                    splatmapData[x, y, 1] = 1 - height * 1.1f;
                }
                else if (height >= 0.25f && height < 0.45f)
                {
                    splatmapData[x, y, 0] = (1 - height) / 0.75f;
                }
                else if (height >= 0.45f && height < 1)
                {
                    splatmapData[x, y, 2] = Mathf.Clamp01(height * 1.2f);
                }

                // Texture[0] has constant influence
                //splatWeights[0] = 0.3f;

                //// Texture[1] is stronger at lower altitudes
                //splatWeights[1] = Mathf.Clamp01((terrain.terrainData.heightmapHeight - height));

                //// Texture[2] stronger on flatter terrain
                //// Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
                //// Subtract result from 1.0 to give greater weighting to flat surfaces
                //splatWeights[2] = 1.0f - Mathf.Clamp01(steepness * steepness / (terrain.terrainData.heightmapHeight / 5.0f));

                //// Texture[3] increases with height but only on surfaces facing positive Z axis 
                //splatWeights[3] = height * Mathf.Clamp01(normal.z);

                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                //float z = splatWeights.Sum();

                //// Loop through each terrain texture
                //for (int i = 0; i < terrain.terrainData.alphamapLayers; i++)
                //{

                //    // Normalize so that sum of all texture weights = 1
                //    splatWeights[i] /= z;

                //    // Assign this point to the splatmap array
                //    splatmapData[x, y, i] = splatWeights[i];
                //}

                //// determine the mix of textures 1, 2 and 3 to use 
                //// (using a vector3, since it can be lerped and normalized)
                //Vector3 splat = new Vector3(0, 1, 0);
                //if (height > 0.5f)
                //{
                //    splat = Vector3.Lerp(splat, new Vector3(0, 0, 1), (height - 0.5f) * 2);
                //}
                //else
                //{
                //    splat = Vector3.Lerp(splat, new Vector3(1, 0, 0), height * 2);
                //}
                //// now assign the values to the correct location in the array
                //splat.Normalize();
                //splatmapData[x, y, 0] = splat.x;
                //splatmapData[x, y, 1] = splat.y;
                //splatmapData[x, y, 2] = splat.z;
            }
        }

        // Finally assign the new splatmap to the terrainData:
        terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
    }

void SubstractFaloffMap(ref float[,] heightMap, float strength)
    {
        int width = terrain.terrainData.heightmapWidth;
        float[,] faloffMap = FalloffMapGenerator.GenerateFalloffMap(width);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                heightMap[i, j] = Mathf.Clamp01(heightMap[i, j] - faloffMap[i, j] / strength);
            }
        }
    }

    void SmoothHeightMapWithCurve(ref float[, ] heightMap)
    {
        int width = terrain.terrainData.heightmapWidth;
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                heightMap[i,j] = terProps.TerrainHeightCurve.Evaluate(heightMap[i, j]);
            }
        }
    }
}
