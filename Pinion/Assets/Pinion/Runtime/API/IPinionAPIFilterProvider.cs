namespace Pinion
{
	public interface IPinionAPIFilterProvider
	{
		public int ApplyOrder { get; }
		public PinionAPI.IncludeMethodByTagHandler GetIncludeMethodByTagHandler();
	}
}