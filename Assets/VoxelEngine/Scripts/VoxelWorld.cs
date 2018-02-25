using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class VoxelWorld : MonoBehaviour
{
    public static float time;
    public static VoxelWorld singleton;
    public static int seed = 0;

    static Queue<VoxelChunk> dirtyChuncks = new Queue<VoxelChunk>();
    static Queue<Vector3> spawnChuncks = new Queue<Vector3>();

    public int mapSize = 10;
    public GameObject chunkPrefab;

    public Thread mainThread;

    public static Vector3 MapMiddle;
    public static float MapMiddleDistance;

    public static VBlock AirBlock;
    public static VBlock GrassBlock;
    public static VBlock DirtBlock;
    public static VBlock GravelBlock;
    public static VBlock WoodBlock;
    public static VBlock LavaBlock;

    private void Awake()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        InitBlocks();
        singleton = this;

        float middle = (mapSize / 2) * VoxelChunk.chunkSize;
        MapMiddle = new Vector3(middle, 0, middle);
        MapMiddleDistance = MapMiddle.magnitude / 2f;

        mainThread = new Thread(VoxelUpdate);
        mainThread.Start();
    }
    private void InitBlocks()
    {
        AirBlock = new VBlock(0, 0); //Air
        GrassBlock = new VBlock(2, 0); //Grass
        DirtBlock = new VBlock(3, 0); //Dirt
        GravelBlock = new VBlock(0, 0); //Gravel
        WoodBlock = new VBlock(0, 1); //Wood
        LavaBlock = new VBlock(1, 0); //Lava
    }
    
    void Start ()
    {
        seed = Random.Range(int.MinValue, int.MaxValue);
        for(int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int z = 0; z < mapSize; z++)
                {
                    Instantiate(
                        chunkPrefab,
                        new Vector3(x * VoxelChunk.chunkSize, y * VoxelChunk.chunkSize, z * VoxelChunk.chunkSize),
                        Quaternion.identity, transform);
                }
            }
        }
    }

    public static void QueueDirtyChunk(VoxelChunk c)
    {
        dirtyChuncks.Enqueue(c);
    }
    public static void QueueSpawnChunk(Vector3 pos)
    {
        VoxelChunk.chunks.Add(pos, null);
        spawnChuncks.Enqueue(pos);
    }

    private void Update()
    {
        time = Time.time;
        if (dirtyChuncks.Count != 0)
        {
            VoxelChunk chunk = dirtyChuncks.Dequeue();
            if (chunk != null)
                UpdateChunk(chunk);
        }
        if (spawnChuncks.Count != 0)
        {
            Vector3 spawnPos = spawnChuncks.Dequeue();

            Instantiate(chunkPrefab,
                        spawnPos,
                        Quaternion.identity, transform);
        }
    }

    private void VoxelUpdate()
    {
        while(true)
        {
            try
            {
                foreach (var c in VoxelChunk.chunks)
                {
                    if (c.Value == null) continue;

                    if (!c.Value.map.startedGenerating)
                    {
                        c.Value.CreateMap(null);
                    }

                    if( c.Value.chunkDirty &&
                        c.Value.map.isReady &&
                        c.Value.chunkReady && 
                        !c.Value.map.mapEmpty)
                    {
                        c.Value.CreateMesh(4);
                    }
                }
            }
            catch { }

            Thread.Sleep(30);
        }
    }

    private void UpdateChunk(VoxelChunk chunk)
    {
        chunk.threadReady = true;
        chunk.MeshReady();
    }

    static FastNoise fn = new FastNoise(0);
    public static Vector3 TransformPoint(VoxelChunk c, Vector3 pos)
    {
        Vector3 worldpos = pos + c.myPosition;
        
        System.Random r = new System.Random(Mathf.FloorToInt(worldpos.x + worldpos.z) * (int)worldpos.y);
        Vector3 offset = new Vector3((float)(r.NextDouble() - 0.5f), 0, (float)(r.NextDouble() - 0.5f));
        return pos + (offset / 5f);
    }

    private void OnDestroy()
    {
        mainThread.Interrupt();
    }
}
