namespace BigMarch.BlendTree
{
	public abstract class BlendData
	{
		protected BlendData()
		{
		}

		public abstract void CaculateLerp(BlendData from, BlendData to, float lerpRatio);
		public abstract void CopyFrom(BlendData target);
		public abstract void CaculateAdd(BlendData lhs, BlendData rhs);
	}
}