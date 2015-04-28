using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour {
	public AudioClip startGame;
	private bool escHeld;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey (KeyCode.Escape)){
			if(Input.GetKeyDown (KeyCode.Escape))escHeld = !escHeld;
		} else if (!escHeld && (Input.anyKey || Input.GetMouseButton (0) || Input.GetMouseButton (1))){
			if(Application.loadedLevelName == "Title"){
				transform.gameObject.GetComponent<AudioSource>().PlayOneShot(startGame);
				Application.LoadLevel ("Terrain");
			} else {
				Application.LoadLevel ("Title");
			}
		}
	}

	void OnGUI(){
		if(escHeld){
			float w = Screen.width / 2f;
			float h = Screen.height / 2f;
			if(GUI.Button (new Rect(w-50f,h-10f,100,20),"Exit Game")){
				#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
				#else
				Application.Quit();
				#endif
			}
		}
	}
}
