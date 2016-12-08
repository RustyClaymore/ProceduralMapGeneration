using UnityEngine;
using System;
using TBUnityLib.MeshTools;

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

// This class handles the mesh and information generation required to generate
// snappable intersections that link to SplineRoads

/// <summary>
/// MagnetRoads v1.0.0
/// </summary>
namespace MagnetRoads
{
    /// <summary>
    /// MagnetRoads v1.0.0
    /// </summary>
    [ExecuteInEditMode][AddComponentMenu("")]
    [RequireComponent(typeof(MeshFilter))] [RequireComponent(typeof(MeshRenderer))] [RequireComponent(typeof(MeshCollider))]
    public class Intersection : MonoBehaviour
    {
        #region INTERSECTION_DATA
        // Intersection Data Variables
        /// <summary>
        /// The Material to apply to the intersection surface
        /// </summary>
        [Tooltip("Road Material")]
        public Material surfaceMaterial;

        /// <summary>
        /// The Material to apply to the intersection's sides
        /// </summary>
        [Tooltip("Roadside Material")]
        public Material sideMaterial;

        /// <summary>
        /// The width of this Intersection
        /// </summary>
        [Tooltip("Road width value")]
        public float roadWidth;

        /// <summary>
        /// Depth of the road's sides
        /// </summary>
        [Tooltip("Depth of the road's sides")]
        public float sideDepth;

        // Public access to all SnapPoint positions on this intersection
        /// <summary>
        /// Returns an array of all attached SnapPoints
        /// </summary>
        public SnapPoint[] SnapNodes { get { return gameObject.GetComponentsInChildren<SnapPoint>(); } }

        /// <summary>
        /// Returns an array of all attached StartPoints
        /// </summary>
        public StartPoint[] StartPoints { get { return gameObject.GetComponentsInChildren<StartPoint>(); } }
        #endregion


        #region INTERSECTION_GENERATION_DATA
        // Mesh information for procedural generation
        /// <summary>
        /// The type of intersection to generate (number of lanes)
        /// </summary>
        [SerializeField]
        [HideInInspector]
        private IntersectionType _intersectionType;

        /// <summary>
        /// The cached roadside Material
        /// </summary>
        private Material _cachedSideMaterial;

        /// <summary>
        /// The cached rotation of this object
        /// </summary>
        private Quaternion _cachedRotation;

        /// <summary>
        /// Generated mesh
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

        /// <summary>
        /// Parent of the attached SnapPoints
        /// </summary>
        private GameObject _snapNodeParent;
        #endregion


        #region INITIALIZATION
        // Set up method
        /// <summary>
        /// Intersection initialization method
        /// </summary>
        /// <param name="type">Intersection type</param>
        public void SetUp(IntersectionType type)
        {
            _intersectionType = type;
            roadWidth = 0.5f;
            sideDepth = 0.2f;
            GenerateIntersectionMesh();
        }
        #endregion


        #region UNITY_CALLBACKS
        // Constrain the Intersection data values every frame
        /// <summary>
        /// Draw call for in-editor graphics
        /// </summary>
        public void OnDrawGizmos()
        {
            if (roadWidth < 0) roadWidth = 0.01f; // constrain road width
        }
        #endregion


        #region INTERSECTION_GENERATION
        // Generate the intersection mesh
        /// <summary>
        /// Generate this Intersection's mesh and assign it
        /// </summary>
        public void GenerateIntersectionMesh()
        {
            // Store roadSide texture
            _cachedRotation = transform.rotation;
            if (transform.FindChild("Intersection Sides"))
                _cachedSideMaterial = transform.FindChild("Intersection Sides").gameObject.GetComponent<Renderer>().sharedMaterial;

            // Refresh object information
            foreach (SnapPoint node in SnapNodes)
            {
                DestroyImmediate(node.gameObject);
            }
            foreach (StartPoint point in StartPoints)
            {
                DestroyImmediate(point.gameObject);
            }
            if (_snapNodeParent) DestroyImmediate(_snapNodeParent);
            if (transform.FindChild("Intersection Underside")) DestroyImmediate(transform.FindChild("Intersection Underside").gameObject);
            if (transform.FindChild("Intersection Sides")) DestroyImmediate(transform.FindChild("Intersection Sides").gameObject);
            transform.rotation = Quaternion.Euler(0, 0, 0); // reset any rotation

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

            // Create the SnapPoint parent object
            _snapNodeParent = new GameObject();
            _snapNodeParent.transform.position = transform.position;
            _snapNodeParent.transform.parent = transform;
            _snapNodeParent.hideFlags = HideFlags.HideInHierarchy;

            // Generate road mesh
            _mesh = Geometry.GeneratePlaneMesh(roadWidth, roadWidth);
            _mesh.name = "Procedural Intersection";
            _meshFilter.mesh = _mesh;
            _meshCollider.sharedMesh = _mesh;
            if (surfaceMaterial) GetComponent<Renderer>().sharedMaterial = surfaceMaterial;

            // Generate side mesh & game object
            GameObject sides = new GameObject();
            Mesh sideMesh = Geometry.GeneratePlaneEdge(roadWidth, roadWidth, sideDepth);
            sides.AddComponent<MeshFilter>().mesh = sideMesh;
            sides.AddComponent<MeshRenderer>();
            sides.AddComponent<MeshCollider>().sharedMesh = sideMesh;
            if (!sideMaterial) sides.GetComponent<Renderer>().sharedMaterial = _cachedSideMaterial;
            else sides.GetComponent<Renderer>().sharedMaterial = sideMaterial;
            sides.transform.position = transform.position;
            sides.transform.SetParent(transform);
            sides.gameObject.hideFlags = HideFlags.HideInHierarchy;
            sides.name = "Intersection Sides";

            // Generate underside mesh
            GameObject underSide = new GameObject();
            Mesh underSideMesh = Geometry.GeneratePlaneMesh(roadWidth, roadWidth);
            underSide.AddComponent<MeshFilter>().mesh = underSideMesh;
            underSide.AddComponent<MeshRenderer>();
            underSide.AddComponent<MeshCollider>().sharedMesh = underSideMesh;
            if (!sideMaterial) underSide.GetComponent<Renderer>().sharedMaterial = _cachedSideMaterial;
            else underSide.GetComponent<Renderer>().sharedMaterial = sideMaterial;
            underSide.transform.position = new Vector3(transform.position.x, transform.position.y - sideDepth, transform.position.z);
            underSide.transform.Rotate(new Vector3(180, 0, 0));
            underSide.transform.SetParent(transform);
            underSide.gameObject.hideFlags = HideFlags.HideInHierarchy;
            underSide.name = "Intersection Underside";

            // Generate snap points on each edge of the intersection
            if (_intersectionType == IntersectionType.ThreeLane)
            {
                CreateSnapPoint(Vector3.left * (roadWidth / 2), Quaternion.Euler(0, -90, 0), SnapPoint.PointEnd.Bipolar, "SnapPoint1");
                CreateSnapPoint(Vector3.forward * (roadWidth / 2), Quaternion.Euler(0, 0, 0), SnapPoint.PointEnd.Bipolar, "SnapPoint2");
                CreateSnapPoint(Vector3.right * (roadWidth / 2), Quaternion.Euler(0, 90, 0), SnapPoint.PointEnd.Bipolar, "SnapPoint3");
                CreateStartPoint((Vector3.left * (roadWidth / 4)) + (Vector3.back * (roadWidth / 4)), roadWidth / 8, "StartPoint1");
                CreateStartPoint((Vector3.left * (roadWidth / 4)) + (Vector3.forward * (roadWidth / 4)), roadWidth / 8, "StartPoint2");
                CreateStartPoint((Vector3.right * (roadWidth / 4)) + (Vector3.forward * (roadWidth / 4)), roadWidth / 8, "StartPoint3");
                CreateStartPoint((Vector3.right * (roadWidth / 4)) + (Vector3.back * (roadWidth / 4)), roadWidth / 8, "StartPoint4");
            }
            if (_intersectionType == IntersectionType.FourLane)
            {
                CreateSnapPoint(Vector3.left * (roadWidth / 2), Quaternion.Euler(0, -90, 0), SnapPoint.PointEnd.Bipolar, "SnapPoint1");
                CreateSnapPoint(Vector3.forward * (roadWidth / 2), Quaternion.Euler(0, 0, 0), SnapPoint.PointEnd.Bipolar, "SnapPoint2");
                CreateSnapPoint(Vector3.right * (roadWidth / 2), Quaternion.Euler(0, 90, 0), SnapPoint.PointEnd.Bipolar, "SnapPoint3");
                CreateSnapPoint(Vector3.back * (roadWidth / 2), Quaternion.Euler(0, 180, 0), SnapPoint.PointEnd.Bipolar, "SnapPoint4");
                CreateStartPoint((Vector3.left * (roadWidth / 4)) + (Vector3.back * (roadWidth / 4)), roadWidth / 8, "StartPoint1");
                CreateStartPoint((Vector3.left * (roadWidth / 4)) + (Vector3.forward * (roadWidth / 4)), roadWidth / 8, "StartPoint2");
                CreateStartPoint((Vector3.right * (roadWidth / 4)) + (Vector3.forward * (roadWidth / 4)), roadWidth / 8, "StartPoint3");
                CreateStartPoint((Vector3.right * (roadWidth / 4)) + (Vector3.back * (roadWidth / 4)), roadWidth / 8, "StartPoint4");
            }

            // Rotate back into place
            transform.rotation = _cachedRotation;
        }

        // Method to generate snap points with certain data
        /// <summary>
        /// Generate this intersection's SnapPoints
        /// </summary>
        /// <param name="offset">Offset from center</param>
        /// <param name="rotation">SnapPoint rotation</param>
        /// <param name="polarity">Magnet polarity (bipolar)</param>
        /// <param name="name">SnapPoint name</param>
        /// <returns>The attached SnapPoint's GameObject</returns>
        private GameObject CreateSnapPoint(Vector3 offset, Quaternion rotation, SnapPoint.PointEnd polarity, string name)
        {
            GameObject snapPoint = new GameObject();
            snapPoint.AddComponent<SnapPoint>().SetUp(polarity, roadWidth);
            snapPoint.transform.position = transform.position + offset;
            snapPoint.transform.rotation = rotation;
            snapPoint.transform.parent = _snapNodeParent.transform;
            snapPoint.name = name;
            return snapPoint;
        }

        // Method to generate the intersection's start points
        /// <summary>
        /// Generate this Intersection's StartPoints
        /// </summary>
        /// <param name="offset">Offset from center</param>
        /// <param name="radius">StartPoint editor sphere radius</param>
        /// <param name="name">StartPoint name</param>
        /// <returns>The attached StartPoint's GameObject</returns>
        private GameObject CreateStartPoint(Vector3 offset, float radius, string name)
        {
            GameObject startPoint = new GameObject();
            startPoint.AddComponent<StartPoint>();
            startPoint.transform.position = transform.position + offset;
            startPoint.transform.parent = transform;
            startPoint.name = name;
            return startPoint;
        }

        // enum to define the number of entering lanes to this intersection
        /// <summary>
        /// Intersection lane number enum
        /// </summary>
        public enum IntersectionType
        {
            ThreeLane,
            FourLane
        }
        #endregion
    }


	#if UNITY_EDITOR
    // This class handles the editable attributes of the intersection
    // exposing them to the inspector 

    /// <summary>
    /// Intersection's custom inspector UI
    /// </summary>
    [CustomEditor(typeof(Intersection))]
    public class IntersectionEditorInspector : Editor
    {
        #region INTERSECTION_DATA
        /// <summary>
        /// Reference to this Intersection
        /// </summary>
        private Intersection _intersection;

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

        /// <summary>
        /// Render the Intersection's custom inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            _intersection = target as Intersection;
            GUILayout.Label(_tbLogo, GUILayout.Width(EditorGUIUtility.currentViewWidth - 40.0f), GUILayout.Height(60.0f));
            GUILayout.Label("Intersection Editor:", EditorStyles.boldLabel);
            DrawDefaultInspector();
            var oldColor = GUI.color;
            GUI.color = new Color(1, 0.5f, 0.0f);
            if (GUILayout.Button("Regenerate Intersection Mesh"))
            {
                Undo.RecordObject(_intersection, "Generate Intersection Mesh");
                EditorUtility.SetDirty(_intersection);
                _intersection.GenerateIntersectionMesh();
            }
            GUI.color = oldColor;
        }
        #endregion
    }


    // This class implements a toolbar menu into the unity editor allowing the
    // user to spawn new instances of Intersections into the scene

    /// <summary>
    /// Intersection's Torchbearer toolbar integration
    /// </summary>
    [ExecuteInEditMode]
    public static class IntersectionSpawner
    {
        #region TOOLBAR_FUNCTIONS
        /// <summary>
        /// Toolbar method to create a new three-lane Intersection in the scene
        /// </summary>
        /// <returns>The generated Intersection's GameObject</returns>
        [MenuItem("Torchbearer Interactive/Magnet Roads/New Intersection/Three-lane")]
        public static GameObject CreateNewThreeLane()
        {
            GameObject newOne = new GameObject();
            newOne.name = "Three-lane Intersection";
            newOne.AddComponent<Intersection>().SetUp(Intersection.IntersectionType.ThreeLane);
            return newOne;
        }

        /// <summary>
        /// Toolbar method to create a new four-lane Intersection in the scene
        /// </summary>
        /// <returns>The generated Intersection's GameObject</returns>
        [MenuItem("Torchbearer Interactive/Magnet Roads/New Intersection/Four-lane")]
        public static GameObject CreateNewFourLane()
        {
            GameObject newOne = new GameObject();
            newOne.name = "Four-lane Intersection";
            newOne.AddComponent<Intersection>().SetUp(Intersection.IntersectionType.FourLane);
            return newOne;
        }
        #endregion
    }
	#endif
}