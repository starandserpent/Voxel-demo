using System.Linq;
using Godot;
using System;
using System.Collections.Generic;

namespace VoxelOctree{
public class VoxelBlockThreadManager<T>
{
    public static readonly int MAX_LOD = 32;
    public static readonly int MAX_JOBS = 8;

    JobData<T>[] jobs = new JobData<T>[MAX_JOBS];
    private int jobCount = 0;

    public VoxelBlockThreadManager(int jobCount, int syncIntervalMS, InputBlock<T>[] inputBlocks,
    OutputBlock<T>[] outputBlocks, ProcessorStats stats, bool duplicateRejection = true, int batchCount = 1){
		this.jobCount = jobCount;

		for (int i = 0; i < MAX_JOBS; ++i) {
			JobData<T> job = jobs[i];
			job.jobIndex = i;
			job.duplicateRejection = duplicateRejection;
			job.syncIntervalMs = syncIntervalMS;
			job.batchCount = batchCount;
		}

		for (int i = 0; i < jobCount; ++i) {

			JobData<T> job = jobs[i];

			job.inputMutex = new Mutex();
			job.outputMutex = new Mutex();
			job.semaphore = new Semaphore();
			job.thread = new Thread();
			job.needsSort = true;
            job.stats = stats;
            job.inputBlocks = inputBlocks;
            job.outputBlocks  = outputBlocks; 
		}
    }

    public VoxelBlockThreadManager() {

		for (int i = 0; i < jobCount; ++i) {
			JobData<T> job = jobs[i];
			job.threadExit = true;
			job.semaphore.Post();
		}

		for (int i = 0; i < jobCount; ++i) {

			JobData<T> job = jobs[i];

            job.thread.WaitToFinish();

		/*	memdelete(job.thread);
			memdelete(job.semaphore);
			memdelete(job.inputMutex);
			memdelete(job.outputBlocks);
			*/
		}
	}

	public void Push(Input<T> input) {

	    int replaced_blocks = 0;
	    int highest_pending_count = 0;
	    int lowest_pending_count = 0;

		// Lock all inputs and gather their pending work counts
		for (int job_index = 0; job_index < jobCount; ++job_index) {

			JobData<T> job = jobs[job_index];

			job.inputMutex.Lock();

			highest_pending_count = Math.Max(highest_pending_count, job.sharedInput.blocks.Count);
			lowest_pending_count = Math.Min(lowest_pending_count, job.sharedInput.blocks.Count);
		}

		int i = 0;

		// We don't use a "weakest team gets it" dispatch for speed,
		// So prioritize only jobs under median workload count and not just the highest.
		int median_pending_count = lowest_pending_count + (highest_pending_count - lowest_pending_count) / 2;

		// Dispatch to jobs with least pending requests
		for (int job_index = 0; job_index < jobCount && i < input.blocks.Count; ++job_index) {

			JobData<T> job = jobs[job_index];
		    int pending_count = job.sharedInput.blocks.Count;

			int count = Math.Min(median_pending_count - pending_count, input.blocks.Count);

			if (count > 0) {
				if (i + count > input.blocks.Count) {
					count = input.blocks.Count - i;
				}
				replaced_blocks += push_block_requests(job, input.blocks, i, count);
				i += count;
			}
		}

		// Dispatch equal count of remaining requests.
		// Remainder is dispatched too until consumed through the first jobs.
		int base_count = (input.blocks.Count - i) / jobCount;
		int remainder = (input.blocks.Count - i) % jobCount;
		for (int job_index = 0; job_index < jobCount && i < input.blocks.Count; ++job_index) {

			JobData<T> job = jobs[job_index];

			int count = base_count;
			if (remainder > 0) {
				++count;
				--remainder;
			}

			if (i + count > input.blocks.Count) {
				replaced_blocks += push_block_requests(job, input.blocks, i, input.blocks.Count - i);
			} else {
				replaced_blocks += push_block_requests(job, input.blocks, i, count);
				i += count;
			}
		}

		// Set remaining data on all jobs, unlock inputs and resume
		for (int job_index = 0; job_index < jobCount; ++job_index) {

			JobData<T> job = jobs[job_index];

			if (job.sharedInput.priorityPosition != input.priorityPosition || input.blocks.Count > 0) {
				job.needsSort = true;
			}

			job.sharedInput.priorityPosition = input.priorityPosition;

			if (input.useExlusiveRegion) {
				job.sharedInput.useExlusiveRegion = true;
				job.sharedInput.exlusiveRegionExtent = input.exlusiveRegionExtent;
				job.sharedInput.exlusiveRegionMaxLOD = input.exlusiveRegionMaxLOD;
			}

			bool should_run = !job.sharedInput.IsEmpty();

			job.inputMutex.Unlock();

			if (should_run) {
				job.semaphore.Post();
			}
		}

		if (replaced_blocks > 0) {
			Console.Write("VoxelBlockProcessor: " + replaced_blocks.ToString() +" blocks already in queue were replaced");
		}
	}

	void pop(Output<T> output) {

		output.stats = new Stats<T>().init();
		output.stats.threadCount = jobCount;

		// Harvest results from all jobs
		for (int i = 0; i < jobCount; ++i) {

			JobData<T> job = jobs[i];
			{
			/*	MutexLock lock(job.output_mutex);

				output.blocks.AddRange(job.sharedOutput.blocks);
				merge_stats(output.stats, job.sharedOutput.stats, i);
				job.sharedOutput.blocks.Clear();
			*/
			}
		}
	}

	static Dictionary<string, int> toDictionary(Stats<T> stats) {
		Dictionary<string, int> d = new Dictionary<string, int>();
		d["min_time"] = stats.minTime;
		d["max_time"] = stats.maxTime;
		d["sorting_time"] = stats.sortingTime;
		d["dropped_count"] = stats.droppedCount;
		int[] remainingBlocks = new int[stats.threadCount];
		for ( int i = 0; i < stats.threadCount; ++i) {
			remainingBlocks[i] = stats.remainingBlocks[i];
		}
		d["remaining_blocks_per_thread"] = remainingBlocks.Count();
		d["file_openings"] = stats.processor.fileOpenings;
		d["time_spent_opening_files"] = stats.processor.timeSpentOpeningFiles;
		return d;
	}

	static void MergeStats(Stats<T> a, Stats<T> b, int jobIndex) {

		a.maxTime = Math.Max(a.maxTime, b.maxTime);
		a.minTime = Math.Min(a.minTime, b.minTime);
		a.remainingBlocks[jobIndex] = b.remainingBlocks[jobIndex];
		a.sortingTime += b.sortingTime;
		a.droppedCount += b.droppedCount;

		a.processor.fileOpenings += b.processor.fileOpenings;
		a.processor.timeSpentOpeningFiles += b.processor.timeSpentOpeningFiles;
	}

	int push_block_requests(JobData<T> job, List<InputBlock<T>> input_blocks, int begin, int count) {
		// The job's input mutex must have been locked first!

		int replaced_blocks = 0;
		int end = begin + count;

		for (int i = begin; i < end; ++i) {

		 	InputBlock<T> block = input_blocks[i];

			if (job.duplicateRejection) {

				int index = job.sharedInputBlockIndexes[block.lod][block.position];

				// TODO When using more than one thread, duplicate rejection is less effective... is it relevant to keep it at all?
				if (index == 0) {
					// The block is already in the update queue, replace it
					++replaced_blocks;
					job.sharedInput.blocks[index] = block;

				} else {
					// Append new block request
					int j = job.sharedInput.blocks.Count;
					job.sharedInput.blocks.Add(block);
					job.sharedInputBlockIndexes[block.lod][block.position] = j;
				}

			} else {
				job.sharedInput.blocks.Add(block);
			}
		}

		return replaced_blocks;
	}

	/*
	public static void ThreadFunc() {
		JobData<T> data = reinterpret_cast<JobData *>(p_data);
		thread_func(data);
	}

	static void thread_func(JobData<T> data) {

		while (!data.thread_exit) {

			uint32_t sync_time = OS::get_singleton()->get_ticks_msec() + data.sync_interval_ms;

			unsigned int queue_index = 0;
			Stats stats;

			thread_sync(data, queue_index, stats, stats.sorting_time, stats.dropped_count);

			// Continue to run as long as there are queries to process
			while (!data.input.blocks.empty()) {

				if (!data.input.blocks.empty()) {

					if (data.thread_exit) {
						// Remove all queries except those that can't be discarded
						std::remove_if(data.input.blocks.begin(), data.input.blocks.end(),
								[](const InputBlock &b) {
									return b.can_be_discarded;
								});
					}

					unsigned int input_begin = queue_index;
					unsigned int batch_count = data.batch_count;

					if (input_begin + batch_count > data.input.blocks.size()) {
						batch_count = data.input.blocks.size() - input_begin;
					}

					if (batch_count > 0) {

						uint64_t time_before = OS::get_singleton()->get_ticks_usec();

						unsigned int output_begin = data.output.blocks.size();
						data.output.blocks.resize(data.output.blocks.size() + batch_count);

						for (unsigned int i = 0; i < batch_count; ++i) {
							CRASH_COND(input_begin + i < 0 || input_begin + i >= data.input.blocks.size());
							InputBlock &ib = data.input.blocks[input_begin + i];
							OutputBlock &ob = data.output.blocks.write[output_begin + i];
							ob.position = ib.position;
							ob.lod = ib.lod;
						}

						data.processor(
								ArraySlice<InputBlock>(&data.input.blocks[0], input_begin, input_begin + batch_count),
								ArraySlice<OutputBlock>(&data.output.blocks.write[0], output_begin, output_begin + batch_count),
								stats.processor);

						uint64_t time_taken = (OS::get_singleton()->get_ticks_usec() - time_before) / batch_count;

						// Do some stats
						if (stats.first) {
							stats.first = false;
							stats.min_time = time_taken;
							stats.max_time = time_taken;
						} else {
							if (time_taken < stats.min_time) {
								stats.min_time = time_taken;
							}
							if (time_taken > stats.max_time) {
								stats.max_time = time_taken;
							}
						}
					}

					queue_index += batch_count;
					if (queue_index >= data.input.blocks.size()) {
						data.input.blocks.clear();
					}
				}

				uint32_t time = OS::get_singleton()->get_ticks_msec();
				if (time >= sync_time || data.input.blocks.empty()) {

					uint64_t sort_time;
					unsigned int dropped_count;
					thread_sync(data, queue_index, stats, sort_time, dropped_count);

					sync_time = OS::get_singleton()->get_ticks_msec() + data.sync_interval_ms;
					queue_index = 0;
					stats = Stats();
					stats.sorting_time = sort_time;
					stats.dropped_count = dropped_count;
				}
			}

			if (data.thread_exit) {
				break;
			}

			// Wait for future wake-up
			data.semaphore->wait();
		}
	}

	static inline float get_priority_heuristic(const InputBlock &a, const Vector3i &viewer_block_pos, const Vector3 &viewer_direction, int max_lod) {
		int f = 1 << a.lod;
		Vector3i p = a.position * f;
		float d = Math::sqrt(p.distance_sq(viewer_block_pos) + 0.1f);
		float dp = viewer_direction.dot(viewer_block_pos.to_vec3() / d);
		// Higher lod indexes come first to allow the octree to subdivide.
		// Then comes distance, which is modified by how much in view the block is
		return (max_lod - a.lod) * 10000.f + d + (1.f - dp) * 4.f * f;
	}


	static void thread_sync(JobData &data, unsigned int queue_index, Stats stats, uint64_t &out_sort_time, unsigned int &out_dropped_count) {

		if (!data.input.blocks.empty()) {
			// Cleanup input vector

			if (queue_index >= data.input.blocks.size()) {
				data.input.blocks.clear();

			} else if (queue_index > 0) {

				// Shift up remaining items since we use a Vector
				shift_up(data.input.blocks, queue_index);
			}
		}

		stats.remaining_blocks[data.job_index] = data.input.blocks.size();
		bool needs_sort;

		// Get input
		{
			MutexLock lock(data.input_mutex);

			// Copy requests from shared to internal
			append_array(data.input.blocks, data.shared_input.blocks);

			data.input.priority_position = data.shared_input.priority_position;

			if (data.shared_input.use_exclusive_region) {
				data.input.use_exclusive_region = true;
				data.input.exclusive_region_extent = data.shared_input.exclusive_region_extent;
				data.input.exclusive_region_max_lod = data.shared_input.exclusive_region_max_lod;
			}

			data.shared_input.blocks.clear();

			if (data.duplicate_rejection) {
				// We emptied shared input, empty shared_input_block_indexes then
				for (unsigned int lod_index = 0; lod_index < MAX_LOD; ++lod_index) {
					data.shared_input_block_indexes[lod_index].clear();
				}
			}

			needs_sort = data.needs_sort;
			data.needs_sort = false;
		}

		if (!data.output.blocks.empty()) {

			//		print_line(String("VoxelMeshUpdater: posting {0} blocks, {1} remaining ; cost [{2}..{3}] usec")
			//				   .format(varray(_output.blocks.size(), _input.blocks.size(), stats.min_time, stats.max_time)));

			// Copy output to shared
			MutexLock lock(data.output_mutex);
			data.shared_output.blocks.append_array(data.output.blocks);
			data.shared_output.stats = stats;
			data.output.blocks.clear();
		}

		// Cancel blocks outside exclusive region.
		// We do this early because if the player keeps moving forward,
		// we would keep accumulating requests forever, and that means slower sorting and memory waste
		int dropped_count = 0;
		if (data.input.use_exclusive_region) {
			for (unsigned int i = 0; i < data.input.blocks.size(); ++i) {
				const InputBlock &ib = data.input.blocks[i];

				if (!ib.can_be_discarded || ib.lod >= data.input.exclusive_region_max_lod) {
					continue;
				}

				Rect3i box = Rect3i::from_center_extents(data.input.priority_position >> ib.lod, Vector3i(data.input.exclusive_region_extent));

				if (!box.contains(ib.position)) {

					// Indicate the caller that we dropped that block.
					// This can help troubleshoot bugs in some situations.
					OutputBlock ob;
					ob.position = ib.position;
					ob.lod = ib.lod;
					ob.drop_hint = true;
					data.output.blocks.push_back(ob);

					// We'll put that block in replacement of the dropped one and pop the last cell,
					// so we don't need to shift every following blocks
					const InputBlock &shifted_block = data.input.blocks.back();

					// Do this last because it invalidates `ib`
					data.input.blocks[i] = shifted_block;
					data.input.blocks.pop_back();

					// Move back to redo this index, since we replaced the current block
					--i;

					++dropped_count;
				}
			}
		}

		if (dropped_count > 0) {
			print_line(String("Dropped {0} blocks from thread").format(varray(dropped_count)));
			out_dropped_count = dropped_count;
		}

		uint64_t time_before = OS::get_singleton()->get_ticks_usec();

		if (!data.input.blocks.empty() && needs_sort) {

			for (auto it = data.input.blocks.begin(); it != data.input.blocks.end(); ++it) {
				InputBlock &ib = *it;
				// Set or override previous heuristic based on new infos
				ib.sort_heuristic = get_priority_heuristic(ib,
						data.input.priority_position,
						data.input.priority_direction,
						data.input.max_lod_index);
			}

			// Re-sort priority
			SortArray<InputBlock, BlockUpdateComparator> sorter;
			sorter.sort(data.input.blocks.data(), data.input.blocks.size());
		}

		out_sort_time = OS::get_singleton()->get_ticks_usec() - time_before;
	}

	JobData _jobs[MAX_JOBS];
	unsigned int _job_count = 0;
	*/
}
}