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
	public UnitController attackTarget;
	public List<UnitController> attackers;
	public Slider sliderPrefab;

	protected float maxHealth;
	protected Slider healthBar;
	protected NavigationController navigationController;
	protected Vector3 navTarget = Vector3.zero;
	protected Vector3 curr_pos;
	protected bool onRails = false;
	protected List<Vector3> path;
    protected int pathRefreshCount = 0;
    protected int pathRefreshRate;
	protected float attackCharge=0f;
	protected Canvas canvas;
	protected delegate void StateDelegate();
	protected StateDelegate stateDelegate;

	// Use this for initialization
	protected virtual void Start ()
	{
		maxHealth = health;
		pathRefreshRate = 300;
		GameObject globalNav = GameObject.FindGameObjectWithTag("global_nav");
		if(globalNav != null){
			navigationController = globalNav.GetComponent<NavigationController>();
		}
		SavePosition ();

		canvas = GameObject.Find ("Canvas").GetComponent<Canvas>();
		healthBar = Instantiate (sliderPrefab,transform.position,Quaternion.Euler (0,0,0)) as Slider;
		healthBar.transform.SetParent(canvas.transform, false);
	}

	// Update is called once per frame
	protected virtual void Update () {
		if(healthBar != null){
			Vector3 target = Camera.main.WorldToViewportPoint(transform.position);
			healthBar.transform.position = new Vector3(target.x*canvas.GetComponent<RectTransform>().rect.width, target.y*canvas.GetComponent<RectTransform>().rect.height + 25f, target.z);
		}

		CheckHealth();

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
	
	public virtual void SavePosition () {
		curr_pos = transform.position;
	}
	
	protected void CheckHealth()
	{
//Kill this Unit, first resetting all attacking units to having no target
		if (health < 1)
		{
			GameObject.Destroy(gameObject);
		} else if(healthBar != null){
			healthBar.GetComponent<Slider>().value = health / maxHealth;
		}
	}
	
	void OnDestroy()
	{
		if(healthBar != null){
			GameObject.Destroy(healthBar.transform.gameObject);
		}
		foreach(UnitController a in attackers)
		{
			a.attackTarget=null;
			a.attackers.Remove(this);
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
	
	//checks to see if ready to attack again
	public virtual bool ReadyToAttack(){
		
		return attackCharge >= attackRate;
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
