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

	private volatile ConcurrentQueue<Vector3> chunks;

	private ArrayPool<Position> pool;

	private bool runThread;

	private int loadSize;

	private Spatial spatial;
	public override void _Ready()
    {

		player = (Player) FindNode ("Player");

		spatial = new Spatial();
		AddChild(spatial, true);

		player.AddShadow(spatial);

		fov = player.camera.Fov;

		chunks = new ConcurrentQueue<Vector3>();

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

		lastPosition = spatial.Transform.origin;

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
		chunks = new ConcurrentQueue<Vector3>();
		loadSize = 0;
		for(int i = 0; i < LOAD_THREADS; i++)
		{
			loadSemaphore.Post();
		}
	}

	public override void _Process (float delta) 
	{
		if(lastPosition != spatial.Transform.origin)
		{
			lastPosition = spatial.Transform.origin;
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

			Vector3 pos = chunkPoints[loadSize];
			loadSize ++;

			Vector3 chunkPos = spatial.ToGlobal(pos) / Constants.CHUNK_LENGHT;
			chunks.Enqueue(chunkPos);
			generationSemaphore.Post();
		}
	}

	private void GenerateChunks(Godot.Object empty)
	{
		while(runThread){
			generationSemaphore.Wait();
			Vector3 chunkPos;
			if(chunks != null && chunks.TryDequeue(out chunkPos)){
				int posX = (int) chunkPos.x;
				int posY = (int) chunkPos.y;
				int posZ = (int) chunkPos.z;

				Chunk chunk = server.RequestChunk(posX, posY, posZ);
				Chunk[] neighbors = new Chunk[6];
				if(chunk != null && !chunk.IsEmpty && chunk.IsFilled && !chunk.IsGenerated && SearchForNeighbors(posX, posY, posZ, neighbors))
				{
					chunk.IsGenerated = true;
					mesher.MeshChunk(chunk, neighbors, pool);
				}
			}
		}
	}

	private bool SearchForNeighbors(int posX, int posY, int posZ, Chunk[] neighbors)
	{
		Chunk chunk;
		for(int i = 0; i < 6; i++)
		{
			switch(i)
			{
				//Front
				case 0:
				  	chunk =	server.RequestChunk(posX, posY, posZ - 1);
					if(chunk != null && chunk.IsFilled){
						neighbors[0] = chunk;
					}
					else
					{
						return false;
					}
				break;

				//Back
				case 1:
					chunk =	server.RequestChunk(posX, posY, posZ + 1);
					if(chunk != null && chunk.IsFilled){
						neighbors[1] = chunk;
					}
					else
					{
						return false;
					}
				break;

				//Right
				case 2:
					chunk =	server.RequestChunk(posX + 1, posY, posZ);
					if(chunk != null && chunk.IsFilled){
						neighbors[2] = chunk;
					}
					else
					{
						return false;
					}
				break;

				//Left
				case 3:
					chunk =	server.RequestChunk(posX - 1, posY, posZ);
					if(chunk != null && chunk.IsFilled){
						neighbors[3] = chunk;
					}
					else
					{
						return false;
					}
				break;

				//Top
				case 4:
					chunk =	server.RequestChunk(posX, posY + 1, posZ);
					if(chunk != null && chunk.IsFilled){
						neighbors[4] = chunk;
					}
					else
					{
						return false;
					}
				break;

				//Bottom
				case 5:
					chunk = server.RequestChunk(posX, posY - 1, posZ);
					if(chunk != null && chunk.IsFilled){
						neighbors[5] = chunk;
					}
					else
					{
						return false;
					}
				break;
			}
		}

		return true;
	}

	public void Stop()
	{
		runThread = false;
	}
}