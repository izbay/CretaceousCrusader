using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitController : MonoBehaviour {

	public int health;
	public int energy;
	public int maxCarry;
	public int currentCarry;
	public int attackStr;
	public float attackRate;
	public float attackRange;
	public float turnSpeed;
	public float moveSpeed;
	public bool selectable=true;


	protected NavigationController navigationController;
	protected Vector3 target = Vector3.zero;
	protected Vector3 curr_pos;
	protected bool onRails = false;
	protected List<Vector3> path;
	protected int counter = 0;
	public  UnitController Atarget;
	protected float attackCharge=0;
	public List<UnitController> attackers;

	// Use this for initialization
	protected virtual void Start () {
		SavePosition ();
		navigationController = GameObject.FindGameObjectWithTag("global_nav").GetComponent<NavigationController>();
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		// If a target exists, move to it.
		if (target != Vector3.zero) {	
			Seek ();
			SavePosition ();
		}

		// Check path with every update.

		if(path != null && onRails){
			setTarget(path[1]);
			if(counter == 100){
				path = navigationController.getPath (transform.position, path[path.Count-1]);
				counter = 0;
			} else {
				Vector3 start = transform.position;
				Vector3 end = path[path.Count-1];
				List<Vector3> returnPath = navigationController.quickScanPath (start, end);
				if(!navigationController.pathIsInvalid (returnPath)){
					path = returnPath;
				}
				counter++;
			}
		} else {
			path = null;
			setRails (false);
		}
		if(Atarget!=null){
			BeginAttack();
		}
		if (attackCharge < attackRate) {
			attackCharge += Time.deltaTime;
		}
	}

	protected virtual void Seek () {
		//Debug.DrawLine (curr_pos, target, Color.magenta);

		float distance = Vector3.Distance (curr_pos,target);
		if (Atarget == null) {
			if (distance >= 1f) {

				Vector3 targetDir = new Vector3 (target.x, 0, target.z) - new Vector3 (curr_pos.x, 0, curr_pos.z);
				Quaternion targetRotation = Quaternion.LookRotation (targetDir);

				transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed);
				float angle = Vector3.Angle (targetDir, transform.forward);
				if (angle < 3) {
					rigidbody.AddForce (transform.forward * moveSpeed * 500);
				}
			} else {
				// We've reached the target. Clear out all memory of it and await further instructions.
				target = Vector3.zero;
				setRails (false);
			}
		} else {
			if(distance >=attackRange){
				Vector3 targetDir = new Vector3 (target.x, 0, target.z) - new Vector3 (curr_pos.x, 0, curr_pos.z);
				Quaternion targetRotation = Quaternion.LookRotation (targetDir);
				
				transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed);
				float angle = Vector3.Angle (targetDir, transform.forward);
				if (angle < 3) {
					rigidbody.AddForce (transform.forward * moveSpeed * 500);
				}
			}
			else{
				// We've reached the target. Clear out all memory of it and await further instructions.
				target = Vector3.zero;
				setRails (false);
			}
		}
	}

	void SavePosition () {
		curr_pos = transform.position;
	}

	public void setPath (List<Vector3> path){
		this.path = path;
		setRails (true);
	}

	public void setTarget(Vector3 target){
		this.target = target;
	}

	public void setATarget(UnitController target){
		this.Atarget = target;
	}

	public void setRails(bool railSetting){
		onRails = railSetting;
		Animator anim = transform.Find("AnimationContainer").GetComponent<Animator>();
		anim.SetBool ("onRails",railSetting);
	}


	public void registerClick(UnitController unit){
		if (unit.selectable == true) {
			target=unit.transform.position;
		} else {
			Atarget = unit;
			target=unit.transform.position;
		}
	}


	protected virtual void BeginAttack() {
		if (TargetInRange () && ReadyToAttack ()) {
			Attack ();

		} else {
			AdjustPosition ();
		}
	}
	//checks to see if enemy is within range to attack
	public bool TargetInRange(){
		Vector3 targetLocation = Atarget.transform.position;
		Vector3 direction = targetLocation - transform.position;
		if(direction.sqrMagnitude < attackRange * attackRange) {
			return true;
		}
		return false;
	}
	//checks to see if ready to attack again
	public bool ReadyToAttack(){
		if (attackCharge >= attackRate) {
			return true;
		} else {
			return false;
		}

	}
	//move towards attacker
	public void AdjustPosition(){
		target = Atarget.transform.position;
		Seek ();
	}

	//Perform Attack
	public void Attack(){
		attackCharge = 0;
		Atarget.Hit (gameObject.GetComponent<UnitController>());
	}
	
	//Take Damage
	public void Hit(UnitController attacker){
		health -= attacker.attackStr;
		if (Atarget == null) {
			Atarget=attacker;
		}
		if(!attackers.Contains (attacker)){
			attackers.Add(attacker);
		}
		//Kill this Unit, first resetting all attacking units to having no target
		if (health < 1) {
			foreach(UnitController a in attackers){
				a.Atarget=null;
				a.attackers.Remove(this);

			}
			GameObject.Destroy(gameObject);
		}
	}
	/*
	//Perform Attack
	public void Attack(){
		attackCharge = 0;
		Atarget.Hit (attackStr);
	}
	
	//Take Damage
	public void Hit(int damage){
		health -= damage;
		//Kill this Unit
		if (health < 1) {
			GameObject.Destroy(gameObject);
		}
	}*/
}
