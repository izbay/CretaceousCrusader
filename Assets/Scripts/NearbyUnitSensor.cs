using UnityEngine;
using System.Collections;

public class NearbyUnitSensor : MonoBehaviour {

	private DinoController parentDino;

	void Start()
	{
		parentDino = this.transform.parent.GetComponent<DinoController>();
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("dino"))
		{
			if(parentDino != null){
				parentDino.dinosNearby.Add(other.GetComponent<DinoController>());
			}
		}
		else if (other.CompareTag("lancer") || other.CompareTag("quarrier") || other.CompareTag("farmer"))
		{
			parentDino.playerUnitsNearby.Add(other.GetComponent<PlayerUnitController>());
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("dino"))
		{
			parentDino.dinosNearby.Remove(other.GetComponent<DinoController>());
		}
		else if (other.CompareTag("lancer") || other.CompareTag("quarrier") || other.CompareTag("farmer"))
		{
			parentDino.playerUnitsNearby.Remove(other.GetComponent<PlayerUnitController>());
		}
	}
}
