using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionStatController : MonoBehaviour {

    [Range(20,200)]
    public float range;
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, range);
    }
}
