/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description: Generator for the unity terrain
*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    
    public Terrain terrain;

    private TerrainProperties terProps = TerrainProperties.Instance;
    private int currentTerrainWidth;

    private void Start()
    {
        //terrain.terrainData.heightmapResolution = terProps.Width;
        //terrain.terrainData.size = new Vector3(terProps.Width, terProps.MaxHeight, terProps.Width);
        //currentTerrainWidth = terProps.Width;
    }

    public void GenerateUnityTerrain()
    {
        // Generate the height map using Perlin Noise
        // Assigning heightmap size needs to be done before assigning terrain size
        terrain.terrainData.heightmapResolution = terProps.HeightMapWidth;
        terrain.terrainData.alphamapResolution = terProps.HeightMapWidth;

        float[,] heightMap;
        if (terProps.NoiseType == (int)TerrainProperties.Noises.Perlin)
        {
            heightMap = PerlinNoise.GenerateNoiseMap(terProps.HeightMapWidth, terProps.HeightMapWidth, terProps.Seed, terProps.Scale,
                                                            terProps.Octaves, terProps.Persistance, terProps.Lacunarity, terProps.Offset);
        }
        else
        {

            heightMap = DiamondSquareNoise.GenerateNoiseMap(terProps.HeightMapWidth, terProps.Seed, terProps.Lacunarity);
        }

        terrain.terrainData.size = new Vector3(terProps.Width, terProps.MaxHeight, terProps.Width);
        currentTerrainWidth = terProps.Width;

        // Smooth the height map using an animation curve
        if (terProps.SmoothTerrainHeight)
            SmoothHeightMapWithCurve(ref heightMap);

        // If we're using the faloff map, substract the faloff map from the height map
        if (terProps.UseFalloffMap)
            SubstractFaloffMap(ref heightMap, 1);
        
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }

    //float[,] DiamondSquareNoise(int size, int seed, float rigosity)
    //{
    //    Random.InitState(seed);
    //    //size = size + 1;
    //    float[,] heightMap = new float[size, size];

    //    float scale;
    //    //rigosity = 0.0005f;
    //    rigosity /= 1000f;

    //    // init
    //    heightMap[0, 0] = Random.Range(0f, 1f);
    //    heightMap[0, size - 1] = Random.Range(0f, 1f);
    //    heightMap[size - 1, size - 1] = Random.Range(0f, 1f);
    //    heightMap[size - 1, 0] = Random.Range(0f, 1f);


    //    int i = size - 1;
    //    while (i > 1)
    //    {
    //        int id = i / 2;
    //        scale = i * rigosity;

    //        for (int x = id; x < size; x += i)
    //        {
    //            for (int y = id; y < size; y += i)
    //            {
    //                float average = (heightMap[x - id, y - id] + heightMap[x - id, y + id] + heightMap[x + id, y + id] + heightMap[x + id, y - id]) / 4f;
    //                heightMap[x, y] = Mathf.Clamp01(average + Random.Range(0f, 1f) * 2 * scale - scale);
    //            }
    //        }

    //        bool isPair = true;
    //        for (int y = 0; y < size; y += id)
    //        {
    //            for (int x = (isPair) ? id : 0; x < size; x = (i % 2 == 0) ? x + i : x + i - 1)
    //            {
    //                float sum = 0;
    //                int n = 0;
    //                if (x >= id)
    //                {
    //                    sum = sum + heightMap[x - id, y];
    //                    n++;
    //                }
    //                if ((x + id) < size)
    //                {
    //                    sum += heightMap[x + id, y];
    //                    n++;
    //                }
    //                if (y >= id)
    //                {
    //                    sum += heightMap[x, y - id];
    //                    n++;
    //                }
    //                if ((y + id) < size)
    //                {
    //                    sum += heightMap[x, y + id];
    //                    n++;
    //                }
    //                sum /= n;
    //                heightMap[x, y] = Mathf.Clamp01(sum + Random.Range(0f, 1f) * 2 * scale - scale);
    //            }
    //            isPair = !isPair;
    //        }

    //        i = id;
    //    }

    //    return heightMap;
    //}

    public void ClearTerrainTrees()
    {
        TerrainData terrainData = terrain.terrainData;
        List<TreeInstance> trees = new List<TreeInstance>(terrain.terrainData.treeInstances);

        trees.Clear();
        terrainData.treeInstances = trees.ToArray();

        float[,] heights = terrainData.GetHeights(0, 0, 0, 0);
        terrainData.SetHeights(0, 0, heights);
        //terrainData.treePrototypes = null;
    }

    public void GenerateTerrainTrees()
    {
        ClearTerrainTrees();

        TerrainData terrainData = terrain.terrainData;
        
        List<TreeInstance> treeInstances = new List<TreeInstance>(terrain.terrainData.treeInstances);
        
        for (int i = 0; i < terrainData.heightmapWidth; i++)
        {
            for (int j = 0; j < terrainData.heightmapHeight; j++)
            {
                // Normalize x and y
                float norX = (float)i / terrain.terrainData.heightmapWidth;
                float norY = (float)j / terrain.terrainData.heightmapHeight;

                // read the height at this location
                float height = terrain.terrainData.GetHeight(Mathf.RoundToInt(norY * terrain.terrainData.heightmapHeight),
                                                             Mathf.RoundToInt(norX * terrain.terrainData.heightmapWidth)) / terrain.terrainData.size.y;


                // Create tree instance
                TreeInstance treeInstance = new TreeInstance();
                treeInstance.prototypeIndex = 0;
                treeInstance.color = new Color(1, 1, 1);
                treeInstance.lightmapColor = new Color(1, 1, 1);
                treeInstance.heightScale = Random.Range(0.7f, 1.3f);
                treeInstance.widthScale = Random.Range(0.9f, 1.1f);

                treeInstance.position = TerrainToWorldPos(norY * terrain.terrainData.size[2], norX * terrain.terrainData.size[0]);
                //treeInstance.position.y = height;
                var angle = Vector3.Angle(Vector3.up, terrain.terrainData.GetInterpolatedNormal(treeInstance.position.x, treeInstance.position.z));

                if (height < terProps.SandLimit)
                {
                    
                }
                else if (height >= terProps.SandLimit && height < terProps.GrassLimit)
                {
                    float interp = Mathf.InverseLerp(terProps.SandLimit, terProps.GrassLimit, height);
                    float x = Random.Range(0, 100 - terProps.TreesDensity);
                    if (x < (0.2f * (1f - interp)))
                        treeInstances.Add(treeInstance);
                }
                else if (height >= terProps.GrassLimit && height < terProps.StoneLimit)
                {
                    float interp = Mathf.InverseLerp(terProps.GrassLimit, terProps.StoneLimit, height);
                    float x = Random.Range(0, 100 - terProps.TreesDensity);
                    if (x < 0.05f && interp < 0.1f)
                        treeInstances.Add(treeInstance);
                }
                else if (height >= terProps.StoneLimit && height < 1)
                {
                   
                }
            }
        }

        // Assign tree instances to the terrain
        terrain.terrainData.treeInstances = treeInstances.ToArray();
        // Reapply the height map to adjust the trees positions 
        var heights = terrain.terrainData.GetHeights(0, 0, terProps.HeightMapWidth, terProps.HeightMapWidth);
        terrain.terrainData.SetHeights(0, 0, heights);
        terrain.Flush();
    }

    Vector3 TerrainToWorldPos(float coorX, float coorZ)
    {
        float x = coorX / terrain.terrainData.size[0];
        float z = coorZ / terrain.terrainData.size[2];
        float y = terrain.SampleHeight(new Vector3(x, 0, z)) / terrain.terrainData.size[1];

        return new Vector3(x, y, z);
    }

    public void ClearTerrainDetails()
    {
        // Get grass layer data
        var map1 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 0);
        var map2 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 1);

        // Set each element of the detail map to zero
        for (int j = 0; j < terrain.terrainData.detailHeight; j++)
        {
            for (int i = 0; i < terrain.terrainData.detailWidth; i++)
            {
                map1[i, j] = 0;
                map2[i, j] = 0;
            }
        }
        
        // Assign the cleared detail map
        terrain.terrainData.SetDetailLayer(0, 0, 0, map1);
        terrain.terrainData.SetDetailLayer(0, 0, 1, map2);
    }

    public void GenerateTerrainDetails()
    {
        // Get grass layer data
        var map1 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 0);
        var map2 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 1);


        Debug.Log(terrain.terrainData.size.y);

        // Set each element of the detail map depending on the height map
        for (int j = 0; j < terrain.terrainData.detailHeight; j++)
        {
            for (int i = 0; i < terrain.terrainData.detailWidth; i++)
            {
                // Normalize x and y
                float norX = (float)i / terrain.terrainData.alphamapWidth;
                float norY = (float)j / terrain.terrainData.alphamapHeight;

                // read the height at this location
                float height = terrain.terrainData.GetHeight(Mathf.RoundToInt(norY * terrain.terrainData.heightmapHeight),
                                                             Mathf.RoundToInt(norX * terrain.terrainData.heightmapWidth)) / terrain.terrainData.size.y;
                
                if (height < terProps.SandLimit)
                {
                    map1[i, j] = 0;
                    map2[i, j] = 0;
                }
                else if (height >= terProps.SandLimit && height < terProps.GrassLimit)
                {
                    float t = (height - terProps.SandLimit) / (terProps.GrassLimit - terProps.SandLimit);
                    float rand = Random.Range(0f, 1f);
                    if(rand <= 0.5f)
                    {
                        map1[i, j] = Mathf.FloorToInt(Mathf.Lerp(2, 6, t));
                        map2[i, j] = 0;
                    }
                    else
                    {
                        map1[i, j] = 0;
                        map2[i, j] = Mathf.FloorToInt(Mathf.Lerp(2, 6, t));
                    }
                }
                else if (height >= terProps.GrassLimit && height < terProps.StoneLimit)
                {
                    float rand = Random.Range(0f, 1f);
                    if (rand <= 0.5f)
                    {
                        map1[i, j] = 1;
                        map2[i, j] = 0;
                    }
                    else
                    {
                        map1[i, j] = 0;
                        map2[i, j] = 1;
                    }
                }
                else if (height >= terProps.StoneLimit && height < 1)
                {
                    map1[i, j] = 0;
                    map2[i, j] = 0;
                }
            }
        }

        // Assign the cleared detail map
        terrain.terrainData.SetDetailLayer(0, 0, 0, map1);
        terrain.terrainData.SetDetailLayer(0, 0, 1, map2);
    }

    public void ClearTerrainTextures()
    {
        float[,,] splatmapData = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapLayers];
        for (int y = 0; y < terrain.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrain.terrainData.alphamapWidth; x++)
            {
                splatmapData[x, y, 0] = 1;
            }
        }
        // Finally assign the new splatmap to the terrainData:
        terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    public void PaintTerrainBasedOnHeight()
    {
        float[,,] splatmapData = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapLayers];
        
        for (int y = 0; y < terrain.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrain.terrainData.alphamapWidth; x++)
            {
                // Normalize x and y
                float norX = (float)x / terrain.terrainData.alphamapWidth;
                float norY = (float)y / terrain.terrainData.alphamapHeight;

                // read the height at this location
                float height = terrain.terrainData.GetHeight(Mathf.RoundToInt(norY * terrain.terrainData.heightmapHeight),
                                                             Mathf.RoundToInt(norX * terrain.terrainData.heightmapWidth)) / (int)terrain.terrainData.size.y;

                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                Vector3 normal = terrain.terrainData.GetInterpolatedNormal(norY, norX);

                // Calculate the steepness of the terrain
                float steepness = terrain.terrainData.GetSteepness(norY, norX);

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrain.terrainData.alphamapLayers];

                
                if (height < terProps.SandLimit)
                {
                    splatmapData[x, y, 0] = (1 - height) / 0.75f;
                }
                else if (height >= terProps.SandLimit && height < terProps.GrassLimit)
                {
                    splatmapData[x, y, 1] = 1 - height * 0.7f;
                }
                else if (height >= terProps.GrassLimit && height < terProps.StoneLimit)
                {
                    splatmapData[x, y, 1] = 1 - height * 1.2f;
                    splatmapData[x, y, 2] = 1 - height * 2;
                }
                if (height >= terProps.StoneLimit && height < 1)
                {
                    if (height >= terProps.StoneLimit + 0.07f && normal.y >= 0.7f && steepness < 70)
                        splatmapData[x, y, 3] = Mathf.Clamp01(height * 1.2f);
                    else if(normal.x > 0 && height < terProps.StoneLimit + 0.1f)
                    {
                        splatmapData[x, y, 1] = 1 - height * 1.8f;
                        splatmapData[x, y, 2] = 1 - height;
                    }
                    else
                    {
                        splatmapData[x, y, 2] = Mathf.Clamp01(height);
                    }
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
