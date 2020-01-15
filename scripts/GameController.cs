using System.Collections.Generic;
using System;
using System.Linq;
using Godot.Collections;
using System.Collections.Concurrent;
using Godot;

public class GameController : Spatial
{
    private Picker picker;
    private volatile Terra terra;
    private Foreman foreman;
    [Export] public int SEED = 19083;
    [Export] public int AVERAGE_TERRAIN_HIGHT = 130;
    [Export] public int TERRAIN_GENERATION_MULTIPLIER = 10;
    [Export] public int MAX_ELEVATION = 100;
    [Export] public float NOISE_FREQUENCY = 0.45F;
    [Export] public int VIEW_DISTANCE = 100;
    [Export] public int WORLD_SIZE_X = 1000;
    [Export] public int WORLD_SIZE_Y = 1000;
    [Export] public int WORLD_SIZE_Z = 1000;
    [Export] public int GENERATION_THREADS = 4;
    private GameMesher mesher;
    private Weltschmerz weltschmerz;
    private Registry registry;
    private int chunkCount;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Has to be devidable by 16
        registry = new Registry();
        PrimitiveResources.register(registry);
        weltschmerz = new Weltschmerz(SEED, TERRAIN_GENERATION_MULTIPLIER, AVERAGE_TERRAIN_HIGHT, MAX_ELEVATION,
            NOISE_FREQUENCY);
        mesher = new GameMesher(registry, this, false);
        terra = new Terra(WORLD_SIZE_X, WORLD_SIZE_Y, WORLD_SIZE_Z, this);
        picker = new Picker(terra, mesher);
    }

    public override void _PhysicsProcess(float delta)
    {
    }

    public void Prepare(Camera camera)
    {
        foreman = new Foreman(weltschmerz, terra, registry, mesher, VIEW_DISTANCE, camera.Fov, GENERATION_THREADS);
        foreman.SetMaterials(registry);
    }

    public void Generate(LoadMarker marker)
    {
        foreman.GenerateTerrain(marker);
    }

    public Picker GetPicker()
    {
        return picker;
    }

    public long GetChunkCount()
    {
        return GameMesher.ChunksMeshed;
    }

    public void Clear()
    {
        foreman.Stop();
    }

    public List<long> GetMeasures()
    {
        return foreman.GetMeasures();
    }
}