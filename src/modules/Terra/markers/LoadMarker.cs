using Godot;

public class LoadMarker : Spatial
{
    /**
     * The radius which this marker will force the world to be loaded.
     * Squared to avoid sqrt.
     */
    public int loadRadius { get; set; }
}