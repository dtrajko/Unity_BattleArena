using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BRS_PlaneDropManager : MonoBehaviour
{
	[Header("Map Settings")]
	public int MapSize = 8;
	[Range(1,9)]
	public int DropZoneRange = 8;
	[Header("Plane Settings")]
	public GameObject BRS_PlaneSpawn;
	public float BRS_PlaneAltitude;
	public float BRS_PlaneAirspeed = 100f;

	private Vector3[] PD_L;
	private Vector3[] PD_R;
	public GameObject PlaneStart;
	public GameObject PlaneStop;
	private int startFlightIndex;
	private int endFlightIndex;
	public bool VerifiedPath = false;

	void Start ()
	{
		PD_L = new Vector3[9];
		PD_R = new Vector3[9];

		var _MapSize = 8000;//MapSize * 500;
		var setupPosition = new Vector3 (-_MapSize, BRS_PlaneAltitude, _MapSize);

		for(int i = 0; i < PD_L.Length; i++)
		{
			PD_L [i] = setupPosition;
			setupPosition = new Vector3 (-_MapSize, BRS_PlaneAltitude, (setupPosition.z - 1000));
		}

			setupPosition = new Vector3 (_MapSize, BRS_PlaneAltitude, _MapSize);

		for(int i = 0; i < PD_R.Length; i++)
		{
			PD_R [i] = setupPosition;
			setupPosition = new Vector3 (_MapSize, BRS_PlaneAltitude, (setupPosition.z - 1000));
		}

		//Create the cylinder for flight check
		GameObject ADZ = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		ADZ.transform.position = Vector3.zero;
		//This value will be calculated in the future based on user preferences
		ADZ.transform.localScale = new Vector3(_MapSize, _MapSize, _MapSize);
		ADZ.name = "AcceptableDropZone";

		//Get an acceptable flight path
		SetupFlightPath();
	}

	private void SetupFlightPath()
	{
		// Let's find a path that is certainly THROUGH the cylinder
		VerifiedPath = false;
		int numberOfAttempts = 0;
		Vector3 startFlight;
		Vector3 endFlight;
        Debug.Log("SetupFlightPath executed.");

        do
		{
			Debug.Log("Planing optimal Route");
			//Pick a Random startpoint
			startFlightIndex = Random.Range(0, PD_L.Length);
			startFlight = PD_L [startFlightIndex];

			//Pick a Random endpoint
			endFlightIndex = Random.Range(0, PD_R.Length);
			endFlight = PD_R [endFlightIndex];

			PlaneStart.transform.position = startFlight;
			PlaneStop.transform.position = endFlight;
			PlaneStart.transform.LookAt (PlaneStop.transform);

			RaycastHit objectHit;
			if (Physics.Raycast (PlaneStart.transform.position, PlaneStart.transform.forward, out objectHit, 8000))
			{

				Debug.Log("Trying " + numberOfAttempts++ + " times");
				if (objectHit.collider.gameObject.name == "AcceptableDropZone")
				{
					VerifiedPath = true;
					Debug.Log ("Optimal Route Calculated");
					GameObject.Destroy (objectHit.collider.gameObject);
				}
			}
		} while (VerifiedPath != true);
	}

	// Update is called once per frame
	void Update ()
	{

	}

	void LateUpdate()
	{
		if (VerifiedPath)
		{
			SpawnPlane ();
		}
	}

	void SpawnPlane()
	{
		PlaneStart.transform.LookAt (PD_R [endFlightIndex]);
		Instantiate (BRS_PlaneSpawn, PlaneStart.transform.position, PlaneStart.transform.rotation);
		VerifiedPath = false;
        Debug.Log("Plane Spawned!");
	}
}
