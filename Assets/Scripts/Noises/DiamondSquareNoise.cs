/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description:
*/

using UnityEngine;

public class DiamondSquareNoise
{
    
    public static float[,] GenerateNoiseMap(int size, int seed, float rigosity)
    {
        Random.InitState(seed);
        float[,] heightMap = new float[size, size];

        float scale;
        rigosity /= 1000f;
        //float rigosity = 0.0005f;
        
        // init
        heightMap[0, 0] = Random.Range(0f, 1f);
        heightMap[0, size - 1] = Random.Range(0f, 1f);
        heightMap[size - 1, size - 1] = Random.Range(0f, 1f);
        heightMap[size - 1, 0] = Random.Range(0f, 1f);


        int i = size - 1;
        while (i > 1)
        {
            int id = i / 2;
            scale = i * rigosity;

            for (int x = id; x < size; x += i)
            {
                for (int y = id; y < size; y += i)
                {
                    float average = (heightMap[x - id, y - id] + heightMap[x - id, y + id] + heightMap[x + id, y + id] + heightMap[x + id, y - id]) / 4f;
                    heightMap[x, y] = average + Random.Range(0f, 1f) * 2 * scale - scale;
                }
            }

            bool isPair = true;
            for (int y = 0; y < size; y += id)
            {
                for (int x = (isPair) ? id : 0; x < size; x = (i % 2 == 0) ? x + i : x + i - 1)
                {
                    float sum = 0;
                    int n = 0;
                    if (x >= id)
                    {
                        sum = sum + heightMap[x - id, y];
                        n++;
                    }
                    if ((x + id) < size)
                    {
                        sum += heightMap[x + id, y];
                        n++;
                    }
                    if (y >= id)
                    {
                        sum += heightMap[x, y - id];
                        n++;
                    }
                    if ((y + id) < size)
                    {
                        sum += heightMap[x, y + id];
                        n++;
                    }
                    sum /= n;
                    heightMap[x, y] = sum + Random.Range(0f, 1f) * 2 * scale - scale;
                }
                isPair = !isPair;
            }

            i = id;
        }

        return heightMap;
    }
}
