using System.Collections.Concurrent;
using System.Buffers;
using System;
using System.Collections.Generic;
using Godot;

public class GameClient : Node
{
	[Export] public int GENERATION_THREADS = 1;

	[Export] public int LOAD_THREADS = 1;

	[Export] public int VIEW_DISTANCE = 100;
	private float fov;
	private Vector3[] chunkPoints;
	private GodotMesher mesher;
	private GameServer server;
	private Player player;
	private Vector3 lastPosition;

	private Semaphore generationSemaphore;
	private Semaphore loadSemaphore;

	private Thread[] loadThreads;
	private Thread[] generationThreads;

	private volatile ConcurrentQueue<Chunk> chunks;

	private ArrayPool<Position> pool;

	private bool runThread;

	private int loadSize;

	public override void _Ready()
    {

		player = (Player) FindNode ("Player");

		fov = player.camera.Fov;

		chunks = new ConcurrentQueue<Chunk>();

		runThread = true;

		generationThreads = new Thread[GENERATION_THREADS];
		loadThreads = new Thread[LOAD_THREADS];

		generationSemaphore = new Semaphore();
		loadSemaphore = new Semaphore();
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

		loadSize = this.chunkPoints.Length;

		for(int i = 0; i < GENERATION_THREADS; i ++)
		{
			generationThreads[i] = new Thread();
			generationThreads[i].Start(this, nameof(GenerateChunks));
		}

		for(int i = 0; i < LOAD_THREADS; i ++)
		{
			loadThreads[i] = new Thread();
			loadThreads[i].Start(this, nameof(LoadChunks));
		}

		lastPosition = player.Transform.origin;

		GD.Print ("Using " + GENERATION_THREADS + " threads");
	}

	public void AddServer(GameServer server, GodotMesher mesher)
	{
		this.server = server;
		this.mesher = mesher;
		StartLoading();
	}

	private void StartLoading()
	{
		chunks = new ConcurrentQueue<Chunk>();
		loadSize = 0;
		for(int i = 0; i < LOAD_THREADS; i++)
		{
			loadSemaphore.Post();
		}
	}

	public override void _Process (float delta) 
	{
		if(lastPosition != player.Transform.origin)
		{
			lastPosition = player.Transform.origin;
			StartLoading();
		}
	}

	private void LoadChunks(Godot.Object empty)
	{
		while(runThread)
		{
			if(loadSize >= chunkPoints.Length){
				loadSemaphore.Wait();
				loadSize = 0;
			}

			foreach(Vector3 pos in chunkPoints)
			{
				Vector3 chunkPos = player.ToGlobal(pos) / Constants.CHUNK_LENGHT;
				Chunk chunk = server.RequestChunk((int) chunkPos.x, (int) chunkPos.y,(int) chunkPos.z);
				if(chunk != null && !chunk.IsGenerated)
				{
					chunks.Enqueue(chunk);
					generationSemaphore.Post();
				}
				loadSize ++;
			}
		}
	}

		private void GenerateChunks(Godot.Object empty)
		{
			while(runThread){
				generationSemaphore.Wait();
				Chunk chunk;
				if(chunks != null && chunks.TryDequeue(out chunk)){
					if(!chunk.IsEmpty)
					{
						mesher.MeshChunk(chunk, pool);
						chunk.IsGenerated = true;
					}
				}
			}
		}

	public void Stop()
	{
		runThread = false;
	}
}