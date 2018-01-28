using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider), typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelChunk : MonoBehaviour
{
    public const int chunkSize = 16;    
    public const int blocksPerUnit = 2;

    public const float worldScale = 5f;

    public static Dictionary<Vector3, VoxelChunk> chunks = new Dictionary<Vector3, VoxelChunk>();

    [System.NonSerialized]
    public Vector3 myPosition;
    [System.NonSerialized]
    public bool threadReady = true;

    public VMap map;
    public VMesh mesh;
    
    void Awake()
    {
        myPosition = transform.position;
        chunks.Add(myPosition, this);
    }

    public void CreateMap()
    {
        map = new VMap(chunkSize * blocksPerUnit, this);
        map.GenMap();
    }
    public void CreateMesh(object lod)
    {
        threadReady = false;

        VMesh mesh = new VMesh(this);
        
        for (int x = 0; x < map.size; x++)
        {
            for (int y = 0; y < map.size; y++)
            {
                for (int z = 0; z < map.size; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    Color blockC = map.GetBlock(pos);

                    if (blockC.a == 0) continue;

                    Vector3 vertexpos = pos / blocksPerUnit;
                    mesh.SetDrawingColor(blockC);

                    if (map.GetBlock(pos + new Vector3(0, 1, 0)).a == 0)
                    {
                        //Top Face
                        mesh.AddQuad(
                            vertexpos + new Vector3(0, 1, 0) / blocksPerUnit,
                            vertexpos + new Vector3(0, 1, 1) / blocksPerUnit,
                            vertexpos + new Vector3(1, 1, 1) / blocksPerUnit,
                            vertexpos + new Vector3(1, 1, 0) / blocksPerUnit,
                            Vector3.up);
                    }
                    if (map.GetBlock(pos + new Vector3(0, -1, 0)).a == 0)
                    {
                        //Bottom Face
                        mesh.AddQuadFlipped(
                           vertexpos + new Vector3(0, 0, 0) / blocksPerUnit,
                           vertexpos + new Vector3(0, 0, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 0, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 0, 0) / blocksPerUnit,
                           Vector3.down);
                    }
                    if (map.GetBlock(pos + new Vector3(1, 0, 0)).a == 0)
                    {
                        //Right Face
                        mesh.AddQuadFlipped(
                           vertexpos + new Vector3(1, 0, 0) / blocksPerUnit,
                           vertexpos + new Vector3(1, 0, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 1, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 1, 0) / blocksPerUnit,
                           Vector3.right);
                    }
                    if (map.GetBlock(pos + new Vector3(-1, 0, 0)).a == 0)
                    {
                        //Left Face
                        mesh.AddQuad(
                           vertexpos + new Vector3(0, 0, 0) / blocksPerUnit,
                           vertexpos + new Vector3(0, 0, 1) / blocksPerUnit,
                           vertexpos + new Vector3(0, 1, 1) / blocksPerUnit,
                           vertexpos + new Vector3(0, 1, 0) / blocksPerUnit,
                           Vector3.left);
                    }
                    if (map.GetBlock(pos + new Vector3(0, 0, 1)).a == 0)
                    {
                        //Front Face
                        mesh.AddQuadFlipped(
                           vertexpos + new Vector3(0, 0, 1) / blocksPerUnit,
                           vertexpos + new Vector3(0, 1, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 1, 1) / blocksPerUnit,
                           vertexpos + new Vector3(1, 0, 1) / blocksPerUnit,
                           Vector3.forward);
                    }
                    if (map.GetBlock(pos + new Vector3(0, 0, -1)).a == 0)
                    {
                        //Front Face
                        mesh.AddQuad(
                           vertexpos + new Vector3(0, 0, 0) / blocksPerUnit,
                           vertexpos + new Vector3(0, 1, 0) / blocksPerUnit,
                           vertexpos + new Vector3(1, 1, 0) / blocksPerUnit,
                           vertexpos + new Vector3(1, 0, 0) / blocksPerUnit,
                           Vector3.back);
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
        Vector3 absolutePos = new Vector3(
            Mathf.Floor(pos.x / chunkSize) * chunkSize,
            Mathf.Floor(pos.y / chunkSize) * chunkSize,
            Mathf.Floor(pos.z / chunkSize) * chunkSize);

        if (chunks.ContainsKey(absolutePos)) return chunks[absolutePos];
        return null;
    }
}
