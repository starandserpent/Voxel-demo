using System.Reflection;
namespace VoxelOctree{
public class Stats
{
    bool First;
    int MinTime;
    int MaxTime;
    int SortingTime;
    int[] RemainingBlocks;
    int ThreadCount;
    int DroppedCount;
    ProcessorArchitecture processor;

    public void init(){
        RemainingBlocks = new int[VoxelBlockThreadManager.MAX_JOBS];
        for (int i = 0; i < VoxelBlockThreadManager.MAX_JOBS; ++i) {
				RemainingBlocks[i] = 0;
		}
    }
}
}