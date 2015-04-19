using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KeepManager : MonoBehaviour {


	public GameObject[] units;
	public Sprite[] unitIco;
	public PlayerUnitController selected;

	private GameObject UI;
	private int maxUnitCount = 5;
	private int spawnLimit = 5;
	private float foodQty = 5f;
	private float rockQty = 20f;
	private float foodTick = 0f;
	private float foodSpeed = 10f;
	private float spawnTick = 0f;
	private float spawnSpeed = 30f;
	private float baseFoodRegen = 2f;
	private float farmerBonusFood = 0.2f;

	private Text resourceIndicator;
	private Text spawnCountIndicator;
	private Text spawnTimerIndicator;
	private Button[] unitPanel = new Button[3];

	// Use this for initialization
	void Start () {
		UI = GameObject.Find ("Canvas");
		Camera.main.GetComponent<CameraController>().keepTransform = this.transform;
		Camera.main.GetComponent<CameraController>().seekKeep = true;

		resourceIndicator = UI.transform.Find("Info_Panel/Resources").GetComponent<Text>();
		spawnCountIndicator = UI.transform.Find("Info_Panel/Unit_Cap_Control/Respawn").GetComponent<Text>();
		spawnTimerIndicator = UI.transform.Find("Info_Panel/Unit_Cap_Control/SpawnCountdown").GetComponent<Text>();
		unitPanel[0] = UI.transform.Find("Unit_Panel/Unit0").GetComponent<Button>();
		unitPanel[1] = UI.transform.Find("Unit_Panel/Unit0_0").GetComponent<Button>();
		unitPanel[2] = UI.transform.Find("Unit_Panel/Unit0_1").GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {
		generateFood();

		resourceIndicator.text = "(♨) "+foodQty.ToString ("F0")+"   (ロ) "+rockQty.ToString ("F0");
		spawnCountIndicator.text = totalUnits() + " / " + spawnLimit;
		if (totalUnits() < spawnLimit) {
			if(spawnTick < spawnSpeed){
				spawnTimerIndicator.text = "(↻) " + Mathf.RoundToInt(spawnSpeed-spawnTick).ToString ();
				spawnTick += Time.deltaTime;
			} else {
				Spawn();
				spawnTick = 0f;
			}
		} else {
			spawnTimerIndicator.text = "";
			spawnTick = 0f;
		}
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

	public void Spawn(){
		Spawn (new int[1] {0});
	}
	public void Spawn(int[] unitList){
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
				selected = unit as PlayerUnitController;
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

	public void changeUnit(PlayerUnitController unit, int id){

				// Command the unit to change. This is a placeholder.
			GameObject newUnit = Instantiate (units[id], unit.transform.position,unit.transform.rotation) as GameObject;
			GameObject.Destroy(unit.transform.root.gameObject);
		if (selected.GetInstanceID() == unit.GetInstanceID()) {
			registerClick (null);
			GameObject.Find ("Main Camera").GetComponent<CameraController> ().unitController = newUnit.GetComponent<UnitController> ();
			registerClick (newUnit.GetComponent<UnitController> ());
		} else {
			GameObject.Find ("Main Camera").GetComponent<CameraController> ().unitController = newUnit.GetComponent<UnitController> ();
		}

		return;

	}
	public void changeUnitClass(int id){
		for (int i=0; i<units.Length; i++) {
			if (unitPanel [id].image.sprite == unitIco [i]) {
				selected.changeClass (i);
			}
		}
	}
	public void alterSpawnCount(int amt){
		if (spawnLimit + amt <= maxUnitCount && spawnLimit + amt > 0) {
			spawnLimit += amt;
			UI.transform.Find("Info_Panel/Unit_Cap_Control/DelSpawn/Del").GetComponent<Text>().color = Color.white;
			UI.transform.Find("Info_Panel/Unit_Cap_Control/AddSpawn/Add").GetComponent<Text>().color = Color.white;
		}

		if(spawnLimit + 1 > maxUnitCount){
			UI.transform.Find("Info_Panel/Unit_Cap_Control/AddSpawn/Add").GetComponent<Text>().color = Color.gray;
		} else if (spawnLimit - 1 <= 0){
			UI.transform.Find("Info_Panel/Unit_Cap_Control/DelSpawn/Del").GetComponent<Text>().color = Color.gray;
		}
	}

	public UnitController getSelected(){
		return selected;
	}
	public void addRock(int r){
		rockQty+=r;
	}
}
