using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour {
	public AudioClip startGame;
	private bool escMenu;
	private GameObject escButton;

	// Use this for initialization
	void Start () {
		escButton = GameObject.Find ("Quit");
		escButton.SetActive(false);
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
	}

	public void quitGame(){
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
}
