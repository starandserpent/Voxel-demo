using System.Numerics;
using System;
using Godot;
using Math3D;

namespace VoxelOctree{
public class LODOctree<T>
{
    public static readonly int NO_CHILDREN = -1;
    public static readonly int ROOT_INDEX = -1;
    public static readonly int MAX_LOD = 32; 

    private Node<T> root;
	private int MaxDepth = 0;
	private float BaseSize = 16;
	private float SplitScale = 2.0F;
	// TODO May be worth making this pool external for sharing purpose
	private NodePool<T> pool;  

    public LODOctree(){
        pool = new NodePool<T>();
    }  
    
    public void Clear(Action destroyAction){
        JoinAllRecursively(root, new Vector3i(), MaxDepth, destroyAction);
		MaxDepth = 0;
		BaseSize = 0;
    }

	static int ComputeLODCount (int base_size, int full_size) {
		int po = 0;
		while (full_size > base_size) {
			full_size = full_size >> 1;
			po += 1;
		}
		return po;
	}

	void ComputeFromLODCount(int base_size, int lod_count, Action destroy_action) {
		Clear(destroy_action);
		BaseSize = base_size;
		MaxDepth = lod_count - 1;
	}

	int GetLODCount() {
		return MaxDepth + 1;
	}


	void SetSplitScale(float p_split_scale) {

		const float minv = 2.0F;
		const float maxv = 5.0F;

		// Split scale must be greater than a threshold,
		// otherwise lods will decimate too fast and it will look messy
		if (p_split_scale < minv) {
			p_split_scale = minv;
		} else if (p_split_scale > maxv) {
			p_split_scale = maxv;
		}

		SplitScale = p_split_scale;
	}

	float GetSplitScale() {
		return SplitScale;
	}

	static int GetLODFactor(int lod) {
		return 1 << lod;
	}

	void Update(Vector3 view_pos, Action create_action, Action destroy_action) {

		if (root.Block != null || root.HasChildren()) {
			Update(ROOT_INDEX, new Vector3(), MaxDepth, view_pos, create_action, destroy_action);

		} else {
			// Treat the root in a slightly different way the first time.
			if (create_action.can_do_root(MaxDepth)) {
				root.Block = create_action(root, new Vector3i(), MaxDepth);
			}
		}
	}

	private Node<T> GetNode(int index) {
		if (index == ROOT_INDEX) {
			return root;
		} else {
			return pool.GetNode(index);
		}
	}

	private void Update( int node_index, Vector3i node_pos, int lod, Vector3 view_pos, Action create_action, Action destroy_action) {
		// This function should be called regularly over frames.

		int lod_factor = GetLODFactor(lod);
		int chunk_size = (int) BaseSize * lod_factor;
		Vector3 world_center = (chunk_size) * (node_pos.ToVector3() + new Vector3(0.5F, 0.5F, 0.5F));
		float split_distance = chunk_size * SplitScale;
		Node<T> node = GetNode(node_index);

		if (!node.HasChildren()) {

			// If it's not the last LOD, if close enough and custom conditions get fulfilled
			if (lod > 0 && world_center.DistanceTo(view_pos) < split_distance && create_action.can_do_children(node, node_pos, lod - 1)) {
				// Split

				int first_child = pool.AllocateChildren();
				// Get node again because `allocate_children` may invalidate the pointer
				node = GetNode(node_index);
				node.FirstChild = first_child;

				for (int i = 0; i < 8; ++i) {

					Node<T> child = pool.GetNode(first_child + i);

					child.Block = create_action(child, GetChildPosition(node_pos, i), lod - 1);

					// If the node needs to split more, we'll ask more recycling at the next frame...
					// That means the initialization of the game should do some warm up and fetch all leaves,
					// otherwise it's gonna be rough
				}

				if (node.Block != null) {
					destroy_action(node, node_pos, lod);
					node.Block = default(T);
				}
			}

		} else {

			bool has_split_child = false;
			int first_child = node.FirstChild;

			for (int i = 0; i < 8; ++i) {
				int child_index = first_child + i;
				Update(child_index, GetChildPosition(node_pos, i), lod - 1, view_pos, create_action, destroy_action);
				has_split_child |= pool.GetNode(child_index).HasChildren();
			}

			// Get node again because `update` may invalidate the pointer
			node = GetNode(node_index);

			if (!has_split_child && world_center.DistanceTo(view_pos) > split_distance && destroy_action.can_do(node, node_pos, lod)) {
				// Join
				if (node.HasChildren()) {

					for (int i = 0; i < 8; ++i) {
						Node<T> child = pool.GetNode(first_child + i);
						destroy_action(child, GetChildPosition(node_pos, i), lod - 1);
						child.Block = default(T);
					}

					pool.RecycleChildren(first_child);
					node.FirstChild = NO_CHILDREN;

					// If this is true, means the parent wasn't properly split.
					// When subdividing a node, that node's block must be destroyed as it is replaced by its children.
					node.Block = create_action(node, node_pos, lod);
				}
			}
		}
	}

    static Vector3i GetChildPosition(Vector3i parentPosition, int i){
        return new Vector3i(
            parentPosition.x * 2 + OctreeTables.GOctantPosition[i][0],
				parentPosition.y * 2 + OctreeTables.GOctantPosition[i][1],
				parentPosition.z * 2 + OctreeTables.GOctantPosition[i][2]);
    }

    private void JoinAllRecursively(Node<T> node, Vector3i nodePos, int lod, Action  destroyAction){
          if (node.HasChildren()) {
		    int first_child = node.FirstChild;

			for ( int i = 0; i < 8; ++i) {
				Node<T> child = pool.GetNode(first_child + i);
				JoinAllRecursively(child, GetChildPosition(nodePos, i), lod - 1, destroyAction);
			}

			pool.RecycleChildren(first_child);
			node.FirstChild = NO_CHILDREN;

		} else if (!node.Block.Equals(default(T))) {
		//	destroyAction(node, nodePos, lod);
			node.Block = default(T);
		}
	}
}
}