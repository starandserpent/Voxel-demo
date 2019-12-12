public class OctreeNode
{
    public long locCode { get; set; }
    public OctreeNode[] children { get; set; }
    public Chunk chunk { get; set; }
}