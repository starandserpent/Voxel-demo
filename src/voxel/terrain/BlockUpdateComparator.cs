
namespace VoxelOctree{
public struct BlockUpdateComparator<T> {
	public bool init(InputBlock<T> a, InputBlock<T> b) {
			return a.sortHeuristic < b.sortHeuristic;
	}
}
}