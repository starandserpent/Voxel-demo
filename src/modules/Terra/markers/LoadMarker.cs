using System.Collections.Generic;
using Godot;

public class LoadMarker : Spatial
{
    /**
     * The radius which this marker will force the world to be loaded.
     * Squared to avoid sqrt.
     */
    public int loadRadiusX{get; protected set;}
    public int loadRadiusY{get; protected set;}
    public int loadRadiusZ{get; protected set;}
    public void SendChunk(Chunk chunk)
    {
    }

    public void SendOctree(Chunk octree)
    {
    }

    public void CalculateMarkerOctants(float size)
    {
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
}