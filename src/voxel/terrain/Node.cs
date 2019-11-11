 namespace VoxelOctree{
public struct Node<T>{
        public int FirstChild;
        public T Block;
        public bool HasChildren(){
            return FirstChild != LODOctree<T>.NO_CHILDREN;
        }

        public void init(){
            Block = default(T);
            FirstChild = LODOctree<T>.NO_CHILDREN;
        }
}
}