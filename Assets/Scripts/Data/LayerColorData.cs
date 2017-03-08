using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LayerColorData {
    public float height;
    public Color color;

    public LayerColorData(float h, Color c)
    {
        height = h;
        color = c;
    }

    public static LayerColorData[] DefaultColorLayers = new LayerColorData[2] { new LayerColorData( 0, Color.black), new LayerColorData( 1, Color.white ) };
}
