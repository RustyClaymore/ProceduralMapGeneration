using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlotsGeneration))]
public class PlotsGenerationEditor : Editor {

    public override void OnInspectorGUI()
    {
        PlotsGeneration myscript = (PlotsGeneration)target;
        if (GUILayout.Button("Add Region Center"))
        {
            myscript.AddRegionPoint();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Generate Roads and Plots"))
        {
            myscript.GeneratePlots();
        }
        if (GUILayout.Button("Clear Data"))
        {
            myscript.ClearPlotsData();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset"))
		{
			myscript.ResetSettings();
		}
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (DrawDefaultInspector())
        {
            if(myscript.autoUpdate)
            {
                myscript.GeneratePlots();
            }
        }
    }
}
