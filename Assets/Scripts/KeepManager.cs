﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KeepManager : MonoBehaviour {


	public GameObject[] units;
	public Sprite[] unitIco;
	public UnitController selected;

	private GameObject UI;
	private int maxUnitCount = 5;
	private int spawnLimit = 5;
	private float foodQty = 5f;
	private float rockQty = 20f;
	private float foodTick = 0f;
	private float foodSpeed = 10f;
	private float baseFoodRegen = 2f;
	private float farmerBonusFood = 0.2f;

	private Text resourceIndicator;
	private Text respawnIndicator;
	private Button[] unitPanel = new Button[3];

	// Use this for initialization
	void Start () {
		UI = GameObject.Find ("Canvas");
		Spawn (new int[]{1,0,0,0,1});
		Camera.main.GetComponent<CameraController>().keepTransform = this.transform;
		Camera.main.GetComponent<CameraController>().seekKeep = true;

		resourceIndicator = UI.transform.Find("Info_Panel/Resources").GetComponent<Text>();
		respawnIndicator = UI.transform.Find("Info_Panel/Unit_Cap_Control/Respawn").GetComponent<Text>();
		unitPanel[0] = UI.transform.Find("Unit_Panel/Unit0").GetComponent<Button>();
		unitPanel[1] = UI.transform.Find("Unit_Panel/Unit0_0").GetComponent<Button>();
		unitPanel[2] = UI.transform.Find("Unit_Panel/Unit0_1").GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {
		generateFood();

		resourceIndicator.text = "(♨) "+foodQty.ToString ("F0")+"   (ロ) "+rockQty.ToString ("F0");
		respawnIndicator.text = totalUnits() + " / " + spawnLimit;
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

	private int totalUnits(){
		return 	GameObject.FindGameObjectsWithTag("farmer").Length +
				GameObject.FindGameObjectsWithTag("quarrier").Length +
				GameObject.FindGameObjectsWithTag("lancer").Length;
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
		Vector3 basePos = transform.position + transform.forward.normalized * 5f;
		for(int i=0; i<unitList.Length; i++){
			Vector3 horz = transform.right.normalized * (i - (unitList.Length/2.0f)) * 10.0f;
			positions[i] = basePos + horz;
		}
		// Spawn the units
		for(int i=0; i<unitList.Length; i++){
			//GameObject unit = 
			Instantiate (units[unitList[i]], positions[i], transform.rotation);//as GameObject;
			//unit.transform.parent = transform;
			//unit.GetComponent<UnitController>().
		}
	}

	public void registerClick(UnitController unit){
		if (unit != null) {
			if (unit.selectable == true) {
				if (selected != null) {
					selected.transform.Find ("Selection Projector").GetComponent<Projector> ().enabled = false;
				}
				selected = unit;
				int altIndex = 1;
				for(int i=0; i<units.Length; i++){
					if(selected.transform.CompareTag (units[i].transform.tag)){
						unitPanel[0].image.sprite = unitIco[i];
					} else {
						unitPanel[altIndex++].image.sprite = unitIco[i];
					}
				}
				unitPanel[0].image.enabled = true;
				if (selected != null) {
					selected.transform.Find ("Selection Projector").GetComponent<Projector> ().enabled = true;
				}
			}
		} else {
			unitPanel[0].image.enabled = false;
			unitPanel[1].image.enabled = false;
			unitPanel[2].image.enabled = false;
			if (selected != null) {
				selected.transform.Find ("Selection Projector").GetComponent<Projector> ().enabled = false;
			}
			selected = null;
		}
	}

	public void toggleAlternateUnitTypes(){
		unitPanel[1].image.enabled = !unitPanel[1].image.enabled;
		unitPanel[2].image.enabled = !unitPanel[2].image.enabled;
	}

	public void changeUnit(int id){
		for(int i=0; i<units.Length; i++){
			if(unitPanel[id].image.sprite == unitIco[i]){
				// Command the unit to change. This is a placeholder.
				GameObject newUnit = Instantiate (units[i], selected.transform.position,selected.transform.rotation) as GameObject;
				GameObject.Destroy(selected.transform.root.gameObject);
				registerClick(null);
				// We need to update the camera for this case where the unit selection changes without a click from it.
				GameObject.Find ("Main Camera").GetComponent<CameraController>().unitController = newUnit.GetComponent<UnitController>();
				registerClick(newUnit.GetComponent<UnitController>());
				return;
			}
		}
	}

	public void alterSpawnCount(int amt){
		maxUnitCount += amt;
	}

	public UnitController getSelected(){
		return selected;
	}
}
