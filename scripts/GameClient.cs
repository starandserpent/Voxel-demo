using System.Buffers;
using System;
using System.Collections.Generic;
using Godot;

public class GameClient : Node
{
	[Export] public int GENERATION_THREADS = 1;
	[Export] public int VIEW_DISTANCE = 100;
	private float fov;
	private Vector3[] chunkPoints;
	private GodotMesher mesher;
	private GameServer server;
	private Player player;
	private Vector3 lastPosition;

	private Thread thread;

	private Dictionary<Tuple<int, int, int>, Chunk> chunks;

	private ArrayPool<Position> pool;

	public override void _Ready()
    {
		player = (Player) FindNode ("Player");

		fov = player.camera.Fov;

		chunks = new Dictionary<Tuple<int, int, int>, Chunk>();

		lastPosition = player.Transform.origin;

		pool = ArrayPool<Position>.Create (Constants.CHUNK_SIZE3D * 4 * 6, 1);

		List<Vector3> chunkPoints = new List<Vector3> ();
		for (int l = -VIEW_DISTANCE; l < VIEW_DISTANCE; l ++) {
			for (int y = -Utils.GetPosFromFOV (fov, l); y < Utils.GetPosFromFOV (fov, l); y ++) {
				for (int x = -Utils.GetPosFromFOV (fov, l); x < Utils.GetPosFromFOV (fov, l); x ++) {
					chunkPoints.Add (new Vector3 (x, y, -l));
				}
			}
		}

		this.chunkPoints = chunkPoints.ToArray();
		GD.Print ("Using " + GENERATION_THREADS + " threads");
	}

	public void AddServer(GameServer server, GodotMesher mesher)
	{
		this.server = server;
		this.mesher = mesher;
		thread = new Thread();
		thread.Start(this, nameof(AddChunks));
	}

	public override void _Process (float delta) 
	{
		if(lastPosition != player.Transform.origin)
		{
			lastPosition = player.Transform.origin;
			if(!thread.IsActive()){
				thread = new Thread();
				thread.Start(this, nameof(AddChunks));
			}
		}
	}

	private void AddChunks(Godot.Object empty)
	{
		Dictionary<Tuple<int, int, int>, Chunk> newChunks = new Dictionary<Tuple<int, int, int>, Chunk>();

		foreach(Vector3 pos in chunkPoints)
		{
			
			Vector3 chunkPos = player.ToGlobal(pos) / Constants.CHUNK_LENGHT;

			Tuple<int, int, int> tuple = new Tuple<int, int, int>((int) chunkPos.x, (int) chunkPos.y, (int) chunkPos.z);
			if(chunks.ContainsKey(tuple))
			{
				newChunks.Add(tuple, chunks[tuple]);
			}
			else
			{
				Chunk chunk = server.RequestChunk(tuple.Item1, tuple.Item2, tuple.Item3);
				if(chunk != null && !newChunks.ContainsKey(tuple))
				{
					if(!chunk.IsEmpty)
					{
						mesher.MeshChunk(chunk, pool);
					}

					newChunks.Add(tuple, chunk);
				}
			}
		}

		chunks = newChunks;
	}
}