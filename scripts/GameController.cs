using System.Collections.Generic;
using Godot;

public class GameController : Spatial {
	private Picker picker;
	private volatile Terra terra;
	private Foreman foreman;
	[Export] public int SEED = 19083;
	[Export] public int VIEW_DISTANCE = 100;
	[Export] public int LONGITUDE = 1000;
	[Export] public int LATITUDE = 1000;
	[Export] public int MAX_ELEVATION = 1000;
	[Export] public int MIN_ELEVATION = 1;

	[Export] public int GENERATION_THREADS = 1;
	[Export] public int PROCESS_THREADS = 1;
	private GodotMesher mesher;
	private Weltschmerz weltschmerz;
	private Registry registry;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready () { }

	public override void _PhysicsProcess (float delta) { }

	public void Prepare (Camera camera, LoadMarker marker) {
		registry = new Registry ();
		PrimitiveResources.Register (registry);
		weltschmerz = new Weltschmerz ();
		Config config = weltschmerz.GetConfig ();
		config.elevation.max_elevation = MAX_ELEVATION;
		config.elevation.min_elevation = MIN_ELEVATION;
		config.map.latitude = LATITUDE;
		config.map.longitude = LONGITUDE;
		mesher = (GodotMesher) FindNode ("GameMesher");
		mesher.SetRegistry (registry);

		if (LONGITUDE < 2) {
			LONGITUDE = 2;
		}

		if (LATITUDE < 2) {
			LATITUDE = 2;
		}

		if (MAX_ELEVATION < 2) {
			MAX_ELEVATION = 2;
		}

		GD.Print ("Using " + GENERATION_THREADS + " threads");

		Position boundries = new Position ();
		boundries.x = LONGITUDE;
		boundries.y = MAX_ELEVATION;
		boundries.z = LATITUDE;

		terra = new Terra (boundries, this);
		picker = new Picker (terra, mesher);
		foreman = new Foreman (weltschmerz, terra, registry, mesher, VIEW_DISTANCE, camera.Fov, GENERATION_THREADS);

		for (int t = 0; t < GENERATION_THREADS; t++) {
			Thread thread = new Thread ();
			thread.Start (this, nameof (Generation));
		}

		for (int t = 0; t < PROCESS_THREADS; t++) {
			Thread thread = new Thread ();
			thread.Start (this, nameof (Loading));
		}

		foreman.SetMaterials (registry);
		marker.Attach (foreman);

		GodotSemaphore semaphore1 = new GodotSemaphore ();
		GodotSemaphore semaphore2 = new GodotSemaphore ();

		foreman.AddLoadMarker (marker, semaphore1, semaphore2);
	}

	public void Generation (Object empty) {
		foreman.Generate ();
	}

	public void Loading (Object empty) {
		foreman.Process ();
	}

	public Picker GetPicker () {
		return picker;
	}

	public void Clear () {
		foreman.Stop ();
	}

	public List<long> GetMeasures () {
		return foreman.GetMeasures ();
	}
}