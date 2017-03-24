using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

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


    Queue<ThreadInfo<MapData>> mMapDataQueue = new Queue<ThreadInfo<MapData>>();
    Queue<ThreadInfo<MeshData>> mMeshDataQueue = new Queue<ThreadInfo<MeshData>>();


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

    MapData GeneratorMapData(Vector2 center)
    {
        var fixedMeshSize = meshSize + 1;
        var noiseMap = Noise.GenerateNoiseMap(fixedMeshSize, fixedMeshSize, seed, scale, octaves, amplitude, frequency, center);

        if (useFalloff)
        {
            var falloffData = FalloffGenerator.GenerateFalloffMap(fixedMeshSize);
            for (int y = 0; y < fixedMeshSize; y++)
            {
                for (int x = 0; x < fixedMeshSize; x++)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffData[x, y]);
                }
            }
        }
        return new MapData(noiseMap);
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart thread = delegate ()
        {
            var mapData = GeneratorMapData(center);

            lock(mMapDataQueue)
            {
                mMapDataQueue.Enqueue(new ThreadInfo<MapData>(callback, mapData));
            }
        };

        new Thread(thread).Start();
    }

    public void RequestMeshData(MapData mapData, int overrideLod, Action<MeshData> callback)
    {
        ThreadStart thread = delegate ()
        {
            var meshData = MeshGenerator.GenerateTerrianMesh(mapData.HeightMap, meshHeightMulti, overrideLod, heightAdjuestCurve);

            lock(mMeshDataQueue)
            {
                mMeshDataQueue.Enqueue(new ThreadInfo<MeshData>(callback, meshData));
            }
        };

        new Thread(thread).Start();
    }

    public Texture2D RequestTextureData(MapData mapData)
    {
        return TextureGenerator.TextureFromHeightMap(mapData.HeightMap, layers);
    }

    void Update()
    {
        if (mMapDataQueue.Count > 0)
        {
            for (int i = 0; i < mMapDataQueue.Count; ++i)
            {
                var info = mMapDataQueue.Dequeue();
                info.callback(info.param);
            }
        }
        if (mMeshDataQueue.Count > 0)
        {
            for (int i = 0; i < mMeshDataQueue.Count; ++i)
            {
                var info = mMeshDataQueue.Dequeue();
                info.callback(info.param);
            }
        }
    }

    void OnValidate()
    {
        meshSize = meshSize > 0 ? meshSize : 1;
        scale = scale > 0 ? scale : 1;

        //DrawMap();
    }

    public struct ThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T param;

        public ThreadInfo(Action<T> callback, T param)
        {
            this.callback = callback;
            this.param = param;
        }
    }

}

public struct MapData
{
    public readonly float[,] HeightMap;
    public MapData(float[,] heightMap)
    {
        HeightMap = heightMap;
    }
}