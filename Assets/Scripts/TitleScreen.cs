using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleScreen : MonoBehaviour {
	public AudioClip startGame;
	private bool escMenu;
	private GameObject escButton;
	private StatTracker statTracker;

	// Use this for initialization
	void Start () {
		escButton = GameObject.Find ("Quit");
		escButton.SetActive(false);
		try{statTracker = GameObject.FindGameObjectWithTag("stat_tracker").GetComponent<StatTracker>();}catch{}
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey (KeyCode.Escape)){
			if(Input.GetKeyDown (KeyCode.Escape)){
				escMenu = !escMenu;
				escButton.SetActive(escMenu);
			}
		} else if (!escMenu && (Input.anyKey || Input.GetMouseButton (0) || Input.GetMouseButton (1))){
			if(Application.loadedLevelName == "Title"){
				transform.gameObject.GetComponent<AudioSource>().PlayOneShot(startGame);
				Application.LoadLevel ("Terrain");
			} else {
				Application.LoadLevel ("Title");
			}
		}

		DisplayStats();
	}

	private void DisplayStats(){
		if(GameObject.Find ("Stats") == null || statTracker == null){
			return;
		} else {
			Text UItext = GameObject.Find ("Stats").GetComponent<Text>();

			if(UItext.text == ""){
				string newText = "Time Elapsed: "+FormatTime()+"\n";
				newText = newText + "Food Consumed: "+statTracker.foodConsumed.ToString ("F1")+"\n";
				newText = newText + "Dinos Slain: "+statTracker.dinosKilled.ToString ()+"\n"; 
				newText = newText + "Units Lost: "+statTracker.unitsKilled.ToString ()+"\n"; 
				newText = newText + "Largest Population: "+statTracker.maxUnits.ToString ()+"\n"; 
				if(Application.loadedLevelName == "Lose"){
					newText = newText + "\nLord of Rex at "+statTracker.LdinoHealth.ToString ("F0")+"%" ;
				}

				UItext.text = newText;
			}
		}
	}

	private string FormatTime(){
		int time = Mathf.RoundToInt(statTracker.time);
		// hours
		string returnVal = (time/3600).ToString ("00");
		// minutes
		returnVal = returnVal+":"+ ((time%3600)/60).ToString ("00");
		// seconds
		returnVal = returnVal+":"+ (time % 60).ToString("00");

		return returnVal;

	}


	public void quitGame(){
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
}
