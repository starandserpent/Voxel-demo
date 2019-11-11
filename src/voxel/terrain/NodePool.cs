
using System.Collections.Generic;

namespace VoxelOctree{
public class NodePool<T>
{
    private List<Node<T>> nodes;
    private List<int> freeIndexes;

    public NodePool(){
        nodes = new List<Node<T>>();
        freeIndexes = new List<int>();
    }

    public Node<T> GetNode(int i){
        return nodes[i];
    }

    public int AllocateChildren(){
        if(freeIndexes.Count == 0){
            int i0 = nodes.Count;
            nodes.Capacity = i0 + 8;
            return i0;
        }else{
            int i0 = freeIndexes[freeIndexes.Count - 1];
			freeIndexes.Remove(i0);
            return i0;
        }
    }

    public void RecycleChildren(int i0){
        for(int i = 0; i < 8; ++i){
            nodes[i0 + 1].init();
        }

        freeIndexes.Add(i0);
    }
}
}