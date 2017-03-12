using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EndlessTerrian : MonoBehaviour {

    public Transform Viewer;
    public int ViewDist = 1;
    public Material TerrainMaterial;

    MapGenerator generator;

    int ChunkSize = 256;

    Dictionary<ChunkIdx, TerrainChunk> visibleChunk = new Dictionary<ChunkIdx, TerrainChunk>();

    ChunkIdx currentIdx;

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

    void UpdateTerrainChunk()
    {
        for (int i = -ViewDist; i <= ViewDist; ++i)
        {
            for (int j = -ViewDist; j <= ViewDist; ++j)
            {
                TerrainChunk chunk = null;
                var chunkIdx = new ChunkIdx(currentIdx.X + i, currentIdx.Z + j);
                visibleChunk.TryGetValue(chunkIdx, out chunk);
                if (chunk == null)
                {
                    chunk = new TerrainChunk(chunkIdx, ChunkSize, TerrainMaterial);
                }

                chunk.RequestShowMesh(generator);

                visibleChunk[chunkIdx] = chunk;
            }
        }

        var outsideChunkds = visibleChunk.Keys.Where((chunkIdx) => chunkIdx.X < currentIdx.X- ViewDist 
                        || chunkIdx.X > currentIdx.X+ViewDist 
                        || chunkIdx.Z < currentIdx.Z - ViewDist 
                        || chunkIdx.Z > currentIdx.Z +ViewDist).ToList();
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

        public MeshData meshData;

        public Vector2 center;

        public GameObject obj;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public MeshCollider meshCollider;

        public TerrainChunk(ChunkIdx idx, float chunkSize, Material material)
        {
            chunkIdx = idx;

            center = new Vector2(chunkIdx.X * chunkSize, chunkIdx.Z * chunkSize);

            obj = new GameObject(chunkIdx.ToString());
            meshFilter = obj.AddComponent<MeshFilter>();
            meshRenderer = obj.AddComponent<MeshRenderer>();
            meshCollider = obj.AddComponent<MeshCollider>();

            meshRenderer.material = material;

            obj.transform.position = new Vector3(center.x, 0, center.y);
        }

        public void RequestShowMesh(MapGenerator generator)
        {
            if (meshData == null)
            {
                meshData = generator.RequestMeshData(center);

                meshFilter.sharedMesh = meshData.CreateMesh();
                meshRenderer.material.mainTexture = generator.RequestTextureData(center);
                meshCollider.sharedMesh = meshFilter.sharedMesh;
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
}
