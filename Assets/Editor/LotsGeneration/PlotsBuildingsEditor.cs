using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LotBuildingGenerator))]
public class PlotsBuildingsEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        LotBuildingGenerator myscript = (LotBuildingGenerator)target;
        if (GUILayout.Button("Generate Buildings"))
        {
            myscript.GenerateBuildings();
        }

		if(GUILayout.Button("Clear Buildings"))
		{
			myscript.DeleteBuildings();
		}
    }
}
