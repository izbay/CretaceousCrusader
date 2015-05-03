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
    public Transform selectedTransform;
	public bool seekKeep;
    public bool seekSelected;

	private GameObject Keep;
	private Vector3 oldPos;
	private Vector3 panOrigin;
	private KeepManager selectionController;
	private bool mapActive = false;
	private Camera mapCamera;

	// Use this for initialization
	void Start () {
		Keep = GameObject.FindGameObjectWithTag("Player");
		selectionController = Keep.GetComponent<KeepManager>();
		seekKeep = true;
	    seekSelected = false;
		mapCamera = GameObject.Find ("Map Camera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
		registerClicks();
		moveCamera();
	}

	private void registerClicks(){
		//Left Mouse Button
		if(!selectionController.placingHut){
			if(Input.GetMouseButtonDown (0)){
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit)){
					int layerHit = hit.transform.gameObject.layer;
					Keep.GetComponentInChildren<Projector>().enabled = false;
					if (layerHit==LayerMask.NameToLayer("Unit")){
						unitController= hit.transform.root.GetComponent<UnitController>();
						selectionController.registerClick(unitController);
					}else if(!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()){
						selectionController.registerClick (null);
						if(layerHit == LayerMask.NameToLayer("Keep")){
							Keep.GetComponentInChildren<Projector>().enabled = true;
							selectionController.displayKeepButton();
						}
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
						if(layerHit==LayerMask.NameToLayer("Water") && !(unitController is FarmerController)){
							navigationController.registerClick (hit.point);
							unitController.assistTarget=null;
							unitController.attackTarget=null;
						}else if(layerHit==LayerMask.NameToLayer("Enemy Unit") && !(unitController is FarmerController)){
							unitController.assistTarget=null;
							unitController.registerClick(hit.transform.gameObject.GetComponent<UnitController>());
						}else if(layerHit == LayerMask.NameToLayer("Stone")){
							if(unitController is QuarrierController){
								unitController.assistTarget=null;
								unitController.registerClick(hit.transform.gameObject.GetComponent<StoneController>());
							}
						}else if(layerHit == LayerMask.NameToLayer("Keep")){
							if(unitController is QuarrierController){
								navigationController.registerClick(Keep.transform.position);
								(unitController as QuarrierController).ReturnResources();
								unitController.assistTarget=null;
								unitController.attackTarget=null;
							}
						}else if(layerHit == LayerMask.NameToLayer ("Unit") && !(unitController is FarmerController)){
							unitController.registerClick(hit.transform.gameObject.GetComponent<UnitController>());
							unitController = hit.transform.gameObject.GetComponent<UnitController>();
							selectionController.registerClick(unitController);
							//unitController.attackTarget = null;
							//unitController.assistTarget = hit.transform.gameObject.GetComponent<UnitController>();
						}
					}
				}
			}
			if(Input.GetKeyDown (KeyCode.M)){
				mapActive = !mapActive;
				mapCamera.enabled = mapActive;
			}
			if(Input.GetKeyDown(KeyCode.Alpha1) && unitController != null){
				PlayerUnitController puc = unitController as PlayerUnitController;
				puc.ChangeClass(0);
			}
			if(Input.GetKeyDown(KeyCode.Alpha2) && unitController != null){
				PlayerUnitController puc = unitController as PlayerUnitController;
				puc.ChangeClass(1);
			}
			if(Input.GetKeyDown(KeyCode.Alpha3) && unitController != null){
				PlayerUnitController puc = unitController as PlayerUnitController;
				puc.ChangeClass(2);
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
		    seekSelected = false;
			oldPos = transform.position;
			panOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition);
		}
		
		if (Input.GetMouseButton (2)) {
			seekKeep=false;
            seekSelected = false;
			Vector3 pos = Camera.main.ScreenToViewportPoint (Input.mousePosition) - panOrigin;
			pos.z = pos.y;
			pos.y = 0;
			transform.position = oldPos - pos * dragSpeed;
		} 
		//starts to seek the Keep Object if H is hit
		else if (Input.GetKeyDown ("h")) {
			seekKeep=true;
        }
        else if (Input.GetKeyDown("g") && !seekKeep && selectionController.getSelected() != null)
        {
            seekSelected = true;
            selectedTransform = selectionController.getSelected().transform;
        }
        else if (Input.GetKeyDown("n") && !seekKeep)
        {
            selectionController.selectNext();
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

			if (destination.x < -30)
			{
				destination.x = -30;
			} else if (destination.x > 3030)
			{
				destination.x = 3030;
			}

			if (destination.z < -100)
			{
				destination.z = -100;
			} else if (destination.z > 2900)
			{
				destination.z = 2900;
			}
			
			//if a change in position is detected perform the necessary update
			if(destination != origin)
			{
				seekKeep=false;
                seekSelected = false;
				Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * scrollSpeed);
			}

			if (seekKeep) {
				if(keepTransform != null){
					destination = keepTransform.position;
					destination.y=Camera.main.transform.position.y;
                    destination.z -= 100f;
					if (Vector3.Distance(destination,origin)> 0f) {
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
            if (seekSelected)
            {
                selectedTransform = selectionController.getSelected().transform;
                if (selectionController != null)
                {
                    destination = selectedTransform.position;
                    destination.y = Camera.main.transform.position.y;
                    destination.z -= 100f;
                    if (Vector3.Distance(destination, origin) > 0f)
                    {
                        Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * scrollSpeed * 2);
                    }
                    else
                    {
                        seekSelected = false;
                    }
                }
                else
                {
                    seekSelected = false;
                }
            }

		}
	}
}
