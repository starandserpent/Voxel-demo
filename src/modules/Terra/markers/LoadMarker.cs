using System.Collections.Generic;
using Godot;

public class LoadMarker : Spatial
{

    /**
     * The radius which this marker will force the world to be loaded.
     * Squared to avoid sqrt.
     */
    public float hardRadius;

    private List<int> playerOctants;

    public void SendChunk(Chunk chunk){

    }

    public void SendOctree(Chunk octree){

    }

    public void CalculateMarkerOctants(float size) {
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

    public List<int> GetPlayerOctants() {
        return playerOctants;
    }

    public int GetOctant(int index) {
       // return playerOctants.get(index);
        return 0;
    }

    public float GetHardRadius() {
        return hardRadius;
    }
    
    public void SendPosition(float x, float y, float z){

    }
    
    public void Move(float x, float y, float z){

    }
}
