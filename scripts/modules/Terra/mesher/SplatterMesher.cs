using Godot;

public class SplatterMesher
{
    static readonly Vector3[] Normals = new Vector3[]
    {
        new Vector3(0f, -1f, 0f), //Bottom
        new Vector3(0f, 1f, 0f), //Top
        new Vector3(1, 0, 0), //Right
        new Vector3(-1, 0, 0), //Left
        new Vector3(0, 0, 1), //front
        new Vector3(0, 0, -1) //back
    };

    ShaderMaterial chunkMaterial;
    float VOX_SIZE = Constants.VOXEL_SIZE;
    int CHUNK_SIZE = Constants.CHUNK_SIZE1D;

    private Registry registry;
    public Color[] voxelTypes = new Color[] {new Color(0f, 0f, 0.55f, 1f), new Color(1f, 1f, 1f, 1f)};

    public SplatterMesher(ShaderMaterial material, Registry registry)
    {
        this.registry = registry;
        chunkMaterial = material;
    }

    public void SetVoxelTypes(ref Color[] colorArr)
    {
        voxelTypes = colorArr;
    }

    public MeshInstance CreateChunkMesh(Chunk chunk)
    {
        MeshInstance instance = new MeshInstance();
        SurfaceTool surfacetool = new SurfaceTool();
        surfacetool.Begin(Mesh.PrimitiveType.Points);
        surfacetool.SetMaterial(chunkMaterial);
        int count = 0;

        for (int i = 0; i < Constants.CHUNK_SIZE3D / chunk.materials; i++)
        {
            uint objectID = chunk.voxels[i];
            TerraObject terraObject = registry.SelectByID((int) objectID);
            var x = i % CHUNK_SIZE;
            var y = (i / CHUNK_SIZE) % CHUNK_SIZE;
            var z = i / (CHUNK_SIZE * CHUNK_SIZE);
            if (chunk.voxels[i] != 0)
            {
                int face = 0b000000;
                //Left
                if (x == 0 || chunk.voxels[i - 1] != objectID)
                {
                    face = 0b000001;
                }

                //Right
                else if (x == 63 || chunk.voxels[i + 1] != objectID)
                {
                    face = 0b000010;
                }
                //Top
                else if (y == 63 || chunk.voxels[i + 64] != objectID)
                {
                    face = 0b000100;
                }
                //Bottom
                else if (y == 0 || chunk.voxels[i - 64] != objectID)
                {
                    face = 0b001000;
                }
                //Back
                else if (z == 63 || chunk.voxels[i + 4096] != objectID)
                {
                    face = 0b010000;
                }
                //Front
                else if (z == 0 || chunk.voxels[i - 4096] != objectID)
                {
                    face = 0b100000;
                }

                if (face != 0b000000)
                {
                    int counter = 0;
                    Vector3 normalAvg = Vector3.Zero;
                    for (int j = 0; j < 6; j++)
                    {
                        int bitFlagN = (face >> j) & 1;
                        if (bitFlagN == 1)
                        {
                            normalAvg = normalAvg + Normals[j];
                            counter += 1;
                        }
                    }

                    count += 1;
                    surfacetool.AddColor(new Color(1f, 1f, 1f, 1f));
                    Vector3 voxPosition = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z) * VOX_SIZE);
                    voxPosition.x = voxPosition.x + (chunk.x * CHUNK_SIZE * VOX_SIZE);
                    voxPosition.y = voxPosition.y + (chunk.y * CHUNK_SIZE * VOX_SIZE);
                    voxPosition.z = voxPosition.z + (chunk.z * CHUNK_SIZE * VOX_SIZE);
                    if (counter > 0)
                    {
                        normalAvg = normalAvg / counter;
                        surfacetool.AddNormal(normalAvg);
                    }

                    surfacetool.AddVertex(voxPosition);
                }
            }
        }

        surfacetool.Index();
        instance.SetMesh(surfacetool.Commit());
        surfacetool.Clear();
        instance.MaterialOverride = chunkMaterial.Duplicate() as ShaderMaterial;

        // Console.WriteLine("Mesh AABB Pos: {0} , Size: {1}, End: {2}",bb.Position,bb.Size,bb.End);
        return instance;
    }
}