using UnityEngine;
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
	public  UnitController attackTarget;
	public List<UnitController> attackers;
	
	protected NavigationController navigationController;
	protected Vector3 navTarget = Vector3.zero;
	protected Vector3 curr_pos;
	protected bool onRails = false;
	protected List<Vector3> path;
    protected int pathRefreshCount = 0;
    protected int pathRefreshRate;
	protected float attackCharge=0f;

	protected delegate void StateDelegate();
	protected StateDelegate stateDelegate;

	// Use this for initialization
	protected virtual void Start ()
	{
	    pathRefreshRate = 300;
		navigationController = GameObject.FindGameObjectWithTag("global_nav").GetComponent<NavigationController>();
		SavePosition ();
	}
	
	// Update is called once per frame
	protected virtual void Update () {

		//Kill this Unit, first resetting all attacking units to having no target
		if (health < 1) {
			foreach(UnitController a in attackers){
				a.attackTarget=null;
				a.attackers.Remove(this);
				
			}
			GameObject.Destroy(gameObject);
		}

		// If a target exists, move to it.
		if (navTarget != Vector3.zero) {	
			Seek ();
		}
		SavePosition ();

		// Check path with every update.

		if(path != null && path.Count > 1 && onRails){
			setTarget(path[1]);
            if (pathRefreshCount == pathRefreshRate)
            {
				navigationController.registerClick(this, path[path.Count-1]);
                pathRefreshCount = 0;
			} else {
				List<Vector3> returnPath = navigationController.quickScanPath (transform.position, path[path.Count-1]);
				if(!navigationController.pathIsInvalid (returnPath)){
					path = returnPath;
				}
				/**if(path != null){
					drawPath(path);
				}*/
                pathRefreshCount++;
			}
		} else {
			path = null;
			navTarget = Vector3.zero;
			setRails (false);
		}
		if(attackTarget!=null){
			BeginAttack();
		}
		if (attackCharge < attackRate) {
			attackCharge += Time.deltaTime;
		}
	
	}

	protected virtual void Seek () {
		//Debug.DrawLine (curr_pos, navTarget, Color.magenta);
		float distance = Vector3.Distance (curr_pos,navTarget);
		if (distance >= 1f || (attackTarget != null && distance >= attackRange)) {

			Vector3 targetDir = new Vector3 (navTarget.x, 0, navTarget.z) - new Vector3 (curr_pos.x, 0, curr_pos.z);
			Quaternion targetRotation = Quaternion.LookRotation (targetDir);

			transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed);
			float angle = Vector3.Angle (targetDir, transform.forward);
			if (angle < 3) {
				rigidbody.AddForce (transform.forward * moveSpeed * 500);
			}
		} else {
			// We've reached the target. Clear out all memory of it and await further instructions.
			navTarget = Vector3.zero;
			setRails (false);
		}
	}

	public virtual void SavePosition () {
		curr_pos = transform.position;
	}

	public virtual void setPath (List<Vector3> path){
		if(this.path == null || path != null){
			this.path = path;
			setRails (true);
		}
	}

	public virtual void setTarget(Vector3 target){
		this.navTarget = target;
	}

	public virtual void setATarget(UnitController target){
		this.attackTarget = target;
	}

	public virtual void setRails(bool railSetting){
		onRails = railSetting;
		Animator anim = transform.Find("AnimationContainer").GetComponent<Animator>();
		anim.SetBool ("onRails",railSetting);
	}


	public virtual void registerClick(UnitController unit){
		if (unit.selectable == true) {
			navTarget=unit.transform.position;
		} else {
			attackTarget = unit;
			navigationController.registerClick(this, attackTarget.transform.position);
			AdjustPosition ();
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

		return Vector3.Distance (transform.position, attackTarget.transform.position) < attackRange;
	}
	
	//checks to see if ready to attack again
	public virtual bool ReadyToAttack(){
		
		return attackCharge >= attackRate;
	}
	
	//move towards attacker
	public virtual void AdjustPosition(){
		if (path != null && onRails) {
			if(path.Count > 1){
				setTarget (path [1]);
			}
            if (pathRefreshCount == pathRefreshRate)
            {
				navigationController.registerClick(this, attackTarget.transform.position);
                pathRefreshCount = 0;
			} else {
				List<Vector3> returnPath = navigationController.quickScanPath (curr_pos, path [path.Count - 1]);
				if (!navigationController.pathIsInvalid (returnPath)) {
					path = returnPath;
				}
                pathRefreshCount++;
			}
		} else {
			//navigationController.registerClick (this, attackTarget.transform.position);
			path = null;
			navTarget = Vector3.zero;
			setRails (false);
		}
	}

	//Perform Attack
	public virtual void Attack(){
		attackCharge = 0;
		attackTarget.Hit (gameObject.GetComponent<UnitController>());
	}
	
	//Take Damage
	public virtual void Hit(UnitController attacker){

		health -= attacker.attackStr;


		//healthBar.value -= attacker.attackStr;
		//healthBar.value = health;

		if (attackTarget == null) {
			attackTarget=attacker;
		}
		if(!attackers.Contains (attacker)){
			attackers.Add(attacker);
		}
		//unitHealth.size = health / 100f;
	}

	void drawPath(List<Vector3> path){
		for(int i=1; i<path.Count; i++){
			Debug.DrawLine (path[i-1],path[i],Color.magenta);
		}
	}
}
