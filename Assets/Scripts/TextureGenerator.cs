using UnityEngine;
using System.Collections;

public static class TextureGenerator {

	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
		Texture2D texture = new Texture2D (width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels (colourMap);
		texture.Apply ();
		return texture;
	}


	public static Texture2D TextureFromHeightMap(float[,] heightMap, LayerColorData[] layers) {
		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);

        //if (layers == null) layers = LayerColorData.DefaultColorLayers;

		Color[] colorMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {

                if (layers == null || layers.Length < 2)
                {
                    colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[y, x]);
                } else
                {
                    var h = heightMap[y, x];
                    for (int i = 0; i < layers.Length; i++)
                    {
                        if (h >=layers[i].height)
                        {
                            colorMap[y * width + x] = layers[i].color;
                        }
                    }
                }
			}
		}

		return TextureFromColourMap (colorMap, width, height);
	}

}
