using UnityEngine;
using System.Collections;

public class SmallDinoController : DinoController {

	protected override void Start()
	{
		base.Start();
	}
	
	protected override void Update()
	{
		stateDelegate();
	}
}
