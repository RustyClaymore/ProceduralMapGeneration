/*
* Copyright (c) Yakin Najahi
* Twitter: https://twitter.com/nightkenny
* 
* Description:
*/

using UnityEngine;
using UnityEditor;

public class RoadsGeneratorWindowEditor : EditorWindow {

    private RoadTreeGenerator roadTreeGen;

    [MenuItem("City Generator/Roads and Parcels Generator")]
    static void Init()
    {
        RoadsGeneratorWindowEditor window = (RoadsGeneratorWindowEditor)EditorWindow.GetWindow(typeof(RoadsGeneratorWindowEditor));
    }

    void OnGUI()
    {

    }
}
