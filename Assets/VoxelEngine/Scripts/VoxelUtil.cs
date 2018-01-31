using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class VoxelUtil
{

}

public struct Vector3i
{
    public int x, y, z;
    public Vector3 vector3
    {
        get
        {
            return new Vector3(x, y, z);
        }
    }

    public Vector3i(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public Vector3i(int x)
    {
        this.x = x;
        this.y = x;
        this.z = x;
    }
    public Vector3i(Vector3 p)
    {
        this.x = (int)p.x;
        this.y = (int)p.y;
        this.z = (int)p.z;
    }
    public Vector3i(Vector3i addOnTop, int x, int y, int z)
    {
        this.x = addOnTop.x + x;
        this.y = addOnTop.y + y;
        this.z = addOnTop.z + z;
    }

    public static Vector3i operator +(Vector3i a, Vector3i b)
    {
        return new Vector3i(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static Vector3i operator -(Vector3i a, Vector3i b)
    {
        return new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3i operator *(Vector3i a, Vector3i b)
    {
        return new Vector3i(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    public static Vector3i operator /(Vector3i a, Vector3i b)
    {
        return new Vector3i(a.x / b.x, a.y / b.y, a.z / b.z);
    }
    public static Vector3i operator *(Vector3i a, int b)
    {
        return new Vector3i(a.x * b, a.y * b, a.z * b);
    }
    public static Vector3i operator /(Vector3i a, int b)
    {
        return new Vector3i(a.x / b, a.y / b, a.z / b);
    }

    public bool Equals(Vector3i obj)
    {
        return (obj.x == x && obj.y == y && obj.z == z);
    }

    public void Normalize()
    {
        Vector3 norm = new Vector3(x, y, z).normalized;
        x = (int)norm.x;
        x = (int)norm.y;
        x = (int)norm.z;
    }

    public Vector3 normalized
    {
        get
        {
            return new Vector3(x, y, z).normalized;
        }
    }

    public float magnitude
    {
        get
        {
            return new Vector3(x, y, z).magnitude;
        }
    }
}
public struct VMesh
{
    public int vertexCount;

    public List<Vector3> verticies;
    public List<Vector3> normals;
    public List<Vector2> uvs;
    public List<int> faces;
    public List<Color> colors;

    Vector3[] _verticies;
    Vector3[] _normals;
    Vector2[] _uvs;
    Color[] _colors;
    int[] _faces;
    VoxelChunk c;

    public Mesh unityMesh
    {
        get
        {
            Mesh m = new Mesh();

            m.vertices = _verticies;
            m.triangles = _faces;
            m.uv = _uvs;
            m.normals = _normals;
            m.RecalculateNormals();
            m.colors = _colors;
            return m;
        }
    }

    public VMesh(VoxelChunk c)
    {
        drawingColor = Color.white;
        vertexCount = 0;

        this.c = c;

        verticies = new List<Vector3>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();
        faces = new List<int>();
        colors = new List<Color>();

        _verticies = null;
        _normals = null;
        _uvs = null;
        _faces = null;
        _colors = null;
    }

    public void GenArrays()
    {
        _verticies = verticies.ToArray();
        _normals = normals.ToArray();
        _uvs = uvs.ToArray();
        _faces = faces.ToArray();
        _colors = colors.ToArray();
    }

    int AddVert(Vector3 pos)
    {
        vertexCount++;

        int i = verticies.Count;
        verticies.Add(pos);
        return i;
    }

    Color drawingColor;
    public void SetDrawingColor(Color c)
    {
        drawingColor = c;
    }

    public void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
    {
        int v0 = AddVert(VoxelWorld.TransformPoint(c, p0));
        int v1 = AddVert(VoxelWorld.TransformPoint(c, p1));
        int v2 = AddVert(VoxelWorld.TransformPoint(c, p2));
        int v3 = AddVert(VoxelWorld.TransformPoint(c, p3));

        faces.Add(v0);
        faces.Add(v1);
        faces.Add(v2);

        faces.Add(v0);
        faces.Add(v2);
        faces.Add(v3);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);

        colors.Add(drawingColor);
        colors.Add(drawingColor);
        colors.Add(drawingColor);
        colors.Add(drawingColor);
    }
    public void AddQuadFlipped(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
    {
        int v0 = AddVert(VoxelWorld.TransformPoint(c, p0));
        int v1 = AddVert(VoxelWorld.TransformPoint(c, p1));
        int v2 = AddVert(VoxelWorld.TransformPoint(c, p2));
        int v3 = AddVert(VoxelWorld.TransformPoint(c, p3));

        faces.Add(v2);
        faces.Add(v1);
        faces.Add(v0);

        faces.Add(v3);
        faces.Add(v2);
        faces.Add(v0);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);
        normals.Add(normal);

        colors.Add(drawingColor);
        colors.Add(drawingColor);
        colors.Add(drawingColor);
        colors.Add(drawingColor);
    }

    public void Clear()
    {
        verticies.Clear();
        normals.Clear();
        uvs.Clear();
        faces.Clear();
    }
}
public struct VMap
{
    public const int undergroundBlocks = 10;
    public const int montainsMaxHeight = 50;

    public uint size;
    public VoxelChunk chunk;
    Color[,,] map;

    bool ready;

    public bool startedGenerating;
    public bool isReady
    {
        get
        {
            return ready;
        }
    }

    public VMap(uint size, VoxelChunk chunk)
    {
        startedGenerating = false;
        ready = false;
        this.chunk = chunk;
        this.size = size;
        map = new Color[size, size, size];
    }

    public void GenMap()
    {
        startedGenerating = true;
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float noise = Mathf.PerlinNoise(
                    (chunk.myPosition.x + (x / (float)VoxelChunk.blocksPerUnit)) / 30f, 
                    (chunk.myPosition.z + (z / (float)VoxelChunk.blocksPerUnit)) / 30f);

                float noiseg = Mathf.PerlinNoise(
                    (chunk.myPosition.x + (x / (float)VoxelChunk.blocksPerUnit) + 200) / 30f,
                    (chunk.myPosition.z + (z / (float)VoxelChunk.blocksPerUnit) + 200) / 30f);

                float noiseb = Mathf.PerlinNoise(
                    (chunk.myPosition.x + (x / (float)VoxelChunk.blocksPerUnit) + 400) / 30f,
                    (chunk.myPosition.z + (z / (float)VoxelChunk.blocksPerUnit) + 400) / 30f);

                int height = undergroundBlocks + Mathf.FloorToInt(noise * montainsMaxHeight);

                for (int y = 0; y < size; y++)
                {
                    if((y + (chunk.myPosition.y * VoxelChunk.blocksPerUnit)) <= height)
                    {
                        map[x, y, z] = Color.white;// new Color(noise, noiseg, noiseb);
                    }
                }
            }
        }

        ready = true;
    }

    public Color GetBlock(Vector3 localPos)
    {
        Vector3i pos = new Vector3i(localPos);

        if (pos.y < 0) return Color.black;

        if (pos.x < 0 || pos.z < 0 ||
            pos.y >= size || pos.x >= size || pos.z >= size)
        {
            VoxelChunk c = VoxelChunk.GetChunck(chunk.myPosition + (localPos / VoxelChunk.blocksPerUnit));
            if (c == null || c == chunk) return new Color();// return Color.white;
            else
            {
                Vector3 local = (chunk.myPosition + (localPos / VoxelChunk.blocksPerUnit)) - c.myPosition;
                local *= VoxelChunk.blocksPerUnit;
                return c.map.GetBlock(local);
            }
        }
        return map[pos.x, pos.y, pos.z];
    }
}
public struct VDebug
{
    System.Diagnostics.Stopwatch watch;
    public long total;

    bool log;

    public VDebug(bool log)
    {
        watch = new System.Diagnostics.Stopwatch();
        total = 0;
        this.log = log;
    }

    public void Start()
    {
        watch = System.Diagnostics.Stopwatch.StartNew();
        total = 0;
    }
    public long Step()
    {
        long last = watch.ElapsedMilliseconds;
        total += last;
        watch.Reset();
        watch.Start();
        return last;
    }
    public void LogStep(string msg)
    {
        if (!log) return;
        Debug.Log(msg + Step() + " ms");
    }
    public long Finish()
    {
        long last = watch.ElapsedMilliseconds;
        total += last;
        watch.Stop();
        return last;
    }
}
public struct VThread
{
    public static void RunThread(WaitCallback thread, object args = null)
    {
        ThreadPool.QueueUserWorkItem(thread, args);
    }
}

public enum Face
{
    Top,
    Bottom,
    Right,
    Left,
    Front,
    Back
}
