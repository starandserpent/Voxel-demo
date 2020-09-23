using Godot;
using System;

public class GameServer : Node
{
	//Exports
	[Export] public int LOAD_RADIUS = 10;
	[Export] public int SEED = 19083;
	[Export] public int LONGITUDE = 1000;
	[Export] public int LATITUDE = 1000;
	[Export] public int MAX_ELEVATION = 1000;
	[Export] public int MIN_ELEVATION = 1;
	[Export] public int PROCESS_THREADS = 1;

	private Weltschmerz weltschmerz;
	private Registry registry;
	
	private volatile Terra terra;

	private Foreman foreman;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		registry = new Registry ();
		PrimitiveResources.Register (registry);
		weltschmerz = new Weltschmerz ();
		Config config = weltschmerz.GetConfig ();
		config.elevation.max_elevation = MAX_ELEVATION;
		config.elevation.min_elevation = MIN_ELEVATION;
		config.map.latitude = LATITUDE;
		config.map.longitude = LONGITUDE;

		if (LONGITUDE < 2) {
			LONGITUDE = 2;
		}

		if (LATITUDE < 2) {
			LATITUDE = 2;
		}

		if (MAX_ELEVATION < 2) {
			MAX_ELEVATION = 2;
		}

		Position boundries = new Position ();
		boundries.x = LONGITUDE;
		boundries.y = MAX_ELEVATION;
		boundries.z = LATITUDE;

		terra = new Terra (boundries, this);
		GodotSemaphore semaphore1 = new GodotSemaphore ();
		GodotSemaphore semaphore2 = new GodotSemaphore ();

		foreman = new Foreman (weltschmerz, registry, terra, LOAD_RADIUS);

		GameClient client = (GameClient) FindNode ("GameClient");
		GodotMesher mesher = (GodotMesher) FindNode("GameMesher");
		mesher.SetRegistry(registry);
		client.AddServer(this, mesher);
	}

	public Chunk RequestChunk(int x, int y, int z)
	{
		OctreeNode node = terra.TraverseOctree(x, y, z, 0);
		if(node != null)
		{
			return node.chunk;
		}
		return null;
	}
}
