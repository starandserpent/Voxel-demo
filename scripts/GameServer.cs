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
	[Export] public int FILLING_THREADS = 4;

	private Weltschmerz weltschmerz;
	private Registry registry;
	
	private volatile Terra terra;

	private Foreman foreman;

	private Thread[] fillingThreads;

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

		fillingThreads = new Thread[FILLING_THREADS];

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

		terra = new Terra (new Position(0, 0 ,0), boundries, this);

		foreman = new Foreman (weltschmerz, registry, terra, LOAD_RADIUS);

		GameClient client = (GameClient) FindNode ("GameClient");
		GodotMesher mesher = (GodotMesher) FindNode("GameMesher");
		mesher.SetRegistry(registry);

		GD.Print("Marking chunks for filling");	

		foreman.SetOrigin(0, 0, 0);

		GD.Print("Filling " + foreman.GetPrefillSize() + " chunks");

		GD.Print("Using " + FILLING_THREADS + " threads to fill chunks");

		for(int i = 0; i < FILLING_THREADS; i++)
		{
			fillingThreads[i] = new Thread();
			fillingThreads[i].Start(this, nameof(FillRadius));
		}

		for(int i = 0; i < FILLING_THREADS; i++)
		{
			fillingThreads[i].WaitToFinish();
		}

		client.AddServer(this, mesher);
	}

	private void FillRadius(Godot.Object empty)
	{
		foreman.Fill();
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
