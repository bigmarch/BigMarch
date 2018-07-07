using BigMarch.Tool;
using UnityEngine;

[ExecuteInEditMode]
public class HealChain : MonoBehaviour
{
	public Transform Follow0;
	public Transform Follow1;

	public Transform From;
	public Transform To;
	public LineRenderer Line;

	// Update is called once per frame
	void Update()
	{
		if (Follow0)
		{
			From.position = Follow0.position;
		}

		if (Follow1)
		{
			To.position = Follow1.position;
		}

		Line.SetPosition(0, To.position);
		Line.SetPosition(Line.positionCount - 1, From.position);

		Vector3 delta = To.position - From.position;
		float length = delta.magnitude;
		float alphaLength = length.Remap(20, 30, 1, 0.1f);
		SetAlpha(alphaLength);

		if (Follow0)
		{
			float angle = Vector3.Angle(Follow0.forward, delta.normalized);
			float alphaAngle = angle.Remap(20, 30, 1, 0.1f);

			SetAlpha(alphaAngle * alphaLength);
		}
	}

	private void SetAlpha(float alpha)
	{
		Color c = Line.startColor;
		c.a = alpha;
		Line.startColor = c;

		c = Line.endColor;
		c.a = alpha;
		Line.endColor = c;
	}
}