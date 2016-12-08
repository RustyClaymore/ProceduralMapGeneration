using UnityEngine;
using System;

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

// This class is used for the sole purpose of indicating, in-editor, the locations of snappable points
// on existing SplineRoads

/// <summary>
/// MagnetRoads (base)
/// </summary>
namespace MagnetRoads
{
    /// <summary>
    /// MagnetRoads (base)
    /// </summary>
    [ExecuteInEditMode] [SelectionBase] [AddComponentMenu("")]
    public class SnapPoint : MonoBehaviour
    {
        #region SNAP_POINT_DATA
        /// <summary>
        /// Magnet polarity data
        /// </summary>
        public enum PointEnd
        {
            Positive,
            Negative,
            Bipolar
        }

        /// <summary>
        /// This SnapPoint's polarity
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private PointEnd _pointEnd;

        /// <summary>
        /// This source road's width
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private float _roadWidth;

        /// <summary>
        /// Accessor for the point's polarity
        /// </summary>
        public PointEnd PointType { get { return _pointEnd; } }
        #endregion


        #region INITIALIZATION
        // Initialisation method
        /// <summary>
        /// Set-up this SnapPoint
        /// </summary>
        /// <param name="pointType">The polarity of the point to set-up</param>
        /// <param name="roadWidth">The source road's width</param>
        public void SetUp(PointEnd pointType, float roadWidth)
        {
            _pointEnd = pointType;
            _roadWidth = roadWidth;
        }
        #endregion


        #region UNITY_CALLBACKS
        // Define the appearance of the different kinds of snap points
        /// <summary>
        /// Draw the in-editor giszmos for this SnapPoint
        /// </summary>
        private void OnDrawGizmos()
        {
            if (PointType == PointEnd.Positive)
            {
                Gizmos.color = new Color(1, 0.5f, 0.0f);
                var offset = new Vector3(_roadWidth / 3.5f, 0, 0);
                Gizmos.DrawLine(transform.position - offset, transform.position + offset);
                offset = new Vector3(0, 0, _roadWidth / 3.5f);
                Gizmos.DrawLine(transform.position - offset, transform.position + offset);
                Gizmos.DrawCube(transform.position, new Vector3(0.05f, 0.05f, 0.05f));

				#if UNITY_EDITOR
				Handles.color = new Color(1, 0.5f, 0.0f);
				Handles.DrawWireDisc(transform.position, Vector3.up, _roadWidth / 3.5f);
				Handles.DrawSolidDisc(transform.position, Vector3.up, _roadWidth / 8f);
				#endif
            }
            if (PointType == PointEnd.Negative)
            {
                Gizmos.color = Color.blue;
                var offset = new Vector3(_roadWidth / 3.5f, 0, 0);
                Gizmos.DrawLine(transform.position - offset, transform.position + offset);
				Gizmos.DrawCube(transform.position, new Vector3(0.05f, 0.05f, 0.05f));

				#if UNITY_EDITOR
				Handles.color = Color.blue;
                Handles.DrawWireDisc(transform.position, Vector3.up, _roadWidth / 3.5f);
                Handles.DrawSolidDisc(transform.position, Vector3.up, _roadWidth / 8f);
				#endif
            }
            if (PointType == PointEnd.Bipolar)
            {
				#if UNITY_EDITOR
                Handles.color = Color.white;
                Handles.DrawWireDisc(transform.position, Vector3.up, _roadWidth / 3f);
				#endif
            }
        }
        #endregion
    }


	#if UNITY_EDITOR
    // This class handles the custom inspector of a SnapPoint using it's stored
    // polarity to display specific information to the user

    /// <summary>
    /// Custom inspector UI for the SnapPoints
    /// </summary>
    [CustomEditor(typeof(SnapPoint))]
    public class SnapPointInspector : Editor
    {
        #region SNAP_POINT_DATA
        /// <summary>
        /// Reference to the source SnapPoint
        /// </summary>
        private SnapPoint _snapPoint;
        #endregion


        #region UNITY_CALLBACKS
        /// <summary>
        /// Draw the SnapPoint's custom inspector UI
        /// </summary>
        public override void OnInspectorGUI()
        {
            _snapPoint = target as SnapPoint;
            DrawDefaultInspector();
            try
            {
                if (_snapPoint.transform.parent.GetComponent<MagnetRoad>())
                {
                    EditorGUILayout.HelpBox("Do not use the Snap Points to manipulate the spline, click the road itself and make use of the yellow handles.", MessageType.Warning);
                }
                if (_snapPoint.transform.parent.GetComponent<Intersection>())
                {
                    EditorGUILayout.HelpBox("This is a Bipolar Snap Point, it will accept road ends of any polarity.", MessageType.Info);
                }
            }
            catch (NullReferenceException)
            {
                EditorGUILayout.HelpBox("This Snap Point must be a child of a relevant game object to function properly!", MessageType.Error);
            }
        }
        #endregion
    }
	#endif
}