using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        for (int i = -ViewDist; i <= 0; ++i)
        {
            for (int j = -ViewDist; j <= 0; ++j)
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

        public TerrainChunk(ChunkIdx idx, float chunkSize, Material material)
        {
            chunkIdx = idx;

            center = new Vector2(chunkIdx.X * chunkSize, chunkIdx.Z * chunkSize);

            obj = new GameObject(chunkIdx.ToString());
            meshFilter = obj.AddComponent<MeshFilter>();
            meshRenderer = obj.AddComponent<MeshRenderer>();
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
            }
        }
    }
}
