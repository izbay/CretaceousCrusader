using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UnitController : MonoBehaviour
{
	public float health = 100f;
	
	public int attackStr;
	public float attackRate;
	public float attackRange;
	public UnitController attackTarget;
	public UnitController assistTarget;
	public List<UnitController> attackers;
	
	public float turnSpeed;
	public float moveSpeed;
	
	public bool selectable;
	
	public Slider sliderPrefab;

	public string stateIndicator;

	protected float followRange = 5f;
	protected float maxHealth;
	protected Slider healthBar;
	
	protected NavigationController navigationController;
	protected Vector3 navTarget = Vector3.zero;
	protected float pathRefreshCount = 0;
	protected float pathRefreshRate;
	protected List<Vector3> path;
	
	protected bool onRails = false;
    
	protected float attackCharge=0f;
	
	protected Canvas canvas;
	
	protected KeepManager keep;
	
	protected delegate void StateDelegate();
	protected StateDelegate stateDelegate;
	protected AnimationController anim;
	protected TerrainBuilder tb;

	protected virtual void Start ()
	{
		keep = GameObject.FindObjectOfType<KeepManager>();
		anim = transform.GetComponentInChildren<AnimationController>();
		tb = GameObject.Find ("Terrain").GetComponent<TerrainBuilder>();
		maxHealth = health;
		pathRefreshRate = 5f;
		GameObject globalNav = GameObject.FindGameObjectWithTag("global_nav");
		
		if(globalNav != null)
		{
			navigationController = globalNav.GetComponent<NavigationController>();
		}

		canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		healthBar = Instantiate(sliderPrefab, transform.position, Quaternion.Euler(0,0,0)) as Slider;
		healthBar.transform.SetParent(canvas.transform, false);
		setRails(false);
		stateDelegate = IdleState;
	}

	protected virtual void Update()
	{
		CheckHealth();
		
		if (stateDelegate != null)
		{
			stateDelegate();
		}
		
		if (attackCharge < attackRate)
		{
			attackCharge += Time.deltaTime;
		}
	}
	
	protected virtual void IdleState()
	{
		stateIndicator = "idle";
		setRails (false);
		if (assistTarget != null)// && !LeaderInRange())
		{
			stateDelegate = FollowingState;
		}
		else if (navTarget != Vector3.zero || path != null)//() && assistTarget == null )
		{
			stateDelegate = MovingState;
		}
		else
		{
			UpdateAssistTargetting();
		}
	}

	protected virtual void MovingState()
	{
		stateIndicator = "moving";
		setRails (true);
		Move();
		if (path == null && navTarget == Vector3.zero)
		{
			stateDelegate = IdleState;
		}
	}

	protected virtual void FollowingState()
	{
		stateIndicator="following";
		if (assistTarget == null || LeaderInRange())
		{
			stateDelegate = IdleState;
		}
		else
		{
			UpdateAssistTargetting();
			Move ();
		}
	}

	protected virtual void UpdateAssistTargetting()
	{
		if(assistTarget != null)
		{
			if(assistTarget.attackTarget != null)
			{
				setAttackTarget(assistTarget.attackTarget);
			}
			else
			{
				attackTarget = null;
			}
		}
	}

	protected virtual void AttackingState()
	{
		stateIndicator = "attacking";
		UpdateAssistTargetting();
		if (attackTarget == null)
		{
			stateDelegate = IdleState;
		}
		else
		{
			if (TargetInRange())
			{
				// TODO use coroutine for attack cooldown
				if (ReadyToAttack())
				{
					Attack();
				}
			}
			else
			{
				// Move towards the enemy
				//navigationController.registerClick(this, attackTarget.transform.position);
				Move();
			}
		}
	}
	
	public virtual void registerClick(UnitController unit)
	{
		if(unit is FarmerController)
		{
			navTarget = unit.transform.position;
		}
		else if (unit.selectable == true)
		{
			assistTarget = unit;
			attackTarget = null;
			navigationController.registerClick(this, assistTarget.transform.position);
			stateDelegate = FollowingState;
		}
		
		else
		{
			attackTarget = unit;
			navigationController.registerClick(this, attackTarget.transform.position);
			stateDelegate = AttackingState;
		}
	}
	
	protected virtual void Move()
	{
		// If a target exists, move to it.
		if (navTarget != Vector3.zero)
		{	
			Seek();
		}
		
		// Check path with every update.
		if(path != null && path.Count > 1 && onRails)
		{
			setNavTarget(path[1]);
			if (pathRefreshCount >= pathRefreshRate)
			{
				if(attackTarget != null){
					navigationController.registerClick(this, attackTarget.transform.position);
				} else if(assistTarget != null){
					navigationController.registerClick(this, assistTarget.transform.position);
				} else {
					navigationController.registerClick(this, path[path.Count - 1]);
				}
				pathRefreshCount = 0f;
			}
			else
			{
				List<Vector3> returnPath = navigationController.quickScanPath(transform.position, path[path.Count - 1]);
				if (!navigationController.pathIsInvalid(returnPath))
				{
					path = returnPath;
				}
				
				/**if(path != null){
					drawPath(path);
				}*/
				if(attackTarget != null && Vector3.Distance (transform.position,attackTarget.transform.position) < 100f){
					pathRefreshCount += Time.deltaTime * 3f;
				} else {
					pathRefreshCount += Time.deltaTime;
				}
			}
		}
		else
		{
			if(assistTarget != null)
			{
				Vector3 targetPos = assistTarget.transform.position-(transform.position-assistTarget.transform.position).normalized*followRange;
				if (pathRefreshCount >= pathRefreshRate && assistTarget != null && !LeaderInRange())
				{
					navigationController.registerClick(this, targetPos);
					pathRefreshCount = 0f;
				}
				else
				{
					if(Vector3.Distance(transform.position, targetPos) > 250f)
					{
						pathRefreshCount += Time.deltaTime * 3f;
					}
					else
					{
						pathRefreshCount +=  Time.deltaTime * 10f;
					}
				}
			}
			else
			{
				path = null;
				navTarget = Vector3.zero;
				setRails(false);
			}
		}
	}
	
	protected void CheckHealth()
	{
		if (health <= 0)
		{
			anim.disappearAnim(gameObject);
			return;
		}
		
		if (healthBar != null)
		{
			healthBar.GetComponent<Slider>().value = health / maxHealth;
			Vector3 target = Camera.main.WorldToViewportPoint(transform.position);
			healthBar.transform.position = new Vector3(target.x * canvas.GetComponent<RectTransform>().rect.width,
				target.y*canvas.GetComponent<RectTransform>().rect.height + 50f, target.z);
		}
	}
	
	protected virtual void OnDestroy()
	{
		if (healthBar != null)
		{
			GameObject.Destroy(healthBar.transform.gameObject);
		}
		
		foreach (UnitController a in attackers)
		{
			a.attackTarget = null;
			a.attackers.Remove(this);
		}
	}

	protected virtual void Seek ()
	{
//		Debug.DrawLine (transform.position, navTarget, Color.magenta);
		
		float distance = Vector3.Distance (new Vector3(transform.position.x,0,transform.position.z), new Vector3(navTarget.x,0,navTarget.z));
		
		if (distance >= 1f || (attackTarget != null && distance >= attackRange))
		{
			
			Vector3 targetDir = new Vector3 (navTarget.x, 0, navTarget.z) - new Vector3 (transform.position.x, 0, transform.position.z);
			Quaternion targetRotation = Quaternion.LookRotation (targetDir);
			
			transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed);
			float angle = Vector3.Angle (targetDir, transform.forward);
			if (angle < 3)
			{
				rigidbody.AddForce (transform.forward * moveSpeed * 500);
			}
		}
		
		else
		{
			// We've reached the target.
			navTarget = Vector3.zero;
			setRails (false);
		}
	}
	
	public virtual void setPath (List<Vector3> path)
	{
		if (this.path == null || path != null)
		{
			this.path = path;
			setRails(true);
		}
	}

	public virtual void setNavTarget(Vector3 target)
	{
		this.navTarget = target;
	}

	public virtual void setAttackTarget(UnitController target)
	{
		this.attackTarget = target;
		stateDelegate = AttackingState;
	}

	public virtual void setRails(bool railSetting)
	{
		onRails = railSetting;
		anim.setRails(railSetting);
	}

	public virtual bool LeaderInRange()
	{
		return Vector3.Distance (transform.position, assistTarget.transform.position) < followRange;
	}

	public virtual bool TargetInRange()
	{
		return Vector3.Distance(transform.position, attackTarget.transform.position) < attackRange;
	}
	
	public virtual bool ReadyToAttack()
	{
		return attackCharge >= attackRate;
	}
	
	//Perform Attack
	public virtual void Attack()
	{
		attackCharge = 0;
		anim.attackAnim();
		attackTarget.Hit(gameObject.GetComponent<UnitController>());
	}
	
	//Take Damage
	public virtual void Hit(UnitController attacker)
	{
		health -= attacker.attackStr;
		anim.flinchAnim();
		if (attackTarget == null)
		{
			attackTarget = attacker;
			stateDelegate = AttackingState;
		}
		
		if (!attackers.Contains(attacker))
		{
			attackers.Add(attacker);
		}
	}
	
	void drawPath(List<Vector3> path)
	{
		for (int i = 1; i < path.Count; i++)
		{
			Debug.DrawLine(path[i - 1],path[i], Color.magenta);
		}
	}
}
