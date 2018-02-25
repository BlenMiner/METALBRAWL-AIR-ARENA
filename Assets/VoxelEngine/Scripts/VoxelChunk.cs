using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelChunk : MonoBehaviour
{
    public const int chunkSize = 16;    
    public const int blocksPerUnit = 1;

    public const float worldScale = 5f;

    public static Dictionary<Vector3, VoxelChunk> chunks = new Dictionary<Vector3, VoxelChunk>();

    [System.NonSerialized]
    public Vector3 myPosition;
    [System.NonSerialized]
    public bool threadReady = true;

    public VMap map;
    public VMesh mesh;

    [System.NonSerialized]
    public bool chunkDirty = false;
    public bool chunkReady
    {
        get
        {
            VoxelChunk c0 = GetChunck(myPosition + new Vector3(chunkSize, 0, 0));
            VoxelChunk c1 = GetChunck(myPosition + new Vector3(-chunkSize, 0, 0));
            VoxelChunk c2 = GetChunck(myPosition + new Vector3(0, 0, chunkSize));
            VoxelChunk c3 = GetChunck(myPosition + new Vector3(0, 0, -chunkSize));
            VoxelChunk c4 = GetChunck(myPosition + new Vector3(0, chunkSize, 0));

            Vector3 down = myPosition + new Vector3(0, -chunkSize, 0);
            VoxelChunk c5 = GetChunck(myPosition + new Vector3(0, -chunkSize, 0));

            return
                c0 != null && c1 != null && c2 != null &&
                c3 != null && c4 != null && (down.y < 0 || c5 != null) &&
                c0.map.isReady &&
                c1.map.isReady &&
                c2.map.isReady &&
                c3.map.isReady &&
                c4.map.isReady &&
                (down.y < 0 || c5.map.isReady);
        }
    }

    void Awake()
    {
        myPosition = transform.position;

        if (chunks.ContainsKey(myPosition))
            chunks[myPosition] = this;
        else
            chunks.Add(myPosition, this);
    }
    
    public void CreateMap(object arg)
    {
        chunkDirty = true;

        map = new VMap(chunkSize * blocksPerUnit, this);
        map.GenMap();
    }
    public void CreateMesh(object lod)
    {
        threadReady = false;
        chunkDirty = false;

        VMesh mesh = new VMesh(this);
        
        for (int x = 0; x <= map.sizeX; x++)
        {
            for (int y = 0; y <= map.sizeY; y++)
            {
                for (int z = 0; z <= map.sizeZ; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    var block = map.GetBlock(pos);

                    if (block.isAirBlock) continue;

                    Vector3 vertexpos = pos / blocksPerUnit;
                    //mesh.SetDrawingColor(blockC);

                    if (map.GetBlock(pos + new Vector3(0, 1, 0)).isAirBlock)
                    {
                        //Top Face
                        mesh.AddQuad(
                            vertexpos + new Vector3(0, 1, 0) / blocksPerUnit,
                            vertexpos + new Vector3(0, 1, 1) / blocksPerUnit,
                            vertexpos + new Vector3(1, 1, 1) / blocksPerUnit,
                            vertexpos + new Vector3(1, 1, 0) / blocksPerUnit,
                            Vector3.up, block);
                    }
                    if (map.GetBlock(pos + new Vector3(0, -1, 0)).isAirBlock)
                    {
                        //Bottom Face
                        mesh.AddQuadFlipped(
                           vertexpos + new Vector3(0, 0, 0) / blocksPerUnit,
                           vertexpos + new Vector3(0, 0, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 0, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 0, 0) / blocksPerUnit,
                           Vector3.down, block);
                    }
                    if (map.GetBlock(pos + new Vector3(1, 0, 0)).isAirBlock)
                    {
                        //Right Face
                        mesh.AddQuadFlipped(
                           vertexpos + new Vector3(1, 0, 0) / blocksPerUnit,
                           vertexpos + new Vector3(1, 0, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 1, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 1, 0) / blocksPerUnit,
                           Vector3.right, block);
                    }
                    if (map.GetBlock(pos + new Vector3(-1, 0, 0)).isAirBlock)
                    {
                        //Left Face
                        mesh.AddQuad(
                           vertexpos + new Vector3(0, 0, 0) / blocksPerUnit,
                           vertexpos + new Vector3(0, 0, 1) / blocksPerUnit,
                           vertexpos + new Vector3(0, 1, 1) / blocksPerUnit,
                           vertexpos + new Vector3(0, 1, 0) / blocksPerUnit,
                           Vector3.left, block);
                    }
                    if (map.GetBlock(pos + new Vector3(0, 0, 1)).isAirBlock)
                    {
                        //Front Face
                        mesh.AddQuadFlipped(
                           vertexpos + new Vector3(0, 0, 1) / blocksPerUnit,
                           vertexpos + new Vector3(0, 1, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 1, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 0, 1) / blocksPerUnit,
                           Vector3.forward, block);
                    }
                    if (map.GetBlock(pos + new Vector3(0, 0, -1)).isAirBlock)
                    {
                        //Front Face
                        mesh.AddQuad(
                           vertexpos + new Vector3(0, 0, 0) / blocksPerUnit,
                           vertexpos + new Vector3(0, 1, 0) / blocksPerUnit,
                           vertexpos + new Vector3(1, 1, 0) / blocksPerUnit,
                           vertexpos + new Vector3(1, 0, 0) / blocksPerUnit,
                           Vector3.back, block);
                    }
                }
            }
        }
        
        mesh.GenArrays();
        this.mesh = mesh;

        VoxelWorld.QueueDirtyChunk(this);
    }

    public void MeshReady()
    {
        Mesh m = mesh.unityMesh;

        GetComponent<MeshFilter>().mesh = m;
        GetComponent<MeshCollider>().sharedMesh = m;
    }
    
    public static VoxelChunk GetChunck(Vector3 pos)
    {
        Vector3 absolutePos = ToChunkPos(pos);

        if (chunks.ContainsKey(absolutePos)) return chunks[absolutePos];
        return null;
    }

    public static Vector3 ToChunkPos(Vector3 pos)
    {
        return new Vector3(
            Mathf.Floor(pos.x / chunkSize) * chunkSize,
            Mathf.Floor(pos.y / chunkSize) * chunkSize,
            Mathf.Floor(pos.z / chunkSize) * chunkSize);
    }
}
