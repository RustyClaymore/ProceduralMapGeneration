using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadTreeGenerator))]
public class SpaceConolizationGeneratorEditor : Editor {
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Repaint();

        GUILayout.Space(20);

        RoadTreeGenerator myscript = (RoadTreeGenerator)target;
        if (GUILayout.Button("Generate Roads"))
        {
            myscript.Generate();
        }
    }

    void OnSceneGUI()
    {
        Repaint();
    }
}
