using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotsGeneration : MonoBehaviour {
    // *************** PUBLICS ***************
    public int seed;

    [Range(1,7)]
    public int numSubdivisions;
    public float maxPrimaryRoadsDistance;

    public bool showObbs;
    public bool autoUpdate;

    [Space(10)]
    public GameObject lineRendererParcel;
    public GameObject lineRendererRoad;
    public GameObject lineRendererObb;
    public GameObject pointPrefab;

    public GameObject regionCenterPrefab;

    public List<Vector3> centroidsLocations;

    // *************** PRIVATES ***************
    private List<GameObject> regionCenters;

    private List<Parcel> parcels;

    private GameObject plotsHolder;
    private GameObject centroidsHolder;
    private GameObject roadsHolder;
    private GameObject regionCentersHolder;
	private GameObject convexHullHolder;
    private GameObject plotPointsHolder;

    private int parcelsIdCounter = 0;

    // Use this for initialization
    void Start () {
		regionCenters = new List<GameObject>();
	}

    public void GeneratePlots()
	{
		ClearPlotsData();
		Random.InitState(seed);

		regionCentersHolder = GameObject.FindGameObjectWithTag("RegionCentersHolder");
		if (regionCenters.Count == 0)
		{
			foreach (Transform child in regionCentersHolder.transform)
			{
				regionCenters.Add(child.gameObject);
			}
		}

		if (regionCenters.Count > 0)
        {
            parcels = new List<Parcel>();
            plotsHolder = new GameObject("Plots Holder");
			plotsHolder.tag = "PlotsHolder";
            centroidsHolder = new GameObject("Centroids Holder");
			centroidsHolder.tag = "CentroidsHolder";
			convexHullHolder = new GameObject("Convex Hull Holder");
			convexHullHolder.tag = "ConvexHullHolder";
            if (!GameObject.FindGameObjectWithTag("PlotPointsHolder"))
            {
                plotPointsHolder = new GameObject("Plot Points Holder");
                plotPointsHolder.tag = "PlotPointsHolder";
            }

            for (int i = 0; i < regionCenters.Count; i++)
            {
                Parcel parcel;// = new Parcel(parcelsIdCounter.ToString());
                parcelsIdCounter++;

                List<Vector3> generatedPoints = new List<Vector3>();

                Vector3 regionCenterPos = regionCenters[i].transform.position;
                float regionRange = regionCenters[i].GetComponent<ParcelGenerationController>().range;
                generatedPoints = GeneratePolygon(new Vector3(regionCenterPos.x, regionCenters[i].transform.position.y, regionCenterPos.z), regionRange, 10);
                parcel = CreateParcelFromPoints(generatedPoints);

				parcel.obb.FindObbLTTS(parcel);
				ShowConvexHull(parcel);

                parcels.Add(parcel);
            }

            for (int i = 0; i < parcels.Count; i++)
            {
                RecursiveParceller(parcels[i], numSubdivisions);
            }

            roadsHolder = new GameObject("Roads Holder");
			roadsHolder.tag = "RoadsHolder";

            for (int i = 0; i < regionCenters.Count; i++)
            {
				for (int j = 0; j < regionCenters.Count; j++)
				{
					if(i != 0)
					{
						//int nextIndex = (i + 1) % regionCenters.Count;
						Segment shortestRoute = FindShortestRouteBetweenParcels(parcels[i], parcels[j]);

                        if(shortestRoute.Length() < maxPrimaryRoadsDistance)
                        {
                            GameObject line = (GameObject)Instantiate(lineRendererRoad);
                            line.GetComponent<LineRenderer>().SetPosition(0, shortestRoute.startPos);
                            line.GetComponent<LineRenderer>().SetPosition(1, shortestRoute.finalPos);
                            line.transform.parent = roadsHolder.transform;
                        }
					}
				}
            }
        }
    }

	public void ShowConvexHull(Parcel parcel)
	{
		foreach(Segment seg in parcel.obb.convexHullSegments)
		{
			GameObject line = (GameObject)Instantiate(lineRendererRoad);
			line.transform.parent = convexHullHolder.transform;
			line.GetComponent<LineRenderer>().SetPosition(0, seg.startPos);
			line.GetComponent<LineRenderer>().SetPosition(1, seg.finalPos);
		}
	}

	public void ClearPlotsData()
    {
        Debug.ClearDeveloperConsole();

        parcelsIdCounter = 0;
        parcels = new List<Parcel>();

		GameObject plotsHolder = GameObject.FindGameObjectWithTag("PlotsHolder");
		GameObject centroidsHolder = GameObject.FindGameObjectWithTag("CentroidsHolder");
		GameObject roadsHolder = GameObject.FindGameObjectWithTag("RoadsHolder");
		GameObject convexHullHolder = GameObject.FindGameObjectWithTag("ConvexHullHolder");
        GameObject plotPointsHolder = GameObject.FindGameObjectWithTag("PlotPointsHolder");

        DestroyImmediate(plotsHolder);
        DestroyImmediate(centroidsHolder);
        DestroyImmediate(roadsHolder);
        DestroyImmediate(convexHullHolder);
        DestroyImmediate(plotPointsHolder);
    }

	public void ResetSettings()
    {
        Debug.ClearDeveloperConsole();

        parcelsIdCounter = 0;

		parcels = new List<Parcel>();
		regionCenters = new List<GameObject>();

		GameObject regionCentersHolder = GameObject.FindGameObjectWithTag("RegionCentersHolder");
		GameObject plotsHolder = GameObject.FindGameObjectWithTag("PlotsHolder");
		GameObject centroidsHolder = GameObject.FindGameObjectWithTag("CentroidsHolder");
		GameObject roadsHolder = GameObject.FindGameObjectWithTag("RoadsHolder");
		GameObject convexHullHolder = GameObject.FindGameObjectWithTag("ConvexHullHolder");
        GameObject plotPointsHolder = GameObject.FindGameObjectWithTag("PlotPointsHolder");

		DestroyImmediate(regionCentersHolder);
		DestroyImmediate(plotsHolder);
		DestroyImmediate(centroidsHolder);
		DestroyImmediate(roadsHolder);
		DestroyImmediate(convexHullHolder);
        DestroyImmediate(plotPointsHolder);
	}

    public void AddRegionPoint()
    {
        if(!regionCentersHolder)
        {
            regionCentersHolder = new GameObject("Region Centers Holder");
			regionCentersHolder.tag = "RegionCentersHolder";
        }
        GameObject regCenter = (GameObject) Instantiate(regionCenterPrefab);
        regionCenters.Add(regCenter);
        regCenter.transform.parent = regionCentersHolder.transform;
    }

    void ShowOBBRect(Parcel parcel)
    {
        GameObject obbRect = new GameObject("OBB Rect");
        obbRect.transform.parent = GameObject.FindGameObjectWithTag("PlotsHolder").transform;

        Rectangle rect = parcel.obb.obbRect;

        GameObject lineTop = (GameObject)Instantiate(lineRendererObb);
        lineTop.transform.parent = obbRect.transform;
        GameObject lineBot = (GameObject)Instantiate(lineRendererObb);
        lineBot.transform.parent = obbRect.transform;
        GameObject lineLeft = (GameObject)Instantiate(lineRendererObb);
        lineLeft.transform.parent = obbRect.transform;
        GameObject lineRight = (GameObject)Instantiate(lineRendererObb);
        lineRight.transform.parent = obbRect.transform;

        lineTop.GetComponent<LineRenderer>().SetPosition(0, rect.topSegment.startPos);
        lineTop.GetComponent<LineRenderer>().SetPosition(1, rect.topSegment.finalPos);

        lineBot.GetComponent<LineRenderer>().SetPosition(0, rect.botSegment.startPos);
        lineBot.GetComponent<LineRenderer>().SetPosition(1, rect.botSegment.finalPos);

        lineLeft.GetComponent<LineRenderer>().SetPosition(0, rect.leftSegment.startPos);
        lineLeft.GetComponent<LineRenderer>().SetPosition(1, rect.leftSegment.finalPos);

        lineRight.GetComponent<LineRenderer>().SetPosition(0, rect.rightSegment.startPos);
        lineRight.GetComponent<LineRenderer>().SetPosition(1, rect.rightSegment.finalPos);
    }

    void RecursiveParceller(Parcel parcel, int iter)
    {
        List<Parcel> subParcels = new List<Parcel>();
        subParcels = UseParceller(parcel, iter);

        if(showObbs)
            ShowOBBRect(parcel);


        if (parcel.obb.obbRect.area > 0)
        {
            if (iter > 0)
            {
                for (int i = 0; i < subParcels.Count; i++)
                {
                    RecursiveParceller(subParcels[i], iter - 1);
                }
            }
            //else
            //{
            //    GameObject centroid = Instantiate(pointPrefab, parcel.FindParcelCentroid(), Quaternion.identity);
            //    centroid.transform.parent = centroidsHolder.transform;
            //}
        }
    }

    List<Parcel> UseParceller(Parcel parcel, int iter)
    {
        Parceller parceller = new Parceller(parcel);
        parceller.SubdivideParcel(iter);
        DrawSubParcels(parceller.parcel);

        GameObject centroid = Instantiate(pointPrefab, parcel.FindParcelCentroid(), Quaternion.identity);
        centroid.transform.parent = centroidsHolder.transform;

        centroidsLocations.Add(centroid.transform.position);

        return parceller.parcel.subParcels;
    }

    void DrawSubParcels(Parcel parcel)
    {
        for (int j = 0; j < parcel.subParcels.Count; j++)
        {
            if(parcel.subParcels[j].parcelSegments.Count != 0)
            {
                GameObject subParcel = new GameObject("Sub Parcel " + parcel.subParcels[j].name);
                subParcel.transform.parent = plotsHolder.transform;
                for (int i = 0; i < parcel.subParcels[j].parcelSegments.Count; i++)
                {
                    GameObject line = Instantiate(lineRendererParcel, parcel.subParcels[j].parcelSegments[i].startPos, Quaternion.identity);
                    line.GetComponent<LineRenderer>().SetPosition(0, parcel.subParcels[j].parcelSegments[i].startPos);
                    line.GetComponent<LineRenderer>().SetPosition(1, parcel.subParcels[j].parcelSegments[i].finalPos);
                    line.transform.parent = subParcel.transform;
                }
            }
        }
    }

    Segment FindShortestRouteBetweenParcels(Parcel par1, Parcel par2)
    {
        float shortestDistance = float.MaxValue;
        Vector3 closestPointPar1 = Vector3.zero;
        Vector3 closestPointPar2 = Vector3.zero;

        for (int i = 0; i < par1.parcelPoints.Count; i++)
        {
            for (int j = 0; j < par2.parcelPoints.Count; j++)
            {
                float distance = Vector3.Distance(par1.parcelPoints[i], par2.parcelPoints[j]);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPointPar1 = par1.parcelPoints[i];
                    closestPointPar2 = par2.parcelPoints[j];
                }
            }
        }

        Segment seg = new Segment(closestPointPar1, closestPointPar2);
        return seg;
    }

    Parcel CreateParcelFromPoints(List<Vector3> points)
    {
        Parcel parcel = new Parcel((parcelsIdCounter - 1).ToString(), 0);
        for (int i = 0; i < points.Count; i++)
        {
            int nextIndex = (i + 1) % points.Count;
            Segment seg = new Segment(points[i], points[nextIndex]);
            parcel.parcelSegments.Add(seg);
        }
        parcel.UpdateParcelPointsFromSegments();

        return parcel;
    }

    List<Vector3> GeneratePolygon(Vector3 startingPos, float size, int numPoits)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < numPoits; i++)
        {
            points.Add(new Vector3(0, 0, 0));
        }

        float angle = 0f;
        float distance = 0f;

        GameObject plotHolder = new GameObject("Plot Holder");
        plotHolder.transform.parent = plotPointsHolder.transform; 

        for (int i = 0; i < points.Count; i++)
        {
            distance = Random.Range(size - size / 3, size + size / 3);
            angle = angle + Random.Range((360f / numPoits) - 5f, (360f / numPoits) + 5f) * Mathf.PI / 180f;

            float newX = Mathf.Cos(angle);
            float newZ = Mathf.Sin(angle);
            
            points[i] = startingPos + new Vector3(newX, 0, newZ) * distance;

            GameObject plotPoint = Instantiate(pointPrefab, points[i], Quaternion.identity);
            plotPoint.transform.parent = plotHolder.transform;
        }
        
        return points;
    }

}
