using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanIndicator : MonoBehaviour
{
	public float Angle = 100;
	public float Radius = 2;
	
	public MeshFan Fan;
	public LineRenderer Line0;
	public LineRenderer Line1;


	void Update()
	{
		Fan.angleDegree = Angle;
		Fan.radius = Radius;
		
		Line0.SetPosition(0 , transform.position);
		Line0.SetPosition(1, transform.position + Quaternion.Euler(0, Angle / 2, 0) * transform.forward * Radius);
		
		Line1.SetPosition(0 , transform.position);
		Line1.SetPosition(1, transform.position + Quaternion.Euler(0, -Angle / 2, 0) * transform.forward * Radius);
	}
}