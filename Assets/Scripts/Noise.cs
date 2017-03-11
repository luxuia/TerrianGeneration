using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {
    
    public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves, float amplitude, float frequency, Vector2 offset)
    {
        var map = new float[height, width];

        var randSeeds = new Vector2[octaves];

        float ampl = 1;
        float freq = 1;

        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;

        var rng = new System.Random(seed);

        float absMaxHeight = 0;

        for (int i = 0; i < octaves; ++i)
        {
            randSeeds[i] = new Vector2(rng.Next(-100000, 100000), rng.Next(-100000, 100000));

            absMaxHeight += ampl;
            ampl *= amplitude;
        }

        for (int y = 0; y < height; ++y)
        {
            for (int x =0; x < width; ++x)
            {
                ampl = 1;
                freq = 1;
                float totalHeight = 0;

                for (int i = 0; i < octaves; ++i)
                {
                    float sampleX = (x+offset.x + randSeeds[i].x) / scale * freq;
                    float sampleY = (y+offset.y + randSeeds[i].y) / scale * freq;

                    totalHeight += Mathf.PerlinNoise(sampleX, sampleY) * ampl;

                    freq *= frequency;
                    ampl *= amplitude;
                }

                maxHeight = Mathf.Max(maxHeight, totalHeight);
                minHeight = Mathf.Min(minHeight, totalHeight);

                map[y, x] = totalHeight/absMaxHeight;
            }
        }

       // for (int y = 0; y < height; ++y)
       // {
       //     for (int x = 0; x < width; ++x)
       //     {
       //         map[y, x] = Mathf.InverseLerp(minHeight, maxHeight, map[y, x]);
       //     }
       // }

        return map;
    }
}
