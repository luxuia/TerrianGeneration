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

        if ((borderInfo & 1) == 1)
        {
            for (int i = 1; i < length; i+=2)
            {
                var avg = (heightMap[i - 1, 0] + heightMap[i + 1, 0])/2;
                int idx = (i - 1) * 6 * (length - 1);
                vertices[idx].y = avg;
                vertices[idx + 3].y = avg;

                idx = i * 6 * (length - 1);
                vertices[idx+2].y = avg;
            }
        }
        if ((borderInfo & 2) == 2)
        {
            int base_idx = (length - 1) * (length - 2) * 6;
            for (int i = 1; i < length-2; i+=2 )
            {
                var avg = (heightMap[length - 1, i - 1] + heightMap[length - 1, i + 1]) / 2;

                Debug.Log(base_idx + i * 6 + 4 + ", " + vertices.Length);
                vertices[base_idx + i*6+4].y = avg;
                vertices[base_idx + i*6+6].y = avg;
                vertices[base_idx + i*6+9].y = avg;
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