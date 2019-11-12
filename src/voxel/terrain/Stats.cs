using System.Reflection;
namespace VoxelOctree{
public class Stats<T>
{
    public bool first;
    public int minTime;
    public int maxTime;
    public int sortingTime;
    public int[] remainingBlocks;
    public int threadCount;
    public int droppedCount;
    public ProcessorStats processor;

    public Stats<T> init(){
        remainingBlocks = new int[VoxelBlockThreadManager<T>.MAX_JOBS];
        for (int i = 0; i < VoxelBlockThreadManager<T>.MAX_JOBS; ++i) {
				remainingBlocks[i] = 0;
		}

        return this;
    }
}
}