using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRoadTree : MonoBehaviour {

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

	// Use this for initialization
	IEnumerator Start () {
        yield return Generate();
        Debug.Log("done");  
    }

    public IEnumerator Generate()
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
            Instantiate(treeLeaf, lf.position, Quaternion.identity, treeHolder.transform);
        }

        GameObject branchesHolder = new GameObject("Branches Holder");

        for (int i = 1; i < roadTree.branches.Count; i++)
        {
            GameObject brch = (GameObject)Instantiate(treeBranch, roadTree.branches[i].position, Quaternion.identity, branchesHolder.transform);
            brch.GetComponent<LineRenderer>().SetPosition(0, roadTree.branches[i].parent.position);
            brch.GetComponent<LineRenderer>().SetPosition(1, roadTree.branches[i].position);

            yield return new WaitForSecondsRealtime(0);
            roadTree.Grow();
        }
    }
}
