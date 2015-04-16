using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KeepManager : MonoBehaviour {


	public GameObject[] units;

	private GameObject UI;
	private int maxUnitCount = 5;
	private int spawnLimit = 5;
	private float foodQty = 5f;
	private float rockQty = 20f;
	private float foodTick = 0f;
	private float foodSpeed = 10f;
	private float baseFoodRegen = 2f;
	private float farmerBonusFood = 0.2f;

	// Use this for initialization
	void Start () {
		UI = GameObject.Find ("Canvas");
		Spawn (new int[]{1,0,0,0,1});
	}
	
	// Update is called once per frame
	void Update () {
		generateFood();

		Text txt = UI.transform.Find("Info_Panel/Resources").GetComponent<Text>();
		txt.text = "(♨) "+foodQty.ToString ("F0")+"   (ロ) "+rockQty.ToString ("F0");
	}

	public float requestFood(float amount){
		if(amount <= foodQty){
			foodQty -= amount;
			return amount;
		} else {
			float returnVal = foodQty;
			foodQty = 0;
			return returnVal;
		}
	}

	private void generateFood(){
		if (foodTick >= foodSpeed){
			int farmerCnt = GameObject.FindGameObjectsWithTag("farmer").Length;
			foodQty += baseFoodRegen + farmerBonusFood * farmerCnt;
			foodTick = 0f;
		} else {
			foodTick += Time.deltaTime;
		}
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
