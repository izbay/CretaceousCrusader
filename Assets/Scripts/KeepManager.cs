using UnityEngine;
using System.Collections;

public class KeepManager : MonoBehaviour {

	public float foodQty;
	public float rockQty;
	public GameObject[] units;

	private int maxUnitCount = 5;
	private int spawnLimit = 5;

	// Use this for initialization
	void Start () {
		Spawn (new int[]{1,0,0,0,1});
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void Spawn(int[] unitList){
		Vector3[] positions = new Vector3[unitList.Length];
		Vector3 basePos = transform.position + transform.forward.normalized;
		for(int i=0; i<unitList.Length; i++){
			Vector3 horz = transform.right.normalized * (i - (unitList.Length/2.0f)) * 10.0f;
			positions[i] = basePos + horz;
		}
		// Spawn the units
		for(int i=0; i<unitList.Length; i++){
			GameObject unit = Instantiate (units[unitList[i]], positions[i], transform.rotation) as GameObject;
			//unit.transform.parent = transform;
			//unit.GetComponent<UnitController>().
		}
	}
}
