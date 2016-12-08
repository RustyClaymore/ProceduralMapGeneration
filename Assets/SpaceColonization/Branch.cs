using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Branch {

    public Vector3 position;
    public Vector3 direction;
    public Transform parent;

    public Branch(Vector3 pos, Vector3 dir, Transform parent)
    {
        this.position = pos;
        this.direction = dir;
        this.parent = parent;
    }
}
