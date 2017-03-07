/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description: A falloff map generator. To be used alongside a height map to create islands.
*/

using UnityEngine;

public class FalloffMapGenerator {

    #region Variables

    #endregion

    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    static float Evaluate(float value)
    {
        float a = TerrainProperties.Instance.FalloffStrength;
        float b = TerrainProperties.Instance.FalloffSpeed;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
