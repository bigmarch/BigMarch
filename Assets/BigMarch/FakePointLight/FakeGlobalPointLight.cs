using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FakeGlobalPointLight : MonoBehaviour
{
	public float Radias = 0.5f;
	public Color Color = Color.white;
	public float Strength = 1;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		Shader.SetGlobalVector("_Global_FakePointLightPosition",
			new Vector4(transform.position.x, transform.position.y, transform.position.z, Radias));
		Shader.SetGlobalColor("_Global_FakePointLightColor", Color);
		Shader.SetGlobalFloat("_Global_FakePointLightStrength", Strength);
	}

	void OnEnable()
	{
		Shader.EnableKeyword("_USE_FAKEPOINTLIGHT");
	}


	void OnDisable()
	{
		Shader.DisableKeyword("_USE_FAKEPOINTLIGHT");
	}
}
