﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UnitController : MonoBehaviour {
	public float health = 100f;
	public int attackStr;
	public float attackRate;
	public float attackRange;
	public float turnSpeed;
	public float moveSpeed;
	public bool selectable;
	public Scrollbar unitHealth;
	
	protected NavigationController navigationController;
	protected Vector3 target = Vector3.zero;
	protected Vector3 curr_pos;
	protected bool onRails = false;
	protected List<Vector3> path;
	protected int counter = 0;
	public  UnitController Atarget;
	protected float attackCharge=0f;
	public List<UnitController> attackers;

	protected delegate void StateDelegate();
	protected StateDelegate stateDelegate;

	// Use this for initialization
	protected virtual void Start () {
		navigationController = GameObject.FindGameObjectWithTag("global_nav").GetComponent<NavigationController>();
		SavePosition ();
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		// If a target exists, move to it.
		if (target != Vector3.zero) {	
			Seek ();
		}
		SavePosition ();

		// Check path with every update.

		if(path != null && onRails){
			setTarget(path[1]);
			if(counter == 300){
				path = navigationController.getPath (curr_pos, path[path.Count-1]);
				counter = 0;
			} else {
				List<Vector3> returnPath = navigationController.quickScanPath (curr_pos, path[path.Count-1]);
				if(!navigationController.pathIsInvalid (returnPath)){
					path = returnPath;
				}
				counter++;
			}
		} else {
			path = null;
			//target = Vector3.zero;
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

	public virtual void SavePosition () {
		curr_pos = transform.position;
	}

	public virtual void setPath (List<Vector3> path){
		this.path = path;
		setRails (true);
	}

	public virtual void setTarget(Vector3 target){
		this.target = target;
	}

	public virtual void setATarget(UnitController target){
		this.Atarget = target;
	}

	public virtual void setRails(bool railSetting){
		onRails = railSetting;
		Animator anim = transform.Find("AnimationContainer").GetComponent<Animator>();
		anim.SetBool ("onRails",railSetting);
	}


	public virtual void registerClick(UnitController unit){
		if (unit.selectable == true) {
			target=unit.transform.position;
		} else {
			Atarget = unit;
			navigationController.registerClick(unit.transform.position);
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
	public virtual bool TargetInRange(){
		Vector3 targetLocation = Atarget.transform.position;
		Vector3 direction = targetLocation - transform.position;
		if(direction.sqrMagnitude < attackRange * attackRange) {
			return true;
		}
		return false;
	}
	//checks to see if ready to attack again
	public virtual bool ReadyToAttack(){
		if (attackCharge >= attackRate) {
			return true;
		} else {
			return false;
		}

	}
	//move towards attacker
	public virtual void AdjustPosition(){
		navigationController.registerClick(Atarget.transform.position);
	}

	//Perform Attack
	public virtual void Attack(){
		attackCharge = 0;
		Atarget.Hit (gameObject.GetComponent<UnitController>());
	}
	
	//Take Damage
	public virtual void Hit(UnitController attacker){

		health -= attacker.attackStr;


		//healthBar.value -= attacker.attackStr;
		//healthBar.value = health;

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
		unitHealth.size = health / 100f;
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
