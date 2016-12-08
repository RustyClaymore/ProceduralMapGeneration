using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingInEditorGeneration))]
public class BuildingGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        BuildingInEditorGeneration myscript = (BuildingInEditorGeneration)target;
        if (GUILayout.Button("Generate Building"))
        {
            myscript.CreateNewBuilding();
        }
        if (GUILayout.Button("Generate In Next Row"))
        {
            myscript.GoToNextRow();
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Delete Buildings"))
        {
            myscript.DeleteBuildings();
        }
    }
}
