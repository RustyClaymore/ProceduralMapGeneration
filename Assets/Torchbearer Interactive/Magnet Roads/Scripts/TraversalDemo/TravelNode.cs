using UnityEngine;
using System.Collections.Generic;
using MagnetRoads;

// ************************************************************************
// Copyright (C) Torchbearer Interactive, Ltd. - All Rights Reserved
//
// Unauthorized copying of this file, via any medium is strictly prohibited
// proprietary and confidential
// 
// Written by: Jonathan H Langley - jon@tbinteractive.co.uk, 2016
// ************************************************************************

// This class handles the managment of connection data inside the traversal
// demonstration scene and should serve as an examplar implementation

/// <summary>
/// (TraversalDemo)
/// </summary>
namespace TraversalDemo
{
    /// <summary>
    /// (TraversalDemo)
    /// </summary>
    [AddComponentMenu("")]
    public class TravelNode : MonoBehaviour
    {
        #region NODE_DATA
        // Array of connecting routes and their destinations
        /// <summary>
        /// List of connections for this TravelNode
        /// </summary>
        public List<Route> connections;
        #endregion


        #region ROUTE_DEFINITION
        // Enum to define the side of the road to be on
        /// <summary>
        /// The side of the road to use for a connection
        /// </summary>
        public enum RoadSide
        {
            Left,
            Right
        }

        // Serializable route class for defining viable connections
        /// <summary>
        /// Defines the data stored by a connection
        /// </summary>
        [System.Serializable]
        public class Route
        {
            #region ROUTE_DATA
            /// <summary>
            /// The stored vector array defining the route in 3D space
            /// </summary>
            [Tooltip("The stored vector array defining the route in 3D space")]
            private Vector3[] _route; // vector3 array containing the route to follow - generated based on the road side selected

            /// <summary>
            /// The Route's StartPoint
            /// </summary>
            [Tooltip("The Route's StartPoint")]
            public StartPoint startPoint; // start point object (invisible in-game... preferably)

            /// <summary>
            /// Array of MagnetRoads making up the route
            /// </summary>
            [Tooltip("Array of MagnetRoads making up the route")]
            public MagnetRoad[] roads; // spline road(s) reference

            /// <summary>
            /// Destination TravelNode
            /// </summary>
            [Tooltip("Destination TravelNode")]
            public TravelNode destination; // destination node

            /// <summary>
            /// Road lane to use
            /// </summary>
            [Tooltip("Road lane to use")]
            public RoadSide side; // side of the road to use - must be accurate
            #endregion


            #region META_GETTERS
            // Return the vector list of this route
            /// <summary>
            /// Returns a vector array defining the Route
            /// </summary>
            /// <returns>An array of Vector3s defining the Route</returns>
            public Vector3[] GetRoute()
            {
                List<Vector3> routeExport = new List<Vector3>();
                foreach (MagnetRoad road in roads)
                {
                    if (side == RoadSide.Left)
                    {
                        foreach (Vector3 v in road.GetLeftCarPath()) routeExport.Add(v);
                    }
                    if (side == RoadSide.Right)
                    {
                        foreach (Vector3 v in road.GetRightCarPath()) routeExport.Add(v);
                    }
                }
                _route = routeExport.ToArray();
                return _route;
            }
            #endregion
        }
        #endregion


        #region INITIALIZATION
        // Constructor
        /// <summary>
        /// TravelNode's constructor method
        /// </summary>
        public TravelNode()
        {
            connections = new List<Route>();
        }
        #endregion


        #region DEBUG
        // Method to check that this travel node object is active
        /// <summary>
        /// Check that this node is currently active
        /// </summary>
        /// <returns>A boolean value of this nodes active state</returns>
        public bool CheckNodeActive()
        {
            return true; // always true for basic travel nodes (i.e. intersections)
        }
        #endregion
    }
}