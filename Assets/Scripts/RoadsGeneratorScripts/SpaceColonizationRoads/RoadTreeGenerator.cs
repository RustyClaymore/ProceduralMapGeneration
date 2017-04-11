using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

[ExecuteInEditMode]
public class RoadTreeGenerator : MonoBehaviour
{
    public Transform root;

    public int leavesCount;
    public GameObject treeLeaf;
    public GameObject treeBranch;

    public int minDist = 10;
    public int maxDist = 100;

    public float branchLength;

    public Vector3 minVectorRange;
    public Vector3 maxVectorRange;

    public Vector3 rootPosition;
    public Vector3 rootDirection;

    private Tree roadTree;
    private GameObject treeHolder;

    private GameObject[] treeLeaves;

    public void Generate()
    {
        TreeParameters treeParams = new TreeParameters();
        treeParams.leavesCount = leavesCount;
        treeParams.minDist = minDist;
        treeParams.maxDist = maxDist;
        treeParams.branchLength = branchLength;
        treeParams.minVectorRange = minVectorRange;
        treeParams.maxVectorRange = maxVectorRange;
        treeParams.rootPosition = rootPosition;
        treeParams.rootDirection = rootDirection;

        roadTree = new Tree(treeParams);

        int treeLeavesCounter = 0;
        treeLeaves = new GameObject[roadTree.leaves.Count];

        treeHolder = new GameObject("Tree Holder");

        foreach (Leaf lf in roadTree.leaves)
        {
            //RaycastHit hit;
            //if (Physics.Raycast(lf.position, Vector3.down, out hit, 300))
            //{
            //    if (hit.transform.tag == "Terrain")
            //    {
            //        lf.position = hit.point;
            //    }
            //}
            treeLeaves[treeLeavesCounter] = (GameObject)Instantiate(treeLeaf, lf.position, Quaternion.identity, treeHolder.transform);
            treeLeavesCounter++;
        }

        GameObject branchesHolder = new GameObject("Branches Holder");

        int n = roadTree.leaves.Count;
        for (int i = 1; i < n; i++)
        {
            GameObject brch = (GameObject)Instantiate(treeBranch, roadTree.branches[i].position, Quaternion.identity, branchesHolder.transform);
            Vector3 pos0 = roadTree.branches[i].parent.position;
            Vector3 pos1 = roadTree.branches[i].position; 
            
            RaycastHit hit;
            if (Physics.Raycast(pos0, Vector3.down, out hit, 300))
            {
                if (hit.transform.tag == "Terrain")
                {
                    brch.transform.position = hit.point;
                    pos0 = hit.point;
                }
            }

            if (Physics.Raycast(pos1, Vector3.down, out hit, 300))
            {
                if (hit.transform.tag == "Terrain")
                {
                    pos1 = hit.point;
                }
            }

            brch.GetComponent<LineRenderer>().SetPosition(0, pos0);
            brch.GetComponent<LineRenderer>().SetPosition(1, pos1);
      
            Debug.Log("Added branch!");
            roadTree.Grow();
        }
        Debug.Log("done!");
    }
}
