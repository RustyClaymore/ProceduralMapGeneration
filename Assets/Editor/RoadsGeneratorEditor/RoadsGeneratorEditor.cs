using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(DrawLines))]
public class RoadsGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        DrawLines myscript = (DrawLines)target;
        if (GUILayout.Button("Generate Roads"))
        {
            //myscript.GenerateRoads();
        }
    }
}
