using UnityEngine;
using System.Collections;

public class HutManager : MonoBehaviour {

	public Material[] defaultMaterials;
	public Material[] placementMaterials;

	private bool isInitialized = false;
	private CapsuleCollider capsuleCollider;
	private MeshRenderer modelrenderer; 
	private Material[] modelmaterials;
	private KeepManager keep;

	// Use this for initialization
	void Start () {
		if(!isInitialized){
			init ();
		}
		keep = GameObject.FindGameObjectWithTag("Player").GetComponent<KeepManager>();
		varyHutDimensions ();
	}
	
	// Update is called once per frame
	void Update () {}

	void varyHutDimensions(){
		float height = Random.Range (0.95f,1.1f);
		float width = Random.Range (0.9f,1.2f);
		// Trend away from the center.
		height *= height;
		width *= width;
		transform.localScale = new Vector3(width, height, width);
	}

	private void init(){
		capsuleCollider = GetComponentInChildren< CapsuleCollider >();
		modelrenderer = transform.GetComponentInChildren< MeshRenderer >();
		modelmaterials = modelrenderer.materials;
	}

	public void planningTextures(bool valid){
		Material useMat;
		if(!isInitialized){
			init ();
		}

		capsuleCollider.enabled = false;

		if (valid) {
			useMat = placementMaterials [0];
		} else {
			useMat = placementMaterials [1];
		}
		for(int i = 0; i < modelmaterials.Length; i++) {
			modelmaterials[i] = useMat;
		}
		modelrenderer.materials = modelmaterials;
	}

	public void restoreTextures(){
		if(!isInitialized){
			init();
		}

		capsuleCollider.enabled = true;

		if(defaultMaterials.Length == modelmaterials.Length) {
			for(int i = 0; i < modelmaterials.Length; i++) {
				modelmaterials[i] = defaultMaterials[i];
			}
			modelrenderer.materials = modelmaterials;
		}

		keep.maxUnitCount += 3;
		// Refresh the button graphics.
		keep.alterSpawnCount(0);
	}
}
