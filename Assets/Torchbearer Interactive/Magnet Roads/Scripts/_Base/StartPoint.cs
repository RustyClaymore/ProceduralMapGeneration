using UnityEngine;

// ************************************************************************
// Copyright (C) Torchbearer Interactive, Ltd. - All Rights Reserved
//
// Unauthorized copying of this file, via any medium is strictly prohibited
// proprietary and confidential
// 
// Written by: Jonathan H Langley - jon@tbinteractive.co.uk, 2016
// ************************************************************************

// This script simply defines and draws the start nodes for user benefit

/// <summary>
/// MagnetRoads (base)
/// </summary>
namespace MagnetRoads
{
    /// <summary>
    /// MagnetRoads (base)
    /// </summary>
    [AddComponentMenu("")]
    public class StartPoint : MonoBehaviour
    {
        #region START_POINT_DATA
        // Size information & accessor
        /// <summary>
        /// Size of all StartNodes
        /// </summary>
        private const float START_NODE_SIZE = 0.05f;
        #endregion


        #region UNITY_CALLBACKS
        // Draw the correct colour for the start positions 
        /// <summary>
        /// Draw the gizmos for the StartPoint
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0.5f, 0.0f);
            Gizmos.DrawSphere(transform.position, START_NODE_SIZE);
        }
        #endregion
    }
}
