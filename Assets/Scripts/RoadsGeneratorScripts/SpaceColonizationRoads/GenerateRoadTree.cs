using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRoadTree : MonoBehaviour {

    public int leavesCount;
    public GameObject treeLeaf;

    private Tree roadTree;
    private GameObject treeHolder;


	// Use this for initialization
	void Start () {
		roadTree = new Tree(leavesCount);

        treeHolder = new GameObject("Tree Holder");
        foreach(Leaf lf in roadTree.leaves)
        {
            Instantiate(treeLeaf, lf.position, Quaternion.identity, treeHolder.transform);
        }
	}
	
}
