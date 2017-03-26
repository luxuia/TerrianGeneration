using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeshGenerator {
    public static MeshData GenerateTerrianMesh(float[,] heightMap, float heightMulti, int lod, AnimationCurve curve)
    {
        int borderSize = heightMap.GetLength(0);

        int meshSimpleInc = (lod == 0) ? 1 : lod * 2;

        int meshSize = (borderSize-1) / meshSimpleInc + 1;

        var meshData = new MeshData(meshSize);


        float topLeftX = borderSize / -2;
        float topLeftZ = borderSize / 2;

        float[,] newHeightMap = new float[borderSize, borderSize];
        Buffer.BlockCopy(heightMap, 0, newHeightMap, 0, borderSize * borderSize);

        int[,] vertexIdxMap = new int[meshSize, meshSize];

        int meshIdx = 0;
        for (int y = 0; y < borderSize; y += meshSimpleInc)
        {
            for (int x = 0; x < borderSize; x += meshSimpleInc)
            {
                vertexIdxMap[y / meshSimpleInc, x / meshSimpleInc] = meshIdx++;
            }
        }

        for (int y = 0; y < borderSize; y +=meshSimpleInc)
        {
            for (int x = 0; x < borderSize; x+=meshSimpleInc)
            {
                float height = curve.Evaluate(heightMap[y, x]) * heightMulti;

                newHeightMap[y, x] = height;

                Vector3 vertexPos = new Vector3(x, height, y);
                Vector2 uv = new Vector2(x / (float)borderSize, y / (float)borderSize);
                meshData.AddVertex(vertexPos, uv, vertexIdxMap[y/meshSimpleInc, x/meshSimpleInc]);

                if (x < borderSize-meshSimpleInc && y < borderSize-meshSimpleInc)
                {
                    int xx = x / meshSimpleInc;
                    int yy = y / meshSimpleInc;
                    int a = vertexIdxMap[yy, xx];
                    int b = vertexIdxMap[yy, xx + 1];
                    int c = vertexIdxMap[yy + 1, xx];
                    int d = vertexIdxMap[yy + 1, xx + 1];

                    meshData.AddTriangle(c, b, a);
                    meshData.AddTriangle(c, d, b);
                }
            }
        }

        meshData.meshSimpleInc = meshSimpleInc;
        meshData.heightMap = newHeightMap;

        meshData.ProcessMesh();

        return meshData;
    }
}


public class MeshData
{
    int[] triangles;
    Vector3[] vertices;
    Vector2[] uvs;

    public float[,] heightMap;
    public int meshSimpleInc;

    int triangleIdx;

    public MeshData(int meshSize)
    {
        vertices = new Vector3[meshSize * meshSize];
        uvs = new Vector2[meshSize * meshSize];
        triangles = new int[(meshSize - 1) * (meshSize - 1) * 6];
    }

    public void AddVertex(Vector3 pos, Vector2 uv, int vertexIdx)
    {
        vertices[vertexIdx] = pos;
        uvs[vertexIdx] = uv;
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIdx] = a;
        triangles[triangleIdx + 1] = b;
        triangles[triangleIdx + 2] = c;

        triangleIdx += 3;
    }

    public void ProcessMesh()
    {
        Vector3[] flatVertices = new Vector3[triangles.Length];
        Vector2[] flatUVs = new Vector2[triangles.Length];

        for (int i =0; i < triangles.Length; ++i)
        {
            flatVertices[i] = vertices[triangles[i]];
            flatUVs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        uvs = flatUVs;
        vertices = flatVertices;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }


    // 1: -1, 0
    // 2: 0, 1
    // 4: 1, 0
    // 8: 0, -1
    public Mesh CreateBorderMesh(int borderInfo)
    {
        int length = heightMap.GetLength(0);
        int blockCount = (length - 1) / meshSimpleInc;

        if ((borderInfo & 1)  > 0)
        {
            for (int i = 1; i < blockCount; i+=2)
            {
                var avg = (heightMap[(i-1)*meshSimpleInc, 0] + heightMap[(i+1)*meshSimpleInc, 0])/2;
                int idx = (i - 1) * 6 * blockCount;
                vertices[idx].y = avg;
                vertices[idx + 3].y = avg;
                vertices[idx + 6*blockCount + 2].y = avg;
            }
        } else
        {
            for (int i = 1; i < blockCount; i += 2)
            {
                var avg = heightMap[i * meshSimpleInc, 0];
                int idx = (i - 1) * 6 * blockCount;
                vertices[idx].y = avg;
                vertices[idx + 3].y = avg;
                vertices[idx + 6 * blockCount + 2].y = avg;
            }
        }
        if ((borderInfo & 2) > 0)
        {
            int base_idx = blockCount* (blockCount-1) * 6;
            for (int i = 1; i < blockCount; i+=2 )
            {
                var avg = (heightMap[length - 1, (i - 1)* meshSimpleInc] + heightMap[length - 1, (i + 1)* meshSimpleInc]) / 2;

                int idx = base_idx + (i - 1) * 6;
                vertices[idx + 4].y = avg;
                vertices[idx + 6].y = avg;
                vertices[idx + 9].y = avg;
            }
        } else
        {
            int base_idx = blockCount * (blockCount - 1) * 6;
            for (int i = 1; i < blockCount; i += 2)
            {
                var avg = heightMap[length - 1, i * meshSimpleInc];

                int idx = base_idx + (i - 1) * 6;
                vertices[idx + 4].y = avg;
                vertices[idx + 6].y = avg;
                vertices[idx + 9].y = avg;
            }
        }

        if ((borderInfo & 4 ) > 0)
        {
            for (int i = 1; i < blockCount; i += 2)
            {
                var avg = (heightMap[(i - 1) * meshSimpleInc, length-1] + heightMap[(i + 1) * meshSimpleInc, length-1]) / 2;
                int idx = (i - 1) * 6 * blockCount + 6*(blockCount-1);
                vertices[idx + 4].y = avg;
                vertices[idx + 6*blockCount + 1].y = avg;
                vertices[idx + 6*blockCount + 5].y = avg;
            }
        } else
        {
            for (int i = 1; i < blockCount; i += 2)
            {
                var avg = heightMap[i * meshSimpleInc, length - 1];

                int idx = (i - 1) * 6 * blockCount + 6 * (blockCount - 1);
                vertices[idx + 4].y = avg;
                vertices[idx + 6 * blockCount + 1].y = avg;
                vertices[idx + 6 * blockCount + 5].y = avg;
            }
        }
        if ((borderInfo & 8) > 0)
        {
            for (int i = 1; i < blockCount; i += 2)
            {
                var avg = (heightMap[0, (i - 1) * meshSimpleInc] + heightMap[0, (i + 1) * meshSimpleInc]) / 2;

                int idx = (i - 1) * 6;
                vertices[idx + 1].y = avg;
                vertices[idx + 5].y = avg;
                vertices[idx + 8].y = avg;
            }
        } else
        {
            for (int i = 1; i < blockCount; i += 2)
            {
                var avg = heightMap[0, i * meshSimpleInc];

                int idx = (i - 1) * 6;
                vertices[idx + 1].y = avg;
                vertices[idx + 5].y = avg;
                vertices[idx + 8].y = avg;
            }
        }
        

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }
}