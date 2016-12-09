using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Branch {

    public Vector3 position;
    public Vector3 direction;
    public Vector3 originalDirection;
    public Vector3 parentPos;
    public Branch parent;
    public int count = 0;

    public float length = 30;

    public Branch(Vector3 pos, Vector3 dir, float branchLength, Branch parent)
    {
        this.position = pos;
        this.direction = dir;
        this.parent = parent;
        this.originalDirection = this.direction;
        this.length = branchLength;
    }

    public void Reset()
    {
        this.direction = originalDirection;
        count = 0;
    }

    public Branch Next()
    {
        Vector3 nextPosition = this.position + this.direction * length;
        Branch nextBranch = new Branch(nextPosition, this.direction, length, this);
        return nextBranch;
    }

    public void Draw()
    {
        if (parent != null)
        {
            Gizmos.DrawLine(this.position, this.parent.position);
        }
    }
}
