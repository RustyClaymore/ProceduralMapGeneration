/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description:
*/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ParcelGenerationController))]
public class ParcelGenerationControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ParcelGenerationController myscript = (ParcelGenerationController)target;
        if (GUILayout.Button("Generate Parcel"))
        {
            myscript.GenerateParcel();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Show Parcel"))
        {
            myscript.ShowPlot();
        }

        if(GUILayout.Button("Show Convex Hull"))
        {
            myscript.ShowConvexHull();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Parcel"))
        {
            myscript.HidePlot();
        }

        if (GUILayout.Button("Clear Convex Hull"))
        {
            myscript.HideConvexHull();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Update Parcel"))
        {
            myscript.UpdateParcel();
        }

        if (GUILayout.Button("Generate Sub Parcels"))
        {
            myscript.GenerateSubParcels();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Show OBB"))
        {
            myscript.ShowOBBRect();
        }

        if (GUILayout.Button("Hide OBB"))
        {
            myscript.HideOBBRect();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Clear Data"))
        {
            myscript.ClearData();
        }
    }
}
