using UnityEngine;
using System.Collections;

public class HutManager : MonoBehaviour {

	public Material[] defaultMaterials;
	public Material[] placementMaterials;

	private MeshRenderer modelrenderer; 
	private Material[] modelmaterials;

	// Use this for initialization
	void Start () {
		modelrenderer = transform.GetComponentInChildren< MeshRenderer >();
		modelmaterials = modelrenderer.materials;
		varyHutDimensions ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	void varyHutDimensions(){
		float height = Random.Range (0.95f,1.1f);
		float width = Random.Range (0.9f,1.2f);
		// Trend away from the center.
		height *= height;
		width *= width;
		transform.localScale = new Vector3(width, width, height);
	}

	void planningTextures(bool valid){
		Material useMat;
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

	void restoreTextures(){
		if(defaultMaterials.Length == modelmaterials.Length) {
			for(int i = 0; i < modelmaterials.Length; i++) {
				modelmaterials[i] = defaultMaterials[i];
			}
			modelrenderer.materials = modelmaterials;
		}
	}
}
