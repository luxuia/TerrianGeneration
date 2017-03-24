using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class EndlessTerrian : MonoBehaviour {

    public Transform Viewer;
    public int ViewDist = 1;
    public Material TerrainMaterial;

    public static MapGenerator generator;

    int ChunkSize = 256;

    Dictionary<ChunkIdx, TerrainChunk> visibleChunk = new Dictionary<ChunkIdx, TerrainChunk>();

    public static ChunkIdx currentIdx;

    public static int MAX_LOD = 4;

	// Use this for initialization
	void Start () {
        generator = GetComponent<MapGenerator>();
        ChunkSize = generator.meshSize;

    }

    // Update is called once per frame
    void Update() {
        var pos = Viewer.position;
        var x = Mathf.FloorToInt(pos.x / ChunkSize);
        var z = Mathf.FloorToInt(pos.z / ChunkSize);


        if (currentIdx == null || currentIdx.X != x || currentIdx.Z != z)
        {
            currentIdx = new ChunkIdx(x, z);

            UpdateTerrainChunk();
        }
	}

    bool IsChunkVisible(int x, int z)
    {
        return x + z >= -1 && x + z <= ViewDist * 2 && z - x >= -2 * ViewDist && z - x < 2 * ViewDist;
    }

    void UpdateTerrainChunk()
    {
        for (int i = -ViewDist; i <= 2 * ViewDist; ++i)
        {
            for (int j = -ViewDist; j <= 2 * ViewDist; ++j)
            {
                if (IsChunkVisible(i, j))
                {
                    TerrainChunk chunk = null;
                    var chunkIdx = new ChunkIdx(currentIdx.X + i, currentIdx.Z + j);
                    visibleChunk.TryGetValue(chunkIdx, out chunk);
                    if (chunk == null)
                    {
                        chunk = new TerrainChunk(chunkIdx, ChunkSize, TerrainMaterial);
                    } else
                    {
                        chunk.UpdateChunk();
                    }

                    visibleChunk[chunkIdx] = chunk;
                }
            }
        }

        var outsideChunkds = visibleChunk.Keys.Where((chunkIdx) => !IsChunkVisible(chunkIdx.X-currentIdx.X, chunkIdx.Z-currentIdx.Z)).ToList();
        foreach (var chunkIdx in outsideChunkds)
        {
            var chunk = visibleChunk[chunkIdx];
            chunk.Destroy();
            visibleChunk.Remove(chunkIdx);
        }
    }

    public class ChunkIdx
    {
        public int X;
        public int Z;
        string _str;

        public ChunkIdx(int x, int z)
        {
            X = x; Z = z;

            _str = string.Format("{0},{1}", X, Z);
        }

        public override string ToString()
        {
            return _str;
        }

        public override int GetHashCode()
        {
            return _str.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(ChunkIdx)) return false;
            return obj.GetHashCode() == GetHashCode();
        }
    }

    public class TerrainChunk
    {
        public ChunkIdx chunkIdx;

        public LodMesh[] meshData;

        public Vector2 center;

        public GameObject obj;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public MeshCollider meshCollider;

        MapData mapData;
        bool mapDataReceived;
        int previousLod;

        public TerrainChunk(ChunkIdx idx, float chunkSize, Material material)
        {
            chunkIdx = idx;

            center = new Vector2(chunkIdx.X * chunkSize, chunkIdx.Z * chunkSize);

            obj = new GameObject(chunkIdx.ToString());
            meshFilter = obj.AddComponent<MeshFilter>();
            meshRenderer = obj.AddComponent<MeshRenderer>();
            meshCollider = obj.AddComponent<MeshCollider>();

            previousLod = -1;
            meshRenderer.material = material;

            obj.transform.position = new Vector3(center.x, 0, center.y);

            meshData = new LodMesh[MAX_LOD+1];
            for (int i = 0; i < MAX_LOD+1; ++i)
            {
                meshData[i] = new LodMesh(i, UpdateChunk);
            }

            generator.RequestMapData(center, OnReceivedMapData);
        }

        void OnReceivedMapData(MapData data)
        {
            mapData = data;
            mapDataReceived = true;

            UpdateChunk();
        }

        public void UpdateChunk()
        {
            if (mapDataReceived )
            {
                var lod = Mathf.Abs(currentIdx.X - chunkIdx.X) + Mathf.Abs(currentIdx.Z - chunkIdx.Z);
                lod = Mathf.Min(lod/4, MAX_LOD);

                if (previousLod != lod)
                {
                    var data = meshData[lod];
                    if (data.hasMesh)
                    {
                        meshFilter.sharedMesh = data.mesh;
                        meshCollider.sharedMesh = data.mesh;
                        meshRenderer.material.mainTexture = data.texture;

                        previousLod = lod;
                    } else if (!data.hasRequest)
                    {
                        data.RequestMesh(mapData);
                    }
                }
            }
        }

        public void Destroy()
        {
            if (meshFilter)
            {
                if (meshFilter.sharedMesh) 
                    GameObject.Destroy(meshFilter.sharedMesh);
                GameObject.Destroy(meshFilter);
            }
            if (meshRenderer)
            {
                if (meshRenderer.material.mainTexture)
                {
                    GameObject.Destroy(meshRenderer.material.mainTexture);
                }
                GameObject.Destroy(meshRenderer);
            }

            GameObject.Destroy(obj);
        }
    }

    public class LodMesh
    {
        public Mesh mesh;
        public Texture2D texture;

        public bool hasMesh;
        public bool hasRequest;

        int lod;
        Action updateCallback;

        public LodMesh(int lod, Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }


        void OnMeshDataReceived(MeshData data)
        {
            mesh = data.CreateMesh();

            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            generator.RequestMeshData(mapData, lod, OnMeshDataReceived);
            
           texture = generator.RequestTextureData(mapData);
        }
    }
}
