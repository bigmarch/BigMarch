using UnityEngine;
using System.Collections;

namespace BigMarch.Tool
{
	public class FollowTransform : MonoBehaviour
	{
		public bool FollowPosition;
		public bool FollowRotation;
		public bool FollowLocalScale;

		private Transform _target;
		private Vector3 _positionOffset;
		private Vector3 _eulerAnglesOffset;
		private Vector3 _localScaleOffset;

		public void SetFollowTarget(Transform target)
		{
			_target = target;
			_positionOffset = _target.position - transform.position;
			_eulerAnglesOffset = _target.eulerAngles - transform.eulerAngles;
			_localScaleOffset = _target.localScale - transform.localScale;
		}

		void Update()
		{
			if (!_target)
			{
				return;
			}
			if (FollowPosition)
			{
				transform.position = _target.position - _positionOffset;
			}
			if (FollowRotation)
			{
				transform.eulerAngles = _target.eulerAngles - _eulerAnglesOffset;
			}
			if (FollowLocalScale)
			{
				transform.localScale = _target.localScale - _localScaleOffset;
			}
		}
	}
}
