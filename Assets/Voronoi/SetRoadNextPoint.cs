using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SetRoadNextPoint : MonoBehaviour {

    public GameObject initialPoint;
    public GameObject nextPoint;
    
    private Vector3 storedInitialPoint;
    private Vector3 storedNextPoint;

    private LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
        storedInitialPoint = initialPoint.transform.position;
        storedNextPoint = nextPoint.transform.position;

        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, storedInitialPoint);
        lineRenderer.SetPosition(1, storedNextPoint);
	}

    // Update is called once per frame
    void Update()
    {
        if (InitialPointHasChanged())
        {
            lineRenderer.SetPosition(0, initialPoint.transform.position);
        }
        if (NextPointHasChanged())
        {
            lineRenderer.SetPosition(1, nextPoint.transform.position);
        }
    }

    bool InitialPointHasChanged()
    {
        if (initialPoint.transform.position != storedInitialPoint)
        {
            storedInitialPoint = initialPoint.transform.position;
            return true;
        }
        else
        {
            return false;
        }
    }

    bool NextPointHasChanged()
    {
        if (nextPoint.transform.position != storedNextPoint)
        {
            storedNextPoint = nextPoint.transform.position;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetInitialPoint(GameObject initPnt)
    {
        initialPoint = initPnt;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, initialPoint.transform.position);
    }

    public void SetNextPoint(GameObject nextPnt)
    {
        nextPoint = nextPnt;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(1, nextPoint.transform.position);
    }
}
