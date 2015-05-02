using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class KeepManager : MonoBehaviour {


	public GameObject[] units;
	public Sprite[] unitIco;
	public PlayerUnitController selected;
	public bool placingHut;
	public GameObject hutPrefab;
	public int maxUnitCount = 5;
	public Font font;
	public int spawnLimit = 5;

	private bool rotatingHut = false;
	private GameObject hutPlacement;
	private HutManager hutPlacementManager;
	private GameObject UI;
	private bool clickSpamLimiter;
	private float foodQty = 10f;
	private float rockQty = 0f;
	private float foodTick = 0f;
	private float foodSpeed = 10f;
	private float spawnTick = 0f;
	private float spawnSpeed = 30f;
	private float spawnMod = 0f;
	private float baseFoodRegen = 2f;
	private float farmerBonusFood = 0.2f;
	private float hutCost = 200f;
	private TerrainBuilder tb;
	private List<float> harmonicNums = new List<float>();
    private List<PlayerUnitController> playerUnitList = new List<PlayerUnitController>();  
	private Text resourceIndicator;
	private Text spawnCountIndicator;
	private Text spawnTimerIndicator;
	private Button[] unitPanel = new Button[3];
	private bool escMenu = false;
	private GameObject escButton;
	private StatTracker statTracker;

	// Use this for initialization
	void Start () {
		UI = GameObject.Find ("Canvas");
		Camera.main.GetComponent<CameraController>().keepTransform = this.transform;
		Camera.main.GetComponent<CameraController>().seekKeep = true;
		tb = GameObject.Find("Terrain").GetComponent<TerrainBuilder>();
		try{ statTracker = GameObject.FindGameObjectWithTag("stat_tracker").GetComponent<StatTracker>(); } catch {}
		escButton = GameObject.Find ("Quit");
		escButton.SetActive(false);
		resourceIndicator = UI.transform.Find("Info_Panel/Resources").GetComponent<Text>();
		spawnCountIndicator = UI.transform.Find("Info_Panel/Unit_Cap_Control/Respawn").GetComponent<Text>();
		spawnTimerIndicator = UI.transform.Find("Info_Panel/Unit_Cap_Control/SpawnCountdown").GetComponent<Text>();
		unitPanel[0] = UI.transform.Find("Unit_Panel/Unit0").GetComponent<Button>();
		unitPanel[1] = UI.transform.Find("Unit_Panel/Unit0_0").GetComponent<Button>();
		unitPanel[2] = UI.transform.Find("Unit_Panel/Unit0_1").GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {
		try{
			statTracker.time += Time.deltaTime;
		} catch {}
		if(Input.GetKeyDown (KeyCode.Escape)){
			escMenu = !escMenu;
			escButton.SetActive(escMenu);
		}
		
		int unitTotal = totalUnits ();

		/** This case is obsolete with the Princess change. If there are 0 units, she already ended the game.
		if(unitTotal == 0){
			Application.LoadLevel("Lose");
		}*/

		generateFood();

		resourceIndicator.text = "(♨) "+foodQty.ToString ("F0")+"   (ロ) "+rockQty.ToString ("F0");
		spawnCountIndicator.text = unitTotal + " / " + spawnLimit;
		if (totalUnits() < spawnLimit) {
			if(spawnTick < spawnSpeed + spawnMod){
				spawnTimerIndicator.text = "(↻) " + Mathf.RoundToInt(spawnSpeed+spawnMod-spawnTick).ToString ();
				spawnTick += Time.deltaTime;
			} else {
				Spawn();
				spawnTick = 0f;
			}
		} else {
			spawnTimerIndicator.text = "";
			spawnTick = 0f;
		}

		if(placingHut){
			dragHut ();
		}
	}

	public void quitGame(){
		GameObject LDino = GameObject.Find("L_Dino");
		if(LDino != null)
			LDino.GetComponent<LargeDinoController>().loseFlag = true;
		Application.LoadLevel("Lose");
	}

	private void dragHut(){
		if(rotatingHut){
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)){
				Vector3 sourcePosition = new Vector3(hutPlacement.transform.position.x, hit.point.y, hutPlacement.transform.position.z);
				if(sourcePosition-hit.point != Vector3.zero){
					hutPlacement.transform.rotation = Quaternion.LookRotation(sourcePosition-hit.point, transform.up);
				}
			}

			if(!(Input.GetMouseButton (0) || Input.GetMouseButton (1))){
				clickSpamLimiter = true;
			}

			if(clickSpamLimiter && (Input.GetMouseButton (0))){
				hutPlacementManager.restoreTextures();
				rockQty -= hutCost;
				hutPlacement = null;
				hutPlacementManager = null;
				placingHut = false;
				rotatingHut = false;
			} else if(Input.GetMouseButton (1)){
				GameObject.Destroy (hutPlacement);
				hutPlacement = null;
				hutPlacementManager = null;
				placingHut = false;
				rotatingHut = false;
				return;
			}
		} else {
			int biome = tb.getBiomeAtWorldCoord(hutPlacement.transform.position);
			bool isValid = !(biome == 0 || biome == 5) && Vector3.Distance(transform.position, hutPlacement.transform.position) < 200f;
			hutPlacementManager.planningTextures (isValid);

			if (Input.GetMouseButton (0) && isValid) {
				rotatingHut = true;
				clickSpamLimiter = false;
				return;
			}

			if (Input.GetMouseButton (1)) {
				GameObject.Destroy (hutPlacement);
				hutPlacement = null;
				hutPlacementManager = null;
				placingHut = false;
				return;
			}

			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)){
				int layerHit = hit.transform.gameObject.layer;
				if(layerHit==LayerMask.NameToLayer("Water")){
					hutPlacement.transform.position = hit.point;
				}
			}
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

	public int totalUnits(){
		int total = GameObject.FindGameObjectsWithTag("farmer").Length + // includes princesses...
					GameObject.FindGameObjectsWithTag("quarrier").Length +
					GameObject.FindGameObjectsWithTag("lancer").Length;
		try{
			if(total > statTracker.maxUnits){
				statTracker.maxUnits = total;
			}
		}catch{}
		return total;
	}

	private void generateFood(){
		if (foodTick >= foodSpeed){
			int farmerCnt = GameObject.FindGameObjectsWithTag("farmer").Length;
			foodQty += baseFoodRegen + farmerBonusFood * farmerCnt;
			foodTick = 0f;

			spawnMod = getHarmonicNum(farmerCnt) * -5f;
		} else {
			foodTick += Time.deltaTime;
		}
	}

	private float getHarmonicNum(int num){
		if (num <= 0){
			return 0;
		} else if(num <= harmonicNums.Count){
			return harmonicNums[num-1];
		} else {
			for(int i=harmonicNums.Count-1; i<num; i++){
				if(i == -1){
					harmonicNums.Add (0.5f);
				} else {
					harmonicNums.Add (harmonicNums[i]+(1f/(i+3)));
				}
			}
			return harmonicNums[harmonicNums.Count-1];
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
		for(int i=0; i<unitList.Length; i++)
		{
		    GameObject temp = Instantiate(units[unitList[i]], positions[i], transform.rotation) as GameObject;
			if(!(temp.GetComponent<PlayerUnitController>() is PrincessController))
			   playerUnitList.Add(temp.GetComponent<PlayerUnitController>());
		}
	}

	public void registerClick(UnitController unit){
		if (unit != null) {
			if (unit.selectable == true) {
				if (selected != null) {
					selected.toggleSelected();
				}
				selected = unit as PlayerUnitController;
				int altIndex = 1;
				for(int i=0; i<units.Length-1; i++){
					if(selected.transform.CompareTag (units[i].transform.tag)){
						unitPanel[0].image.sprite = unitIco[i];
					} else {
						unitPanel[altIndex++].image.sprite = unitIco[i];
					}
				}
				unitPanel[0].image.enabled = true;
				if (selected != null) {
					selected.toggleSelected ();
				}
			}
		} else {
			unitPanel[0].image.enabled = false;
			unitPanel[1].image.enabled = false;
			unitPanel[2].image.enabled = false;
			if (selected != null) {
				selected.toggleSelected();
			}
			selected = null;
		}
	}

	public void doPrimaryButtonAction(){
		bool isUnit = false;
		for(int i=0; i<units.Length-1; i++){
			if(selected != null && selected.transform.CompareTag (units[i].transform.tag)){
				isUnit = true;
			}
		}
		if(isUnit){
			unitPanel[1].image.enabled = !unitPanel[1].image.enabled;
			unitPanel[2].image.enabled = !unitPanel[2].image.enabled;
		} else if(rockQty >= hutCost) {
			placingHut = true;
			hutPlacement = Instantiate (hutPrefab, transform.position, transform.rotation) as GameObject;
			hutPlacementManager = hutPlacement.GetComponent<HutManager>();
		}
	}

	public void displayKeepButton(){
		if(rockQty >= hutCost){
			unitPanel[0].image.sprite = unitIco[3];
			unitPanel[0].image.enabled = true;
		} else {
			unitPanel[0].image.enabled = false;
		}
		unitPanel[1].image.enabled = false;
		unitPanel[2].image.enabled = false;
	}

	public void changeUnit(PlayerUnitController unit, int id){
		// Command the unit to change. This is a placeholder.
			GameObject newUnit = Instantiate (units[id], unit.transform.position,unit.transform.rotation) as GameObject;
			playerUnitList.Add(newUnit.GetComponent<PlayerUnitController>());	
			GameObject.Destroy(unit.transform.root.gameObject);
		if (selected!= null && selected.GetInstanceID() == unit.GetInstanceID()) {
			registerClick (null);
			GameObject.Find ("Main Camera").GetComponent<CameraController> ().unitController = newUnit.GetComponent<UnitController> ();
			registerClick (newUnit.GetComponent<UnitController> ());
		} else {
			GameObject.Find ("Main Camera").GetComponent<CameraController> ().unitController = newUnit.GetComponent<UnitController> ();
		}

		return;

	}
	public void changeUnitClass(int id){
		for (int i=0; i<units.Length-1; i++) {
			if (unitPanel [id].image.sprite == unitIco [i] && unitPanel[id].image.enabled) {
				selected.ChangeClass (i);
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
	
	public void addFood(int f){
		foodQty += f;
	}

    public void selectNext()
    {
        if (getSelected() != null)
        {
            bool next = false;
            for (int i = 0; i < playerUnitList.Count; i++)
            {
                PlayerUnitController x = playerUnitList[i];
                if (next == false)
                {
                    if (selected.GetInstanceID().Equals(x.GetInstanceID()))
                    {
                        next = true;
                        if (i == playerUnitList.Count - 1)
                        {
                            registerClick(playerUnitList[0]);
                            Camera.main.GetComponent<CameraController>().selectedTransform = getSelected().transform;
                            Camera.main.GetComponent<CameraController>().seekSelected = true;
                            return;
                        }
                    }
                }
                else
                {
                    registerClick(x);
                    Camera.main.GetComponent<CameraController>().seekSelected = true;
                    return;
                }
            }
        }
        else
        {
            registerClick(playerUnitList[0]);
            Camera.main.GetComponent<CameraController>().selectedTransform = getSelected().transform;
            Camera.main.GetComponent<CameraController>().seekSelected = true;
            return;
        }

    }

    public void removeUnit(PlayerUnitController x)
    {
        playerUnitList.Remove(x);

    }
}
