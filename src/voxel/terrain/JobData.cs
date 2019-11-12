using System.Collections.Generic;
using Godot;
using Math3D;

namespace VoxelOctree{
public struct JobData<T>
{

		// Data accessed from other threads, so they need mutexes
		//------------------------
		public Input<T> sharedInput;
		public Output<T> sharedOutput;
		public Mutex inputMutex;
		public Mutex outputMutex;
		// Indexes which blocks are present in shared_input,
		// so if we push a duplicate request with the same coordinates, we can discard it without a linear search
		public Dictionary<Vector3i, int>[] sharedInputBlockIndexes;
		public bool needsSort;
		// Only read by the thread
		public bool threadExit;
		//------------------------

		public Input<T> input;
		public Output<T> output;
		public Semaphore semaphore;
		public Thread thread;
		public int syncIntervalMs;
		public int jobIndex;
		public bool duplicateRejection;
		public int batchCount;

		public ProcessorStats stats;
        public InputBlock<T>[] inputBlocks;
        public OutputBlock<T>[] outputBlocks;
}
}