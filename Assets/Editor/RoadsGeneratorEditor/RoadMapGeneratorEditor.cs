using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoronoiDiagramRoadsOffline))]
public class RoadMapGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(20);

        VoronoiDiagramRoadsOffline myscript = (VoronoiDiagramRoadsOffline)target;
        if (GUILayout.Button("Generate Roads"))
        {
            myscript.GenerateRoads();
        }
        if (GUILayout.Button("Reproject Roads On Terrain"))
        {
            myscript.ProjectRoadPointsOnTerrain();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("Delete Roads"))
        {
            myscript.DeleteGeneratedRoads();
        }
    }
}
