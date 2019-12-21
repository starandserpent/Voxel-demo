using System.Collections.Concurrent;
using Godot;
using System.Buffers;

public class GameMesher
{
    private SplatterMesher splatterMesher;
    private Registry reg;
   public GameMesher(Registry reg, bool profile)
    {
        ShaderMaterial shaderMat = new ShaderMaterial();
        shaderMat.Shader = (GD.Load("res://assets/shaders/splatvoxel.shader") as Shader);
        splatterMesher = new SplatterMesher(shaderMat, reg);
    }


    public MeshInstance MeshChunk(RawChunk chunk)
    {
        MeshInstance meshInstance = Cull(chunk);

        meshInstance.Name = "chunk:" + chunk.x + "," + chunk.y + "," + chunk.z;
        meshInstance.Translation = new Vector3(chunk.x, chunk.y, chunk.z);
        return meshInstance;
    }

    public static RawChunk CreateRawChunk(Chunk chunk){
        Vector3[][] vertices = new Vector3[chunk.materials - 1][];
        long a = 16777215 << 8;
        byte b = 255;

        int count = 0;
        int[] indice = new int[chunk.materials - 1];
        int[] arraySize = new int[chunk.materials - 1];

        if (chunk.materials > 1)
        {
            for (int i = 0; i < Constants.CHUNK_SIZE3D; i++)
            {
                if (count >= Constants.CHUNK_SIZE3D)
                {
                    break;
                }

                uint bytes = chunk.voxels[i];

                int lenght = (int) (bytes & a) >> 8;
                int objectID = (int) (bytes & b);

                if (objectID == 0)
                {
                    count += lenght;
                    continue;
                }

                if (vertices[objectID - 1] == null)
                {
                    Vector3[] buffer = ArrayPool<Vector3>.Shared.Rent((Constants.CHUNK_SIZE3D / chunk.materials) * 6);
                    vertices[objectID - 1] = buffer;
                }

                int z = count / Constants.CHUNK_SIZE2D;
                int y = count % Constants.CHUNK_SIZE1D;
                int x = (count / Constants.CHUNK_SIZE1D) % Constants.CHUNK_SIZE1D;

                float sx = x * Constants.VOXEL_SIZE;
                float sy = y * Constants.VOXEL_SIZE;
                float sz = z * Constants.VOXEL_SIZE;

                float ax = (x + 1) * Constants.VOXEL_SIZE;
                float ay = (lenght + y) * Constants.VOXEL_SIZE;
                float az = (z + 1) * Constants.VOXEL_SIZE;

                //Front
                Vector3[] vectors = vertices[objectID - 1];

                int index = indice[objectID - 1];
                //Front
                //1
                vectors[index].x = sx;
                vectors[index].y = sy;
                vectors[index].z = sz;

                //2
                vectors[index + 1].x = ax;
                vectors[index + 1].y = sy;
                vectors[index + 1].z = sz;

                //3
                vectors[index + 2].x = ax;
                vectors[index + 2].y = ay;
                vectors[index + 2].z = sz;

                //4
                vectors[index + 3].x = ax;
                vectors[index + 3].y = ay;
                vectors[index + 3].z = sz;

                //5
                vectors[index + 4].x = sx;
                vectors[index + 4].y = ay;
                vectors[index + 4].z = sz;

                //6
                vectors[index + 5].x = sx;
                vectors[index + 5].y = sy;
                vectors[index + 5].z = sz;

                if (z > 0 && index > (36 * Constants.CHUNK_SIZE1D))
                {
                    int pos = index - (36 * Constants.CHUNK_SIZE1D) + 2;
                    if (vectors[pos].x < 0)
                    {
                        pos = (int) -vectors[pos].x;
                    }

                    if (vectors[index + 2].y <= vectors[pos].y)
                    {
                        for (int s = 0; s < 6; s++)
                        {
                            vectors[index + s].x = -pos;
                        }

                        arraySize[objectID - 1] -= 6;
                    }

                    if (vectors[pos].y < vectors[index + 2].y && vectors[pos + 2].y >= vectors[index].y)
                    {
                        vectors[index].y = vectors[pos].y;
                        vectors[index + 1].y = vectors[pos].y;
                        vectors[index + 5].y = vectors[pos].y;
                    }
                }

                index += 6;

                //Back
                //1
                vectors[index].x = ax;
                vectors[index].y = sy;
                vectors[index].z = az;

                //2
                vectors[index + 1].x = sx;
                vectors[index + 1].y = sy;
                vectors[index + 1].z = az;

                //3
                vectors[index + 2].x = sx;
                vectors[index + 2].y = ay;
                vectors[index + 2].z = az;

                //4
                vectors[index + 3].x = sx;
                vectors[index + 3].y = ay;
                vectors[index + 3].z = az;

                //5
                vectors[index + 4].x = ax;
                vectors[index + 4].y = ay;
                vectors[index + 4].z = az;

                //6
                vectors[index + 5].x = ax;
                vectors[index + 5].y = sy;
                vectors[index + 5].z = az;

                if (z > 0 && index > (36 * Constants.CHUNK_SIZE1D))
                {
                    int pos = index - (36 * Constants.CHUNK_SIZE1D);
                    if (vectors[pos + 2].y > ay && vectors[pos].y <= sy)
                    {
                        vectors[pos].y = ay;
                        vectors[pos + 1].y = ay;
                        vectors[pos + 5].y = ay;
                    }
                    else if (vectors[pos + 2].y <= ay && vectors[pos].x == ax)
                    {
                        for (int s = 0; s < 6; s++)
                        {
                            vectors[index - ((36 * Constants.CHUNK_SIZE1D) - s)].x = -147457;
                        }

                        arraySize[objectID - 1] -= 6;
                    }
                }

                index += 6;

                //Left
                //1
                vectors[index].x = ax;
                vectors[index].y = sy;
                vectors[index].z = sz;

                //2
                vectors[index + 1].x = ax;
                vectors[index + 1].y = sy;
                vectors[index + 1].z = az;

                //3
                vectors[index + 2].x = ax;
                vectors[index + 2].y = ay;
                vectors[index + 2].z = az;

                //4
                vectors[index + 3].x = ax;
                vectors[index + 3].y = ay;
                vectors[index + 3].z = az;

                //5
                vectors[index + 4].x = ax;
                vectors[index + 4].y = ay;
                vectors[index + 4].z = sz;

                //6
                vectors[index + 5].x = ax;
                vectors[index + 5].y = sy;
                vectors[index + 5].z = sz;

                if (x > 0 && index > 36)
                {
                    if (vectors[index - 34].y > ay && vectors[index - 36].y <= sy)
                    {
                        vectors[index - 36].y = ay;
                        vectors[index - 35].y = ay;
                        vectors[index - 31].y = ay;
                    }
                    else if (vectors[index - 34].y <= ay && vectors[index - 34].x == sx)
                    {
                        for (int s = 0; s < 6; s++)
                        {
                            vectors[index - (31 + s)].x = -147457;
                        }

                        arraySize[objectID - 1] -= 6;
                    }
                }

                index += 6;

                //Right
                //1
                vectors[index].x = sx;
                vectors[index].y = sy;
                vectors[index].z = az;

                //2
                vectors[index + 1].x = sx;
                vectors[index + 1].y = sy;
                vectors[index + 1].z = sz;

                //3
                vectors[index + 2].x = sx;
                vectors[index + 2].y = ay;
                vectors[index + 2].z = sz;

                //4
                vectors[index + 3].x = sx;
                vectors[index + 3].y = ay;
                vectors[index + 3].z = sz;

                //5
                vectors[index + 4].x = sx;
                vectors[index + 4].y = ay;
                vectors[index + 4].z = az;

                //6
                vectors[index + 5].x = sx;
                vectors[index + 5].y = sy;
                vectors[index + 5].z = az;

                if (x > 0 && index > 36)
                {
                    int pos = index - 34;
                    if (vectors[pos].x < 0)
                    {
                        pos = (int) -vectors[pos].x;
                    }

                    if (vectors[index + 2].y <= vectors[pos].y && vectors[pos].z == sz)
                    {
                        for (int s = 0; s < 6; s++)
                        {
                            vectors[index + s].x = -pos;
                        }

                        arraySize[objectID - 1] -= 6;
                    }

                    if (vectors[pos].y < vectors[index + 2].y && vectors[pos + 2].y >= vectors[index].y)
                    {
                        vectors[index].y = vectors[pos].y;
                        vectors[index + 1].y = vectors[pos].y;
                        vectors[index + 5].y = vectors[pos].y;
                    }
                }

                index += 6;

                //Top
                float tx = sx;
                float tz = sz;
                if (x > 0 && index > 36 && vectors[index - 36].y == ay && vectors[index - 35].x == sx &&
                    vectors[index - 34].x == sx)
                {
                    tx = vectors[index - 36].x;
                    for (int s = 0; s < 6; s++)
                    {
                        vectors[index - (31 + s)].x = -147457;
                    }

                    arraySize[objectID - 1] -= 6;
                }

                //1
                vectors[index].x = tx;
                vectors[index].y = ay;
                vectors[index].z = tz;

                //2
                vectors[index + 1].x = ax;
                vectors[index + 1].y = ay;
                vectors[index + 1].z = tz;

                //3
                vectors[index + 2].x = ax;
                vectors[index + 2].y = ay;
                vectors[index + 2].z = az;

                //4 
                vectors[index + 3].x = ax;
                vectors[index + 3].y = ay;
                vectors[index + 3].z = az;

                //5
                vectors[index + 4].x = tx;
                vectors[index + 4].y = ay;
                vectors[index + 4].z = az;

                //6
                vectors[index + 5].x = tx;
                vectors[index + 5].y = ay;
                vectors[index + 5].z = tz;

                index += 6;
                //Bottom
                tx = sx;
                tz = sz;
                if (x > 0 && index > 36 && vectors[index - 35].y == sy && vectors[index - 36].x == sx &&
                    vectors[index - 32].x == sx)
                {
                    tx = vectors[index - 35].x;
                    for (int s = 0; s < 6; s++)
                    {
                        vectors[index - (31 + s)].x = -147457;
                    }

                    arraySize[objectID - 1] -= 6;
                }

                //1
                vectors[index].x = ax;
                vectors[index].y = sy;
                vectors[index].z = tz;

                //2
                vectors[index + 1].x = tx;
                vectors[index + 1].y = sy;
                vectors[index + 1].z = tz;

                //3
                vectors[index + 2].x = tx;
                vectors[index + 2].y = sy;
                vectors[index + 2].z = az;

                //4
                vectors[index + 3].x = tx;
                vectors[index + 3].y = sy;
                vectors[index + 3].z = az;

                //5
                vectors[index + 4].x = ax;
                vectors[index + 4].y = sy;
                vectors[index + 4].z = az;

                //6
                vectors[index + 5].x = ax;
                vectors[index + 5].y = sy;
                vectors[index + 5].z = tz;
                index += 6;

                indice[objectID - 1] = index;
                count += lenght;
            }
        }else{
            int id = (int)chunk.voxels[0] - 1;
           //FRONT
            vertices[id][0] = new Vector3(0, 0, 0);
            vertices[id][1] = new Vector3(Constants.CHUNK_LENGHT, 0, 0);
            vertices[id][2] = new Vector3(Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);
            vertices[id][3] = new Vector3(Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);
            vertices[id][4] = new Vector3(0, Constants.CHUNK_LENGHT, 0);
            vertices[id][5] = new Vector3(0, 0, 0);

                      //BACK
            vertices[id][6] = new Vector3(Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);
            vertices[id][7] = new Vector3(0, 0, Constants.CHUNK_LENGHT);
            vertices[id][8] = new Vector3(0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertices[id][9] = new Vector3(0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertices[id][10] = new Vector3(Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertices[id][11] = new Vector3(Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);

                        //LEFT
            vertices[id][12] = new Vector3(0, 0, Constants.CHUNK_LENGHT);
            vertices[id][13] = new Vector3(0, 0, 0);
            vertices[id][14] = new Vector3(0, Constants.CHUNK_LENGHT, 0);
            vertices[id][15] = new Vector3(0, Constants.CHUNK_LENGHT, 0);
            vertices[id][16] = new Vector3(0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertices[id][17] = new Vector3(0, 0, Constants.CHUNK_LENGHT);

                        //RIGHT
            vertices[id][18] = new Vector3(Constants.CHUNK_LENGHT, 0, 0);
            vertices[id][19] = new Vector3(Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);
            vertices[id][20] = new Vector3(Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertices[id][21] = new Vector3(Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertices[id][22] = new Vector3(Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);
            vertices[id][23] = new Vector3(Constants.CHUNK_LENGHT, 0, 0);

                        // TOP
            vertices[id][24] = new Vector3(0, Constants.CHUNK_LENGHT, 0);
            vertices[id][25] = new Vector3(Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);
            vertices[id][26] = new Vector3(Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertices[id][27] = new Vector3(Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertices[id][28] = new Vector3(0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertices[id][29] = new Vector3(0, Constants.CHUNK_LENGHT, 0);

            
            //BOTTOM
            vertices[id][30] = new Vector3(Constants.CHUNK_LENGHT, 0, 0);
            vertices[id][31] = new Vector3(0, 0, 0);
            vertices[id][32] = new Vector3(0, 0, Constants.CHUNK_LENGHT);
            vertices[id][33] = new Vector3(0, 0, Constants.CHUNK_LENGHT);
            vertices[id][34] = new Vector3(Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);
            vertices[id][35] = new Vector3(Constants.CHUNK_LENGHT, 0, 0);

            indice[0] = 36;
        }

        RawChunk rawChunk = new RawChunk();
        rawChunk.arraySize = arraySize;
        rawChunk.primitives = vertices;
        rawChunk.materials = chunk.materials;
        rawChunk.indice = indice;
        rawChunk.x = chunk.x;
        rawChunk.y = chunk.y;
        rawChunk.z = chunk.z;
        
        return rawChunk;
    }

    private  MeshInstance Cull(RawChunk chunk)
    {
        MeshInstance meshInstance = new MeshInstance();
        Godot.Collections.Array godotArray = new Godot.Collections.Array();
        godotArray.Resize(9);
        ArrayMesh mesh = new ArrayMesh();
        StaticBody body = new StaticBody();
        int[] arraySize = chunk.arraySize;
        int[] indice = chunk.indice;
 
            for (int t = 0; t < chunk.materials - 1; t++)
            {
                int size = indice[t];
                if (size + arraySize[t] > 0)
                {
                    Texture texture = reg.SelectByID(t + 1).texture;
                    int[] indices = new int[(size + arraySize[t])];
                    Vector3[] vertice = new Vector3[size + arraySize[t]];
                    Vector3[] normals = new Vector3[size + arraySize[t]];
                    Vector2[] uvs = new Vector2[size + arraySize[t]];

                    float textureWidth = 2048f / texture.GetWidth();
                    float textureHeight = 2048f / texture.GetHeight();

                    Vector3[] primitives = chunk.primitives[t];
                    int pos = 0;
                    for (int i = 0; i < size; i++)
                    {
                        Vector3 vector = primitives[i];
                        if (vector.x >= 0 && pos < size + arraySize[t])
                        {
                            int s = ((i % 36)) / 6;
                            vertice[pos].x = vector.x;
                            vertice[pos].y = vector.y;
                            vertice[pos].z = vector.z;

                            indices[pos] = pos;

                            switch (s)
                            {
                                case 0:
                                    normals[pos].x = 0f;
                                    normals[pos].y = 0f;
                                    normals[pos].z = -1f;
                                    uvs[pos].x = vector.x * textureWidth;
                                    uvs[pos].y = vector.y * textureHeight;
                                    break;
                                case 1:
                                    normals[pos].x = 0f;
                                    normals[pos].y = 0f;
                                    normals[pos].z = 1f;
                                    uvs[pos].x = vector.x * textureWidth;
                                    uvs[pos].y = vector.y * textureHeight;
                                    break;
                                case 2:
                                    normals[pos].x = -1f;
                                    normals[pos].y = 0f;
                                    normals[pos].z = 0f;
                                    uvs[pos].x = vector.z * textureWidth;
                                    uvs[pos].y = vector.y * textureHeight;
                                    break;
                                case 3:
                                    normals[pos].x = 1f;
                                    normals[pos].y = 0f;
                                    normals[pos].z = 0f;
                                    uvs[pos].x = vector.z * textureWidth;
                                    uvs[pos].y = vector.y * textureHeight;
                                    break;
                                case 4:
                                    normals[pos].x = 0f;
                                    normals[pos].y = 1f;
                                    normals[pos].z = 0f;
                                    uvs[pos].x = vector.x * textureWidth;
                                    uvs[pos].y = vector.z * textureHeight;
                                    break;
                                case 5:
                                    normals[pos].x = 0f;
                                    normals[pos].y = -1f;
                                    normals[pos].z = 0f;
                                    uvs[pos].x = vector.x * textureWidth;
                                    uvs[pos].y = vector.z * textureHeight;
                                    break;
                            }

                            pos++;
                        }
                    }

                    godotArray[0] = vertice;
                    godotArray[1] = normals;
                    godotArray[4] = uvs;
                    godotArray[8] = indices;

                    SpatialMaterial material = new SpatialMaterial();
                    texture.Flags = 2;
                    material.AlbedoTexture = texture;

                    ConcavePolygonShape shape = new ConcavePolygonShape();
                    shape.SetFaces(vertice);
                    ArrayPool<Vector3>.Shared.Return(primitives);

                    mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, godotArray);
                    mesh.SurfaceSetMaterial(mesh.GetSurfaceCount() - 1, material);
                    CollisionShape colShape = new CollisionShape();
                    colShape.SetShape(shape);
                    body.AddChild(colShape);
                }
            }


            meshInstance.CallDeferred("add_child", body);
            meshInstance.Mesh = mesh;
            return meshInstance;
        }
}