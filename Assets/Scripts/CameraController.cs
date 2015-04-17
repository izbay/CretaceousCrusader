using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float minCameraHeight = 10;
	public float maxCameraHeight = 200;
	
	public NavigationController navigationController;
	public UnitController unitController;

	public float scrollSpeed;
	public float dragSpeed;
	public int scrollWidth;

	public Transform keepTransform;
	public bool seekKeep;

	private GameObject Keep;
	private Vector3 oldPos;
	private Vector3 panOrigin;
	private KeepManager selectionController;
	// Use this for initialization
	void Start () {
		Keep = GameObject.FindGameObjectWithTag("Player");
		selectionController = Keep.GetComponent<KeepManager>();
		seekKeep = true;
	}
	
	// Update is called once per frame
	void Update () {
		registerClicks();
		moveCamera();
	}

	private void registerClicks(){
		//Left Mouse Button
		if(Input.GetMouseButtonDown (0)){
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)){
				int layerHit = hit.transform.gameObject.layer;
				if (layerHit==LayerMask.NameToLayer("Unit")){
					unitController= hit.transform.root.GetComponent<UnitController>();
					selectionController.registerClick(unitController);
				}else if(!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()){
					selectionController.registerClick (null);
				}
			}
		}
		//Right Mouse Button
		if (Input.GetMouseButton (1)) {
			if(unitController!=null){
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit)){
					int layerHit = hit.transform.gameObject.layer;
					if(layerHit==LayerMask.NameToLayer("Water")){
						navigationController.registerClick (hit.point);
						unitController.Atarget=null;
					}else if(layerHit==LayerMask.NameToLayer("Enemy Unit")){
						unitController.registerClick(hit.transform.gameObject.GetComponent<UnitController>());
					}else if(layerHit == LayerMask.NameToLayer("Stone")){
						if(unitController is QuarrierController){
							unitController.registerClick(hit.transform.gameObject.GetComponent<StoneController>());
						}
					}else if(layerHit == LayerMask.NameToLayer("Keep")){
						if(unitController is QuarrierController){
							unitController.setTarget(Keep.transform.position);
						}
					}else {
					}
				}
			}
		}
	}

	private void moveCamera()
	{
		if(UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()){
			return;
		}
		// Handlie click-and-drag
		if(Input.GetMouseButtonDown(2))
		{
			seekKeep=false;
			oldPos = transform.position;
			panOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition);
		}
		
		if (Input.GetMouseButton (2)) {
			seekKeep=false;
			Vector3 pos = Camera.main.ScreenToViewportPoint (Input.mousePosition) - panOrigin;
			pos.z = pos.y;
			pos.y = 0;
			transform.position = oldPos - pos * dragSpeed;
		} 
		//starts to seek the Keep Object if H is hit
		else if (Input.GetKeyDown ("h")) {
			seekKeep=true;
		} 
		//seeks out the Keep object
		/*else if (seekKeep) {
			Vector3 origin = Camera.main.transform.position;
			if(Keep != null){
				Vector3 destination = Keep.transform.position;
				destination.y=Camera.main.transform.position.y;
				if (Vector3.Distance(destination,origin)> 175f) {
					Camera.main.transform.position = Vector3.MoveTowards (origin, destination, Time.deltaTime * scrollSpeed*2);
				}
				else{
					seekKeep=false;
				}
			}
			else{
				seekKeep=false;
			}
		}*/
		// Handle camera panning when mouse is near edge of screen
		else
		{
			float xpos = Input.mousePosition.x;
			float ypos = Input.mousePosition.y;
			
			Vector3 movement = new Vector3(0,0,0);
			
			//horizontal camera movement
			if (xpos >= 0 && xpos < scrollWidth)
			{
				movement.x -= scrollSpeed;
			}
			else if (xpos <= Screen.width && xpos > Screen.width - scrollWidth)
			{
				movement.x += scrollSpeed;
			}
			
			//vertical camera movement
			if(ypos >= 0 && ypos < scrollWidth)
			{
				movement.z -= scrollSpeed;
			}
			else if (ypos <= Screen.height && ypos > Screen.height - scrollWidth)
			{
				movement.z += scrollSpeed;
			}
			
			//make sure movement is in the direction the camera is pointing
			//but ignore the vertical tilt of the camera to get sensible scrolling
			movement = transform.TransformDirection(movement);
			movement.y = 0;
			
			// Allow zooming with the scroll wheel
			movement += transform.forward * scrollSpeed * Input.GetAxis ("Mouse ScrollWheel");
			//calculate desired camera position based on received input
			Vector3 origin = Camera.main.transform.position;
			Vector3 destination = origin + movement;
			
			// Keep camera between min and max height
			if (destination.y > maxCameraHeight)
			{
				destination.y = maxCameraHeight;
			}
			else if (destination.y < minCameraHeight)
			{
				destination.y = minCameraHeight;
			}
			
			//if a change in position is detected perform the necessary update
			if(destination != origin)
			{
				seekKeep=false;
				Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * scrollSpeed);
			}

			if (seekKeep) {
				if(keepTransform != null){
					destination = keepTransform.position;
					destination.y=Camera.main.transform.position.y;
					if (Vector3.Distance(destination,origin)> 175f) {
						Camera.main.transform.position = Vector3.MoveTowards (origin, destination, Time.deltaTime * scrollSpeed*2);
					}
					else{
						seekKeep=false;
					}
				}
				else{
					seekKeep=false;
				}
			}

		}
	}
}
