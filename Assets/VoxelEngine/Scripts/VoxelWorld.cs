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

    public int mapSize = 10;
    public GameObject chunkPrefab;

    public Thread mainThread;

    private void Awake()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        singleton = this;

        mainThread = new Thread(VoxelUpdate);
        mainThread.Start();
    }
    
    IEnumerator Start ()
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

        yield return 0;

        /*foreach(var c in VoxelChunk.chunks)
        {
            VThread.RunThread(c.Value.CreateMap);
        }

        yield return 0;

        foreach (var c in VoxelChunk.chunks)
        {
            VThread.RunThread(c.Value.CreateMesh);
        }*/
    }

    public static void QueueDirtyChunk(VoxelChunk c)
    {
        dirtyChuncks.Enqueue(c);
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
    }

    private void VoxelUpdate()
    {
        while(true)
        {
            try
            {
                foreach (var c in VoxelChunk.chunks)
                {
                    if (!c.Value.map.startedGenerating)
                    {
                        c.Value.CreateMap(null);
                    }

                    if( c.Value.chunkDirty &&
                        c.Value.map.isReady &&
                        c.Value.chunkReady)
                    {
                        c.Value.CreateMesh(null);
                    }
                }
            }
            catch { }

            Thread.Sleep(50);
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
