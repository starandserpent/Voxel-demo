using System.Buffers;
using System.Diagnostics;
using Godot;
using Threading = Godot.Thread;
using Priority = Godot.Thread.Priority;

public class SolidCameraPoint : LoadMarker {

	[Export] public int CHUNKS_TO_GENERATE = 10;
	private ChunkFiller chunkFiller;

	private GodotMesher mesher;

	private Weltschmerz weltschmerz;

	public override void _Ready () {
		Registry reg = new Registry ();
		PrimitiveResources.Register (reg);
		mesher = (GodotMesher) GetParent ().FindNode ("GameMesher");
		mesher.Set (reg);
		chunkFiller = new ChunkFiller (1, 2);
		weltschmerz = new Weltschmerz ();
	}

	public override void _Input (InputEvent @event) {
		if (Input.IsActionPressed ("ui_cancel")) {
			GetTree ().Quit ();
		}
	}

	public void Generate () {
		ArrayPool<Position> pool = ArrayPool<Position>.Create (Constants.CHUNK_SIZE3D * 6 * 4, 1);
		Stopwatch stopwatch = new Stopwatch ();
		stopwatch.Start ();
		for (int x = 0; x < CHUNKS_TO_GENERATE; x++) {
			for (int y = 0; y < CHUNKS_TO_GENERATE; y++) {
				for (int z = 0; z < CHUNKS_TO_GENERATE; z++) {
					Chunk chunk = chunkFiller.GenerateChunk (x << Constants.CHUNK_EXPONENT, y << Constants.CHUNK_EXPONENT,
						z << Constants.CHUNK_EXPONENT, weltschmerz);
					if (!chunk.IsSurface) {
						var temp = chunk.Voxels[0];
						chunk.Voxels = new Run[1];
						chunk.Voxels[0] = temp;
						chunk.x = (uint) x << Constants.CHUNK_EXPONENT;
						chunk.y = (uint) y << Constants.CHUNK_EXPONENT;
						chunk.z = (uint) z << Constants.CHUNK_EXPONENT;
					}
					if (!chunk.IsEmpty) {
						mesher.MeshChunk (chunk, pool);
					}
				}
			}
		}
		stopwatch.Stop ();
		Godot.GD.Print (CHUNKS_TO_GENERATE * CHUNKS_TO_GENERATE * CHUNKS_TO_GENERATE + " chunks generated in " + stopwatch.ElapsedMilliseconds);
	}

	public override void _Process (float delta) { }
}