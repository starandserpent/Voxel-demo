using System.Diagnostics;
using Godot;
public class SolidCameraPoint : LoadMarker {
	[Export] public int CHUNKS_TO_GENERATE = 10;

	private int delta = 10;
	private int secondDelta = 0;

	public override void _Ready () {

	}

	public override void _Input (InputEvent @event) {
		if (Input.IsActionPressed ("ui_cancel")) {
			GetTree ().Quit ();
		}
	}

	public override void _Process (float delta) {
		secondDelta += 1;
		if (secondDelta > delta && delta > 0) {
			delta = -1;
			Registry reg = new Registry ();
			PrimitiveResources.Register (reg);
			GodotMesher mesher = new GodotMesher ();
			this.AddChild (mesher);
			mesher.Set (reg);
			ChunkFiller chunkFiller = new ChunkFiller (1, 2);
			Weltschmerz weltschmerz = new Weltschmerz ();

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
							mesher.MeshChunk (chunk);
						}
					}
				}
			}

			stopwatch.Stop ();
			Godot.GD.Print (CHUNKS_TO_GENERATE * CHUNKS_TO_GENERATE * CHUNKS_TO_GENERATE + " chunks generated in " + stopwatch.ElapsedMilliseconds);
		}
	}
}