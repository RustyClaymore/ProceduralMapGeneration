using UnityEngine;
using System;
using TBUnityLib.Generic;
using TBUnityLib.MeshTools;
using BezierSplines;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ************************************************************************
// Copyright (C) Torchbearer Interactive, Ltd. - All Rights Reserved
//
// Unauthorized copying of this file, via any medium is strictly prohibited
// proprietary and confidential
// 
// Written by: Jonathan H Langley - jon@tbinteractive.co.uk, 2016
// ************************************************************************

// This class handles the specic mesh generation and information neccecary to 
// create and retrieve information from spline roads

/// <summary>
/// MagnetRoads v1.0.0
/// </summary>
namespace MagnetRoads
{
    /// <summary>
    /// MagnetRoads v1.0.0
    /// </summary>
    [ExecuteInEditMode] [AddComponentMenu("")]
    [RequireComponent(typeof(MeshFilter))] [RequireComponent(typeof(MeshRenderer))] [RequireComponent(typeof(BezierSpline))] [RequireComponent(typeof(MeshCollider))]
    public class MagnetRoad : MonoBehaviour
    {
        #region MAGNET_ROAD_DATA
        // Road Data Variables
        /// <summary>
        /// This road's BezierSpline source
        /// </summary>
        [HideInInspector]
        public BezierSpline splineSource;

        /// <summary>
        /// The Material to apply to the road surface
        /// </summary>
        [Tooltip("Road Material")]
        public Material surfaceMaterial;

        /// <summary>
        /// The Material to apply to the road's sides
        /// </summary>
        [Tooltip("Roadside Material")]
        public Material sideMaterial;

        /// <summary>
        /// The width of this MagnetRoad
        /// </summary>
        [Tooltip("Road width value")]
        public float roadWidth;

        /// <summary>
        /// The distance from the bottom of the side ramp to the road side
        /// </summary>
        [Tooltip("The distance from the bottom of the side ramp to the road side")]
        public float slopeWidth;

        /// <summary>
        /// Depth of the road's sides
        /// </summary>
        [Tooltip("Depth of the road's sides")]
        public float sideDepth;

        /// <summary>
        /// The number of steps in the curve (mesh resolution)
        /// </summary>
        [Tooltip("Steps per curve")]
        public int stepsPerCurve;

        /// <summary>
        /// Show road outline (flag)
        /// </summary>
        [Tooltip("Show road outline")]
        public bool showRoadOutline;

        /// <summary>
        /// Show car routes (flag)
        /// </summary>
        [Tooltip("Show car routes")]
        public bool showCarRoutes;

        /// <summary>
        /// Use custom car (flag)
        /// </summary>
        [HideInInspector]
        public bool customCar;

        /// <summary>
        /// Custom car GameObject source
        /// </summary>
        [HideInInspector]
        public GameObject carObject;

        // Public accessors for the SnapNode positions
        /// <summary>
        /// Returns the left SnapPoints's Transform
        /// </summary>
        public Transform SnapNodeLeft { get { return transform.FindChild("SnapNodeLeft"); } }

        /// <summary>
        /// Returns the right SnapPoint's Transform
        /// </summary>
        public Transform SnapNodeRight { get { return transform.FindChild("SnapNodeRight"); } }
        #endregion


        #region ROAD_GENERATION_DATA
        // Procedural Generation Variables
        /// <summary>
        /// Left road side GameObject (child)
        /// </summary>
        [HideInInspector]
        public GameObject leftSide;

        /// <summary>
        /// Right road side GameObject (child)
        /// </summary>
        [HideInInspector]
        public GameObject rightSide;

        /// <summary>
        /// Road underside GameObject (child)
        /// </summary>
        [HideInInspector]
        public GameObject underSide;

        /// <summary>
        /// Cached roadside Material
        /// </summary>
        private Material _cachedSideMaterial;

        /// <summary>
        /// Generated Mesh
        /// </summary>
        private Mesh _mesh;

        /// <summary>
        /// Generated mesh's MeshFilter
        /// </summary>
        private MeshFilter _meshFilter;

        /// <summary>
        /// Generated mesh's MeshCollider
        /// </summary>
        private MeshCollider _meshCollider;
        #endregion


        #region INITIALIZATION
        // Constructor - set default values
        /// <summary>
        /// MagnetRoad's constructor method
        /// </summary>
        public MagnetRoad()
        {
            showRoadOutline = true;
            showCarRoutes = true;
            sideDepth = 0.2f;
            slopeWidth = 0.0f;
            roadWidth = 0.5f;
            stepsPerCurve = 20;
            customCar = false;
            carObject = null;
        }
        #endregion


        #region UNITY_CALLBACKS
        // Prepare and arrange (within inspector) component dependencies
        /// <summary>
        /// Initialize the MagnetRoad on awake
        /// </summary>
        private void Awake()
        {
            if (splineSource == null)
            {
                try
                {
                    splineSource = GetComponent<BezierSpline>();
                }
                catch (NullReferenceException)
                {
                    Debug.LogWarning("Spline Road missing Bezier Spline! Component added automatically.");
                    splineSource = gameObject.AddComponent<BezierSpline>();
                }
                // Perform some inspector formatting
				#if UNITY_EDITOR
                for (int i = 0; i < 10; i++) UnityEditorInternal.ComponentUtility.MoveComponentDown(this);
                for (int i = 0; i < 10; i++) UnityEditorInternal.ComponentUtility.MoveComponentDown(splineSource);
				#endif
            }
        }

        // Draw gizmos and bound required data values in-editor
        /// <summary>
        /// Draw call for in-editor graphics
        /// </summary>
        private void OnDrawGizmos()
        {
			#if UNITY_EDITOR
            Tools.hidden = false;
			#endif
            if (roadWidth <= 0) roadWidth = 0.01f; // constrain lower bound
            if (stepsPerCurve <= 0) stepsPerCurve = 1; // constrain lower bound
            if (showCarRoutes) DrawCarPath(GenerateCarPathVectors(splineSource, stepsPerCurve, roadWidth));
            if (showRoadOutline)
            {
                DrawRoadOutline(GenerateRoadVertexOutput(splineSource, stepsPerCurve, roadWidth));
                DrawRoadOutline(GenerateLeftRoadSideVectors(GenerateRoadVertexOutput(splineSource, stepsPerCurve, roadWidth)));
                DrawRoadOutline(GenerateRightRoadSideVectors(GenerateRoadVertexOutput(splineSource, stepsPerCurve, roadWidth)));
            }
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        #endregion


        #region ROAD_GENERATION
        // Mesh transform enum
        /// <summary>
        /// Transform offset types
        /// </summary>
        private enum OffsetTransform
        {
            None,
            Positive,
            Negative
        }

        // Output the required data to generate the road - mesh generation only
        /// <summary>
        /// Generate road mesh data from BezierSpline
        /// </summary>
        /// <param name="spline">BezierSpline source</param>
        /// <param name="stepsPerCurve">Steps per curve (mesh resolution)</param>
        /// <param name="roadWidth">Road mesh width</param>
        /// <returns>An array of vector pairs describing the MagnetRoad's surface</returns>
        public Pair<Vector3>[] GenerateRoadVertexOutput(BezierSpline spline, int stepsPerCurve, float roadWidth)
        {
            Pair<Vector3>[] vertexOutput = new Pair<Vector3>[(stepsPerCurve * spline.CurveCount) + 1];
            int index = 0;
            float roadOffset = roadWidth / 2;
            Vector3 point = spline.GetPoint(0f);
            Pair<Vector3> current = new Pair<Vector3>();
            Quaternion offsetRotation = Quaternion.Euler(0, 90, 0); // 90 degrees
            var vaTemp = point + (spline.GetOffsetRotation(0f, offsetRotation) * roadOffset);
            current.First = new Vector3(vaTemp.x, point.y, vaTemp.z);
            var vbTemp = point + (spline.GetOffsetRotation(0f, offsetRotation) * -roadOffset);
            current.Second = new Vector3(vbTemp.x, point.y, vbTemp.z);
            vertexOutput[index] = current;
            int steps = stepsPerCurve * spline.CurveCount;
            for (int i = 0; i <= steps; i++, index++)
            {
                point = spline.GetPoint(i / (float)steps);
                vaTemp = point + (spline.GetOffsetRotation(i / (float)steps, offsetRotation) * roadOffset);
                current.First = new Vector3(vaTemp.x, point.y, vaTemp.z);
                vbTemp = point + (spline.GetOffsetRotation(i / (float)steps, offsetRotation) * -roadOffset);
                current.Second = new Vector3(vbTemp.x, point.y, vbTemp.z);
                vertexOutput[index] = current;
            }
            return vertexOutput;
        }

        // Generate car path routes - mesh generation only
        /// <summary>
        /// Generate a vector array of car paths (1/2 road width)
        /// </summary>
        /// <param name="spline">BezierSpline source</param>
        /// <param name="stepsPerCurve">Steps per curve (mesh resolution)</param>
        /// <param name="roadWidth">Road mesh width</param>
        /// <returns>An array of vector pairs describing the road's lanes</returns>
        public Pair<Vector3>[] GenerateCarPathVectors(BezierSpline spline, int stepsPerCurve, float roadWidth)
        {
            return GenerateRoadVertexOutput(spline, stepsPerCurve, roadWidth / 2);
        }

        // Output left road side vertex data - mesh generation only
        /// <summary>
        /// Output data for the left roadside mesh
        /// </summary>
        /// <param name="vertexData">Road vertex data</param>
        /// <returns>An array of vector pairs describing the right roadside</returns>
        public Pair<Vector3>[] GenerateLeftRoadSideVectors(Pair<Vector3>[] vertexData)
        {
            Pair<Vector3>[] leftRoadSide = vertexData;
            Pair<Vector3>[] leftRoadSideSlope = GenerateRoadVertexOutput(splineSource, stepsPerCurve, roadWidth + slopeWidth);
            for (int i = 0; i < leftRoadSide.Length; i++)
            {
                leftRoadSide[i].Second = new Vector3(leftRoadSideSlope[i].First.x, leftRoadSideSlope[i].First.y - sideDepth, leftRoadSideSlope[i].First.z);
            }
            return leftRoadSide;
        }

        // Output left road side vertex data - mesh generation only
        /// <summary>
        /// Output data for the right roadside mesh
        /// </summary>
        /// <param name="vertexData">Road vertex data</param>
        /// <returns>An array of vector pairs describing the right roadside</returns>
        public Pair<Vector3>[] GenerateRightRoadSideVectors(Pair<Vector3>[] vertexData)
        {
            Pair<Vector3>[] rightRoadSide = vertexData;
            Pair<Vector3>[] rightRoadSideSlope = GenerateRoadVertexOutput(splineSource, stepsPerCurve, roadWidth + slopeWidth);
            for (int i = 0; i < rightRoadSide.Length; i++)
            {
                rightRoadSide[i].First = new Vector3(rightRoadSideSlope[i].Second.x, rightRoadSideSlope[i].Second.y - sideDepth, rightRoadSideSlope[i].Second.z);
            }
            return rightRoadSide;
        }

        // Generate mesh from generated vertex data - mesh generation only
        /// <summary>
        /// Generate the road mesh and add it to this MagnetRoad
        /// </summary>
        /// <param name="vertexData">Road vertex data</param>
        public void GenerateRoadMesh(Pair<Vector3>[] vertexData)
        {
            // Set-up mesh components
            try
            {
                _meshFilter = GetComponent<MeshFilter>();
            }
            catch (NullReferenceException)
            {
                _meshFilter = gameObject.AddComponent<MeshFilter>();
            }
            try
            {
                GetComponent<MeshRenderer>();
            }
            catch (NullReferenceException)
            {
                gameObject.AddComponent<MeshRenderer>();
            }
            try
            {
                _meshCollider = GetComponent<MeshCollider>();
            }
            catch (NullReferenceException)
            {
                _meshCollider = gameObject.AddComponent<MeshCollider>();
            }

            // Generate road mesh
            _mesh = new Mesh();
            _meshFilter.mesh = Geometry.GenerateStrip(vertexData, transform, true, false, "ProceduralRoad");
            _meshCollider.sharedMesh = _meshFilter.sharedMesh;
            if (surfaceMaterial) gameObject.GetComponent<Renderer>().sharedMaterial = surfaceMaterial;

            // Generate helper points 
            GenerateSnapPoints(splineSource, stepsPerCurve);
        }

        // Generate side meshes for the road - mesh generation only
        /// <summary>
        /// Generate the road side meshes and add them to this MagnetRoad
        /// </summary>
        /// <param name="leftSideVectors">Left roadside mesh data</param>
        /// <param name="rightSideVectors">Right roadside mesh data</param>
        public void GenerateSideMeshes(Pair<Vector3>[] leftSideVectors, Pair<Vector3>[] rightSideVectors)
        {
            // Clear existing child mesh game objects from the road and generate new ones
            if (transform.FindChild("Road Side One"))
            {
                _cachedSideMaterial = transform.FindChild("Road Side One").gameObject.GetComponent<Renderer>().sharedMaterial; // store the material for later use
                DestroyImmediate(transform.FindChild("Road Side One").gameObject);
            }
            if (transform.FindChild("Road Side Two")) DestroyImmediate(transform.FindChild("Road Side Two").gameObject);
            if (transform.FindChild("Road Underside")) DestroyImmediate(transform.FindChild("Road Underside").gameObject);
            rightSide = new GameObject("Road Side One");
            rightSide.transform.parent = gameObject.transform;
            rightSide.hideFlags = HideFlags.HideInHierarchy;
            leftSide = new GameObject("Road Side Two");
            leftSide.transform.parent = gameObject.transform;
            leftSide.hideFlags = HideFlags.HideInHierarchy;
            if (sideDepth > 0)
            {
                underSide = new GameObject("Road Underside");
                underSide.transform.parent = gameObject.transform;
                underSide.hideFlags = HideFlags.HideInHierarchy;
            }

            // Generate RoadSideOne first.
            // Create RoadSideOne's mesh and mesh filter components
            MeshFilter rsOneMf = rightSide.AddComponent<MeshFilter>();
            rightSide.AddComponent<MeshRenderer>();
            rsOneMf.mesh = Geometry.GenerateStrip(rightSideVectors, transform, false, null, "RoadSideOne");
            rightSide.AddComponent<MeshCollider>().sharedMesh = rsOneMf.sharedMesh;
            if (!sideMaterial) rightSide.GetComponent<Renderer>().sharedMaterial = _cachedSideMaterial;
            else rightSide.GetComponent<Renderer>().sharedMaterial = sideMaterial;

            // Do the same for RoadSideTwo
            MeshFilter rsTwoMf = leftSide.AddComponent<MeshFilter>();
            leftSide.AddComponent<MeshRenderer>();
            rsTwoMf.mesh = Geometry.GenerateStrip(leftSideVectors, transform, false, null, "RoadSideTwo");
            leftSide.AddComponent<MeshCollider>().sharedMesh = rsTwoMf.sharedMesh;
            if (!sideMaterial) leftSide.GetComponent<Renderer>().sharedMaterial = _cachedSideMaterial;
            else leftSide.GetComponent<Renderer>().sharedMaterial = sideMaterial;

            // Check the underside is actually below the road
            if (sideDepth > 0)
            {
                // Pull the bottom vertexes out of the left and right side vectors
                Pair<Vector3>[] underSideVectors = new Pair<Vector3>[leftSideVectors.Length];
                for (int i = 0; i < leftSideVectors.Length; i++)
                {
                    underSideVectors[i].First = leftSideVectors[i].Second;
                    underSideVectors[i].Second = rightSideVectors[i].First;
                }

                // Create the components for the underside of the road
                MeshFilter rsUnderMf = underSide.AddComponent<MeshFilter>();
                underSide.AddComponent<MeshRenderer>();
                rsUnderMf.mesh = Geometry.GenerateStrip(underSideVectors, transform, false, null, "RoadUnderside");
                underSide.AddComponent<MeshCollider>().sharedMesh = rsUnderMf.sharedMesh;
                if (!sideMaterial) underSide.GetComponent<Renderer>().sharedMaterial = _cachedSideMaterial;
                else underSide.GetComponent<Renderer>().sharedMaterial = sideMaterial;
            }
        }

        // SnapPoint generation function
        /// <summary>
        /// Generate the MagnetRoad's SnapPoints
        /// </summary>
        /// <param name="spline">BezierSpline source</param>
        /// <param name="stepsPerCurve">Steps per curve (mesh resolution)</param>
        private void GenerateSnapPoints(BezierSpline spline, int stepsPerCurve)
        {
            try
            {
                if (transform.FindChild("SnapNodeLeft") || transform.FindChild("SnapNodeRight"))
                {
                    if (transform.FindChild("SnapNodeLeft")) DestroyImmediate(transform.FindChild("SnapNodeLeft").gameObject); // destroy the old road pieces
                    if (transform.FindChild("SnapNodeRight")) DestroyImmediate(transform.FindChild("SnapNodeRight").gameObject);
                }
            }
            catch (Exception) { }
            Vector3 posLeft = spline.GetPoint(stepsPerCurve * spline.CurveCount);
            Vector3 posRight = spline.GetPoint(0f);
            GameObject snapPosLeft = new GameObject("SnapNodeLeft");
            GameObject snapPosRight = new GameObject("SnapNodeRight");
            snapPosLeft.transform.parent = gameObject.transform;
            snapPosLeft.AddComponent<SnapPoint>().SetUp(SnapPoint.PointEnd.Negative, roadWidth);
            snapPosLeft.transform.position = posLeft;
            snapPosRight.transform.parent = gameObject.transform;
            snapPosRight.AddComponent<SnapPoint>().SetUp(SnapPoint.PointEnd.Positive, roadWidth);
            snapPosRight.transform.position = posRight;
        }

        // Clear the current road mesh
        /// <summary>
        /// Clear the mesh data for this MagnetRoad
        /// </summary>
        public void ClearRoadMesh()
        {
            try
            {
                _mesh.Clear();
                _meshFilter.sharedMesh.Clear();
                _meshCollider.sharedMesh.Clear();
                if (leftSide) DestroyImmediate(leftSide);
                if (rightSide) DestroyImmediate(rightSide);
                if (underSide) DestroyImmediate(underSide);
            }
            catch (Exception e)
            {
                Debug.LogWarning("MESH FAILED TO CLEAR: " + e);
            }
        }
        #endregion


        #region DEBUG
        // Draw the road into the scene view using Gizmos
        /// <summary>
        /// Draw the MagnetRoad's mesh outline in-editor
        /// </summary>
        /// <param name="vertexData">Road vertex data</param>
        private void DrawRoadOutline(Pair<Vector3>[] vertexData)
        {
            Gizmos.color = new Color(1, 0.5f, 0.0f);
            Pair<Vector3> last, current;
            current = vertexData[0];
            Gizmos.DrawLine(current.First, current.Second);
            last = current;
            for (int i = 1; i <= vertexData.Length - 1; i++)
            {
                current = vertexData[i];
                Gizmos.DrawLine(current.First, current.Second);
                Gizmos.DrawLine(current.First, last.First);
                Gizmos.DrawLine(current.Second, last.Second);
                last = current;
            }
        }

        // Draw the car path routes onto the road
        /// <summary>
        /// Draw the MagnetRoad's lanes in-editor
        /// </summary>
        /// <param name="pathData">Road's lane data</param>
        private void DrawCarPath(Pair<Vector3>[] pathData)
        {
            Gizmos.color = Color.blue;
            Pair<Vector3> last, current;
            last = pathData[0];
            for (int i = 1; i <= pathData.Length - 1; i++)
            {
                current = pathData[i];
                Gizmos.DrawLine(current.First, last.First);
                Gizmos.DrawLine(current.Second, last.Second);
                last = current;
            }
        }
        #endregion


        #region META_GETTERS
        /// <summary>
        /// Return the central car path on this road
        /// </summary>
        /// <returns>An array of points following the center of the road</returns>
        public Vector3[] GetMiddleCarPath()
        {
            Pair<Vector3>[] carPathVectors = GenerateCarPathVectors(splineSource, stepsPerCurve, 0);
            Vector3[] middleCarPath = new Vector3[carPathVectors.Length];
            for (int i = 0; i <= carPathVectors.Length - 1; i++)
            {
                middleCarPath[i] = carPathVectors[i].First;
            }
            return middleCarPath;
        }

        /// <summary>
        /// Return the left car path on this road
        /// </summary>
        /// <returns>An array of points following the left side of the road</returns>
        public Vector3[] GetLeftCarPath()
        {
            Pair<Vector3>[] carPathVectors = GenerateCarPathVectors(splineSource, stepsPerCurve, roadWidth);
            Vector3[] leftCarPath = new Vector3[carPathVectors.Length];
            for (int i = 0; i <= carPathVectors.Length - 1; i++)
            {
                leftCarPath[i] = carPathVectors[i].Second;
            }
            return leftCarPath;
        }

        /// <summary>
        /// Return the right car path on this road (inverted)
        /// </summary>
        /// <returns>An array of points following the right side of the road (inverted)</returns>
        public Vector3[] GetRightCarPath()
        {
            Pair<Vector3>[] carPathVectors = GenerateCarPathVectors(splineSource, stepsPerCurve, roadWidth);
            Vector3[] rightCarPath = new Vector3[carPathVectors.Length];
            for (int i = 0; i <= carPathVectors.Length - 1; i++)
            {
                rightCarPath[i] = carPathVectors[rightCarPath.Length - 1 - i].First;
            }
            return rightCarPath;
        }

        /// <summary>
        /// Return the closest SnapPoint on this road to a world coordinate
        /// </summary>
        /// <param name="vector">World coordinate</param>
        /// <returns>The closest SnapPoint</returns>
        public SnapPoint GetClosestSnapPointFromVector(Vector3 vector)
        {
            try
            {
                float distLeft = Vector3.Distance(vector, SnapNodeLeft.position);
                float distRight = Vector3.Distance(vector, SnapNodeRight.position);
                if (distLeft > distRight) return SnapNodeRight.gameObject.GetComponent<SnapPoint>();
                else if (distRight >= distLeft) return SnapNodeLeft.gameObject.GetComponent<SnapPoint>();
            }
            catch (Exception) { }
            return null;
        }
        #endregion
    }


	#if UNITY_EDITOR
    // This class handles the editable attributes of the selected MagnetRoad
    // exposing them to the inspector

    /// <summary>
    /// MagnetRoad's custom inspector UI
    /// </summary>
    [CustomEditor(typeof(MagnetRoad))]
    public class SplineRoadEditorInspector : Editor
    {
        #region MAGNET_ROAD_DATA
        // reference to road object
        /// <summary>
        /// Reference to this MangetRoad
        /// </summary>
        private MagnetRoad _road;

        /// <summary>
        /// Reference to the Torchbearer Logo
        /// </summary>
        private Texture _tbLogo;
        #endregion


        #region UNITY_CALLBACKS
        /// <summary>
        /// OnEnable load the inspector logo
        /// </summary>
        private void OnEnable()
        {
            _tbLogo = (Texture)Resources.Load("TBLogo", typeof(Texture));
        }

        // Disable rotation on the spline road's transform
        /// <summary>
        /// Handle the Transform limitations in-editor
        /// </summary>
        public void OnSceneGUI()
        {
            // Hide the rotation controls on MagnetRoads
            if (_road = target as MagnetRoad)
            {
                if (Tools.current == Tool.Rotate) Tools.hidden = true;
                else Tools.hidden = false;
            }
            else Tools.hidden = false;
        }

        // Custom inspector controls
        /// <summary>
        /// Render the MagnetRoad's custom inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Road editing
            _road = target as MagnetRoad;
            GUILayout.Label(_tbLogo, GUILayout.Width(EditorGUIUtility.currentViewWidth - 40.0f), GUILayout.Height(60.0f));
            GUILayout.Label("Road Editor:", EditorStyles.boldLabel);
            DrawDefaultInspector();
            var oldColor = GUI.color;
            GUI.color = new Color(1, 0.5f, 0.0f);
            if (GUILayout.Button("Generate Road Mesh"))
            {
                Undo.RecordObject(_road, "Generate Road Mesh");
                EditorUtility.SetDirty(_road);
                _road.GenerateRoadMesh(_road.GenerateRoadVertexOutput(_road.splineSource, _road.stepsPerCurve, _road.roadWidth));
                _road.GenerateSideMeshes(
                    _road.GenerateLeftRoadSideVectors(_road.GenerateRoadVertexOutput(_road.splineSource, _road.stepsPerCurve, _road.roadWidth)),
                    _road.GenerateRightRoadSideVectors(_road.GenerateRoadVertexOutput(_road.splineSource, _road.stepsPerCurve, _road.roadWidth)));
            }
            GUI.color = oldColor;
            if (GUILayout.Button("Clear Road Mesh"))
            {
                Undo.RecordObject(_road, "Clear Road Mesh");
                EditorUtility.SetDirty(_road);
                _road.ClearRoadMesh();
            }

            // Road testing buttons
            GUILayout.Label("Car Test:", EditorStyles.boldLabel);
            bool custom = EditorGUILayout.Toggle("Use Custom Car", _road.customCar);
            _road.customCar = custom;
            if (_road.customCar)
            {
                var car = EditorGUILayout.ObjectField("Car Game Object", _road.carObject, typeof(GameObject), true);
                _road.carObject = (GameObject)car;
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Left Lane", EditorStyles.miniButtonLeft))
            {
                if (_road.customCar && _road.carObject != null)
                {
                    DebugSpawnFollower(_road.carObject, _road.GetLeftCarPath()); // spawn custom car
                }
                else
                {
                    DebugSpawnFollower(_road.GetLeftCarPath()); // spawn cube follower
                }
            }
            if (GUILayout.Button("Central", EditorStyles.miniButtonMid))
            {
                if (_road.customCar && _road.carObject != null)
                {
                    DebugSpawnFollower(_road.carObject, _road.GetMiddleCarPath()); // spawn custom car
                }
                else
                {
                    DebugSpawnFollower(_road.GetMiddleCarPath()); // spawn cube follower
                }
            }
            if (GUILayout.Button("Right Lane", EditorStyles.miniButtonRight))
            {
                if (_road.customCar && _road.carObject != null)
                {
                    DebugSpawnFollower(_road.carObject, _road.GetRightCarPath()); // spawn custom car
                }
                else
                {
                    DebugSpawnFollower(_road.GetRightCarPath()); // spawn cube follower
                }
            }
            GUILayout.EndHorizontal();

            // Curve editing
            GUILayout.Label("Add/Remove Curve:", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Curve"))
            {
                Undo.RecordObject(_road.splineSource, "Add Curve");
                _road.splineSource.AddCurve(_road.stepsPerCurve);
                EditorUtility.SetDirty(_road.splineSource);
            }
            if (GUILayout.Button("Remove Curve"))
            {
                Undo.RecordObject(_road.splineSource, "Remove Curve");
                _road.splineSource.RemoveCurve();
                EditorUtility.SetDirty(_road.splineSource);
            }
        }
        #endregion


        #region DEBUG
        /// <summary>
        /// Spawns a Road Follower
        /// </summary>
        /// <param name="source">Custom Road Follower</param>
        /// <param name="roadPath">Path to follow</param>
        /// <returns>The new Road Follower</returns>
        private GameObject DebugSpawnFollower(GameObject source, Vector3[] roadPath)
        {
            GameObject temp = Instantiate(source);
            temp.AddComponent<RoadFollower>();
            temp.GetComponent<RoadFollower>().SetupRoadFollower(roadPath, 10000);
            return temp;
        }

        // Spawn a follower object
        /// <summary>
        /// Spawns a Road Follower
        /// </summary>
        /// <param name="roadPath">Path to follow</param>
        /// <returns>The new Road Follower</returns>
        private GameObject DebugSpawnFollower(Vector3[] roadPath)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            temp.transform.localScale = new Vector3(.2f, .2f, .2f);
            temp.AddComponent<RoadFollower>();
            temp.GetComponent<RoadFollower>().SetupRoadFollower(roadPath, 10000);
            return temp;
        }
        #endregion
    }


    // This class implements a toolbar menu into the unity editor allowing the
    // user to spawn new instances of MagnetRoads into the scene

    /// <summary>
    /// MangetRoad's Torchbearer toolbar integration
    /// </summary>
    [ExecuteInEditMode]
    public static class SplineRoadSpawner
    {
        #region TOOLBAR_FUNCTIONS
        /// <summary>
        /// Toolbar method to create a new MagnetRoad in the scene
        /// </summary>
        /// <returns>The generated MagnetRoad's GameObject</returns>
        [MenuItem("Torchbearer Interactive/Magnet Roads/New Spline Road")]
        private static GameObject CreateNewSplineRoad()
        {
            GameObject newOne = new GameObject();
            newOne.name = "Spline Road";
            newOne.AddComponent<MagnetRoad>();
            return newOne;
        }
        #endregion
    }
	#endif
}