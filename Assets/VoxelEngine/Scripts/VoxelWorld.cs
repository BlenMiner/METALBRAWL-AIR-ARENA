using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelWorld : MonoBehaviour
{
    public static float time;
    public static VoxelWorld singleton;
    public static int seed = 0;

    static Queue<VoxelChunk> dirtyChuncks = new Queue<VoxelChunk>();

    public int mapSize = 10;
    public GameObject chunkPrefab;

    private void Awake()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        singleton = this;
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

        foreach(var c in VoxelChunk.chunks)
        {
            c.Value.CreateMap();
        }

        yield return 0;

        foreach (var c in VoxelChunk.chunks)
        {
            c.Value.CreateMesh(null);
        }
    }

    public static void QueueDirtyChunk(VoxelChunk c)
    {
        dirtyChuncks.Enqueue(c);
    }

    private void Update()
    {
        time = Time.time;
        while (dirtyChuncks.Count != 0)
        {
            VoxelChunk chunk = dirtyChuncks.Dequeue();
            if (chunk == null) continue;
            UpdateChunk(chunk);
        }
    }

    private void UpdateChunk(VoxelChunk chunk)
    {
        chunk.threadReady = true;
        chunk.MeshReady();
    }

    static FastNoise fn = new FastNoise(0);
    public static Vector3 TransformPoint(Vector3 pos)
    {
        /*float noisex = fn.GetSimplex(pos.x, pos.y, pos.z) * 10f;
        float noisey = fn.GetSimplex(pos.y, pos.x, pos.z) * 10f;
        float noisez = fn.GetSimplex(pos.x, pos.z, pos.y) * 10f;*/

        //float noisex = fn.GetSimplex(pos.x * 100f, pos.y * 100f, pos.z * 100f);
        //Vector3 offset = new Vector3(noisex, noisex, noisex);

        System.Random r = new System.Random(Mathf.FloorToInt(pos.x + pos.y + pos.z));
        Vector3 offset = new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
        return pos + (offset / 5f);
    }
}
