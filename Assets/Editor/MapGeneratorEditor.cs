using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor :Editor {

    public override void OnInspectorGUI()
    {
        var mapGenerator = (MapGenerator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generator"))
        {
            mapGenerator.DrawMap();
        }
    }
}
