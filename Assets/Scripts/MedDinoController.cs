using UnityEngine;
using System.Collections;

public class MedDinoController : DinoController {

	protected override void Start()
	{
		base.Start();
	}
	
	protected override void Update()
	{
		stateDelegate();
	}
}
