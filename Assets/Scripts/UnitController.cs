using UnityEngine;
using System.Collections;

public class UnitController : MonoBehaviour {

	public float turnSpeed;
	public float moveSpeed;

	private Vector3 target = Vector3.zero;
	private Vector3 curr_pos;
	private bool onRails = false;

	// Use this for initialization
	void Start () {
		SavePosition ();
	}
	
	// Update is called once per frame
	void Update () {
		// If a target exists, move to it. Any player movement will interrupt this.
		if (target != Vector3.zero) {
			Seek ();
			SavePosition ();
		}
		
		// Handle if W and S are both held.
		int dir_motion = 0;
		if (Input.GetKey (KeyCode.W))
			dir_motion++;
		if (Input.GetKey (KeyCode.S))
			dir_motion--;
			
		// Do motion. Remove target if one existed!
		if (dir_motion != 0){
			transform.Translate (dir_motion * Vector3.forward * moveSpeed * Time.deltaTime);
			target = Vector3.zero;
			setRails (false);
		}
		// Handle if A and D are both held.
		int dir_rotation = 0;
		if (Input.GetKey (KeyCode.A))
			dir_rotation++;
		if (Input.GetKey (KeyCode.D))
			dir_rotation--;

		// Do rotation. Remove target if one existed!
		if (dir_rotation != 0){
			transform.Rotate(dir_rotation * Vector3.down * turnSpeed * 10 * Time.deltaTime);
			target = Vector3.zero;
			setRails (false);
		}
		SavePosition ();
	}

	void Seek () {
		float distance = Vector3.Distance (curr_pos,target);
		
		if(distance >= 0.01) {
			//transform.position = Vector3.MoveTowards (curr_pos, target, moveSpeed * Time.deltaTime);
			Debug.DrawLine (transform.position,target, Color.magenta);
			// Don't forget to look where you're walking!
			//float angle = Mathf.Rad2Deg * Mathf.Atan2 (target.y - curr_pos.z, target.x - curr_pos.x);
			//transform.rotation = Quaternion.Euler (0f, 0f, angle);
		} else {
			// We've reached the target. Clear out all memory of it and await further instructions.
			target = Vector3.zero;
		}
	}
	
	void SavePosition () {
		curr_pos = transform.position;
	}

	public void setTarget(Vector3 target){
		this.target = target;
	}

	public void setRails(bool railSetting){
		onRails = railSetting;
	}
	
	public bool isOnRails(){
		return onRails;
	}
}
