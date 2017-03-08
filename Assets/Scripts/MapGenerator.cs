using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public int meshSize;
    public float scale;
    public int seed;
    public int octaves;

    [Range(0, 5)]
    public int lod;

    [Range(0,1)]
    public float amplitude;
    public float frequency;
    public Vector2 offsets;

    public float meshHeightMulti = 1;
    public AnimationCurve heightAdjuestCurve;

    public bool useFalloff;

    public LayerColorData[] layers;


    public void DrawMap()
    {
        var display = GetComponent<MapDisplay>();

        var noiseMap = Noise.GenerateNoiseMap(meshSize, meshSize, seed, scale, octaves, amplitude, frequency, offsets);

        if (useFalloff)
        {
            var falloffData = FalloffGenerator.GenerateFalloffMap(meshSize);
            for (int y = 0; y < meshSize; y++)
            {
                for (int x = 0; x < meshSize; x++)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffData[x, y]);
                }
            }
        }

        switch(drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap, layers));
                break;
            case DrawMode.Mesh:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap, layers));
                display.DrawMesh(MeshGenerator.GenerateTerrianMesh(noiseMap, meshHeightMulti, lod, heightAdjuestCurve));
                break;
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(meshSize), layers));
                break;
        }
    }

    public MeshData RequestMeshData(Vector2 center)
    {
        var noiseMap = Noise.GenerateNoiseMap(meshSize, meshSize, seed, scale, octaves, amplitude, frequency, center);

        if (useFalloff)
        {
            var falloffData = FalloffGenerator.GenerateFalloffMap(meshSize);
            for (int y = 0; y < meshSize; y++)
            {
                for (int x = 0; x < meshSize; x++)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffData[x, y]);
                }
            }
        }
        return MeshGenerator.GenerateTerrianMesh(noiseMap, meshHeightMulti, lod, heightAdjuestCurve);
    }

    void OnValidate()
    {
        meshSize = meshSize > 0 ? meshSize : 1;
        scale = scale > 0 ? scale : 1;

        DrawMap();
    }
}
