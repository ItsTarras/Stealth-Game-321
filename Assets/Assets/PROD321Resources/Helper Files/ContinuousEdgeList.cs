using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class takes a list of vertices and triangle indices which 
 * make up a polygon and returns a list of the vertices ordered so that
 * they define the outline of an object. We use this in portal culling
 * so that we can get the top left and bottom right vertices without
 * having to know the winding order of the triangles which make up the portal
 * 
 * This was originally written as Unity arbitrarily changed the winding order
 * of their Quads between versions which broke the original code. There was
 * no reason for them to do this, and no promise that it wouldn't happen again
 * so this code will *hopefully* make sure that if it does happen again, our
 * code will keep running
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */


public class ContinuousEdgeList
{
    // A helper class which defines an "edge", which is made of two vertices
    class Edge
    {
        // The two vertices which make the edge
        public Vector3 v1, v2;

        // The constructor which takes the two vertices which define the edge
        public Edge(Vector3 _v1, Vector3 _v2)
        {
            v1 = _v1; v2 = _v2;
        }

        // A helper function which takes a different edge and a distance threshold
        // and will return true if this edge is the same as the other edge, within
        // the distance threshold, regardless of vertex order. This way if we have
        // two triangles which share an edge, even if the vertices are not perfectly
        // overlapped, we can say it is the same edge. 
        public bool MatchesEdge(Edge e, float distThresh)
        {
            // Return true if the vertices of this edge and the other edge are
            // within distThresh of each other
            if (Vector3.Distance(v1, e.v1) < distThresh && Vector3.Distance(v2, e.v2) < distThresh)
                return true;

            // Return true if the vertices of this edge and the alternate vertices
            // of the other edge are within distThresh of each other
            if (Vector3.Distance(v1, e.v2) < distThresh && Vector3.Distance(v2, e.v1) < distThresh)
                return true;

            // Otherwise return false
            return false;
        }
    }

    // This function takes a list of vertices and triangle indices, and willl
    // return a list of vertices which define a continuous outline of the object
    // as long as the vertices around the way are within distThresh of each other
    public static List<Vector3> GetContinuousEdgeList(Vector3[] vertices, int[] triangles, float distThresh)
    {
        // Create a list of all edges
        List<Edge> allEdges = new List<Edge>();

        // Loop through each triangle
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Add the three edges which make up this triangle
            allEdges.Add(new Edge(vertices[triangles[i]], vertices[triangles[i + 1]]));
            allEdges.Add(new Edge(vertices[triangles[i + 1]], vertices[triangles[i + 2]]));
            allEdges.Add(new Edge(vertices[triangles[i + 2]], vertices[triangles[i]]));
        }

        // Remove all edges which are duplicate
        // Create a new list of unique edges
        List<Edge> uniqueEdges = new List<Edge>();

        // Loop through all edges
        for (int i = 0; i < allEdges.Count; i++)
        {
            // Set the "isDuplicate" flag to false
            bool isDuplicate = false;

            // Loop through all other edges
            for (int j = 0; j < allEdges.Count; j++)
            {
                // Skip this edge
                if (i != j)
                {
                    // If the edges match within the distance threshold
                    if (allEdges[i].MatchesEdge(allEdges[j], distThresh))
                        // This edge is a duplicate
                        isDuplicate = true;
                }
            }

            // If it's not a duplicate - add it to the list of unique edges
            if (!isDuplicate) uniqueEdges.Add(allEdges[i]);
        }

        // Create a list which will contain the sorted edges
        // Add the first edge into it
        List<Edge> sortedEdges = new List<Edge>();
        sortedEdges.Add(uniqueEdges[0]);

        // Get the first edge, and remove it from the unique edges list
        Edge currEdge = uniqueEdges[0];
        uniqueEdges.RemoveAt(0);

        // This boolean will exit our loop if we're unable to find a match
        // for an edge - this should never happen, but just in case
        bool found = true;

        // Loop through the remaining unique edges
        while (found && uniqueEdges.Count > 0)
        {
            // We haven't found a matching edge yet
            found = false;

            // Loop through all the remaining edges
            for (int i = 0; i < uniqueEdges.Count; i++)
            {
                // If this new edge starts at the same point as our current edge
                // ends - but doesn't end where the current edge starts (i.e.
                // it is connected, but continues around the outline)
                if (Vector3.Distance(uniqueEdges[i].v1, currEdge.v2) <= distThresh && Vector3.Distance(uniqueEdges[i].v2, currEdge.v1) > distThresh)
                {
                    // Add it to our list of sorted edges
                    sortedEdges.Add(uniqueEdges[i]);
                    // Set it as our current edge
                    currEdge = uniqueEdges[i];
                    // Remove it from our list of edges
                    uniqueEdges.RemoveAt(i);
                    // update the found boolean
                    found = true;
                    break;
                }

                // Otherwise if this new edge end at the same point as our current edge
                // ends - but doesn't start where the current edge starts (i.e.
                // it is connected, but continues around the outline)
                else if (Vector3.Distance(uniqueEdges[i].v2, currEdge.v2) <= distThresh && Vector3.Distance(uniqueEdges[i].v1, currEdge.v1) > distThresh)
                {
                    // Flip the direction of the edge
                    Edge newEdge = new Edge(uniqueEdges[i].v2, uniqueEdges[i].v1);
                    // Add the flipped edge to our list of sorted edges
                    sortedEdges.Add(newEdge);
                    // Set it as our current edge
                    currEdge = newEdge;
                    // Remove it from our list of edges
                    uniqueEdges.RemoveAt(i);
                    // update the found boolean
                    found = true;
                    break;
                }
            }
        }

        // Get the first vertex in every edge and stick it in an array
        // Create a list of continuous edges
        List<Vector3> continuousEdgeList = new List<Vector3>();

        // Loop through the sorted list
        for (int i = 0; i < sortedEdges.Count; i++)
            // Add the first vertex to our continous edge list
            continuousEdgeList.Add(sortedEdges[i].v1);

        // return the continuous edge list
        return continuousEdgeList;
    }
}