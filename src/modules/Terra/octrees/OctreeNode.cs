public class OctreeNode
{
    public OctreeNode[] children { get; set; }
    public Chunk chunk { get; set; }
    public int materialID { get; set; } = -1;
}