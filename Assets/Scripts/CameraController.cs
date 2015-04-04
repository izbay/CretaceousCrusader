using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public NavigationController navigationController;
	public SelectionController selectionController;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		registerClicks();
	}

	private void registerClicks(){
		if(Input.GetMouseButtonDown (0)){
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)){
				int layerHit = hit.transform.gameObject.layer;
				if(layerHit==LayerMask.NameToLayer("Water")){
					navigationController.registerClick (hit.point);
				} else if (layerHit==LayerMask.NameToLayer("Unit")){
					selectionController.registerClick(hit.transform.gameObject.GetComponent<UnitController>());
				} else {
					selectionController.registerClick (null);
				}
			}
		}
	}
}
