using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateRoadTree))]
public class SpaceConolizationGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        GenerateRoadTree myscript = (GenerateRoadTree)target;
        if (GUILayout.Button("Generate Roads"))
        {
            myscript.Generate();
        }
    }
}
