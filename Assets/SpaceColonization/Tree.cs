using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree {

    public int leavesCount = 0;

    public List<Leaf> leaves = new List<Leaf>();
    public List<Branch> branches = new List<Branch>();

    private int minDist;
    private int maxDist;

    private int previousLeavesCount;

	//public Tree(Vector3 rootPos, int leavesCount, int minDist, int maxDist, Vector3 minVectorRange, Vector3 maxVectorRange, float branchLength)
    public Tree(TreeParameters treeParams)
    {
        this.leavesCount = treeParams.leavesCount;
        this.minDist = treeParams.minDist;
        this.maxDist = treeParams.maxDist;

        previousLeavesCount = leavesCount;

        float randX;
        float randY;
        float randZ;

        System.Random random = new System.Random();
        for (int i = 0; i < leavesCount; i++)
        {
            randX = Random.Range(treeParams.minVectorRange.x, treeParams.maxVectorRange.x);
            randY = Random.Range(treeParams.minVectorRange.y, treeParams.maxVectorRange.y);
            randZ = Random.Range(treeParams.minVectorRange.z, treeParams.maxVectorRange.z);

            leaves.Add(new Leaf(randX, randY, randZ));
        }

        //Vector3 rootPos = new Vector3(0f, 0, 0f);
        Branch root = new Branch(treeParams.rootPosition, treeParams.rootDirection, treeParams.branchLength, null);
        branches.Add(root);
        Branch current = root;
        bool found = false;

        int iter = 0;
        while (!found || iter < 50)
        {
            for (int i = 0; i < leaves.Count; i++)
            {
                float dist = Vector3.Distance(current.position, leaves[i].position);
                if (dist < this.maxDist)
                {
                    found = true;
                }
            }

            if (!found)
            {
                Branch nextBranch = current.Next();
                current = nextBranch;
                branches.Add(current);
            }
            iter++;
        }
    }

    public bool Grow()
    {
        for (int i = 0; i < leaves.Count; i++)
        {
            Leaf leaf = leaves[i];

            Branch closestBranch = null;
            float record = 100000;

            for (int j = 0; j < branches.Count; j++)
            {
                Branch branch = branches[j];
                float dist = Vector3.Distance(leaves[i].position, branch.position);

                if (dist < minDist)
                {
                    leaves[i].reached = true;
                    closestBranch = null;
                    break;
                }
                else if (dist > maxDist)
                {
                    continue;
                }
                else if(closestBranch == null || dist < record)
                {
                    closestBranch = branch;
                    record = dist;
                }
            }

            if(closestBranch != null)
            {
                Vector3 newDirection = leaves[i].position - closestBranch.position;
                newDirection.Normalize();
                closestBranch.direction += newDirection;
                closestBranch.count++;
            }
        }

        for (int i = leaves.Count-1; i >= 0; i--)
        {
            if(leaves[i].reached == true)
            {
                leaves.Remove(leaves[i]);
            }
        }

        for (int i = branches.Count - 1; i >= 0; i--)
        {
            Branch branch = branches[i];
            if(branch.count > 0)
            {
                branch.direction /= (branch.count + 1);
                Branch newBranch = branch.Next();
                branches.Add(newBranch);
            }
            branch.Reset();
        }
        return true;
    }

    public void Show()
    {
        for (int i = 0; i < leaves.Count; i++)
        {
            Gizmos.DrawSphere(leaves[i].position, 3);
        }

        for (int i = 0; i < branches.Count; i++)
        {
            branches[i].Draw();
        }
    }
}
