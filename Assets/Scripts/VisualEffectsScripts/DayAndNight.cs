using UnityEngine;
using System.Collections;

public class DayAndNight : MonoBehaviour {

    public bool dayAndNightCycleActive = false;
    public GameObject lightSource;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
        if(dayAndNightCycleActive)
        {
            lightSource.transform.Rotate(new Vector3(10 * Time.deltaTime, 0, 0));
        }
	}
}
