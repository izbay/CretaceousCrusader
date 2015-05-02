using UnityEngine;
using System.Collections;

public class AnimationController : MonoBehaviour {
	public AudioClip step;
	public AudioClip roar;
	public AudioClip hit;

	private Animator anim;
	private AudioSource aux;
	private GameObject obj;

	private float roarTick = 0;
	private float nextRoar = -1;
	private StatTracker statTracker;

	void Start () {
		try{ statTracker = GameObject.FindGameObjectWithTag("stat_tracker").GetComponent<StatTracker>(); } catch {}
		anim = transform.gameObject.GetComponent<Animator>();
		aux = transform.gameObject.GetComponent<AudioSource>();
		if(roar != null){
			nextRoar = Random.Range (10f,30f);
		}
	}

	void Update() {
		if(nextRoar > 0f)
			playRoar();
	}

	public void setRails(bool value){
		if(anim != null)
			anim.SetBool("onRails", value);
	}

	public void attackAnim(){
		if(anim != null)
			anim.SetInteger ("state",2);
		if(aux != null && hit != null)
			aux.PlayOneShot(hit);

	}

	public void flinchAnim(){
		if(anim != null)
			anim.SetInteger ("state",3);
	}

	public void disappearAnim(GameObject obj){
		this.obj = obj;
		if(anim != null)
			anim.SetInteger ("state",4);
	}

	public void playStep(){
		if(aux != null && step != null)
			aux.PlayOneShot(step);
	}

	private void playRoar(){
		if(roarTick >= nextRoar){
			if(aux != null && roar != null){
				aux.PlayOneShot(roar);
			}
			roarTick = 0f;
			nextRoar = Random.Range (10f,30f);
		} else {
			roarTick += Time.deltaTime;
		}
	}

	public void defaultTransition(){
		if(anim != null)
			anim.SetInteger ("state",0);
	}

	public void hideThis(){
		try{
			if(obj.GetComponent<UnitController>() is PlayerUnitController){
				statTracker.unitsKilled += 1;
			} else {
				statTracker.dinosKilled += 1;
			}
		}catch{}
		transform.GetComponentInChildren<MeshRenderer>().enabled = false;
		GameObject.Destroy (obj);
	}
}
