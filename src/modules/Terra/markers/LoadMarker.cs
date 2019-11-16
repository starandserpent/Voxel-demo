using System.Collections.Generic;
using Godot;

public class LoadMarker
{

    /**
     * The radius which this marker will force the world to be loaded.
     * Squared to avoid sqrt.
     */
    private readonly float hardRadius;

    /**
     * The radius in which this marker will make world to not be loaded.
     * Squared to avoid sqrt.
     */
    private readonly float softRadius;

    public readonly Vector3 pos;

    private List<int> playerOctants;

    public LoadMarker(float x, float y, float z, float hardRadius, float softRadius) {
        pos = new Vector3();
        pos.x = x;
        pos.y = y;
        pos.z = z;

        this.hardRadius = hardRadius;
        this.softRadius = softRadius;
        playerOctants = new List<int>();
    }

    public void sendChunk(Chunk chunk){

    }

    public void sendOctree(Chunk octree){

    }

    public void calculateMarkerOctants(float size) {
        /*
        int iterations = CoreUtils.calculateOctreeLayers((int) size);
        size = size * DataConstants.CHUNK_SCALE;

        for (int i = 0; i < iterations; i++) {
            int octant = CoreUtils.selectOctant(x, y, z, size);
            playerOctants.add(octant);
            size = size / 2;
        }
        */
    }

    public List<int> getPlayerOctants() {
        return playerOctants;
    }

    public int getOctant(int index) {
       // return playerOctants.get(index);
        return 0;
    }

    public float getHardRadius() {
        return hardRadius;
    }

    public float getSoftRadius() {
        return softRadius;
    }

    
    public void SendPosition(float x, float y, float z){

    }
    public void Move(float x, float y, float z){

    }
}
