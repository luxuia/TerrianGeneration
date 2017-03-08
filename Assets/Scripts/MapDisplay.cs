using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;

	public void DrawTexture(Texture2D texture) {

        Mesh mesh = new Mesh();

        float width = texture.width/2;
        float height = texture.height/2;
        Vector3[] vertices = new Vector3[4] {
            new Vector3(-width, 0, height), new Vector3(width, 0, height),
            new Vector3(-width, 0, -height), new Vector3(width, 0, -height)
        };
        Vector2[] uvs = new Vector2[4] {
             new Vector3(0, 0), new Vector3(1, 0),
            new Vector3(0, 1), new Vector3(1, 1)
        };
        int[] indices = new int[6]
        {
            0, 1, 2, 1, 3, 2
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;

        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = Vector3.one;
	}

    public void DrawMesh(MeshData meshdata)
    {
        var mesh = meshdata.CreateMesh();
        meshFilter.sharedMesh = mesh;
        meshFilter.transform.localScale = Vector3.one;
    }

}
