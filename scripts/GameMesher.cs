using System.Linq;
using Godot;

public class GameMesher {
    private volatile Registry reg;
    private volatile Node parent;

    public GameMesher (Registry reg, Node parent, bool profile) {
        this.reg = reg;
        this.parent = parent;
    }

    public void MeshChunk (Chunk chunk) {

        RawChunk rawChunk = new RawChunk ();

        rawChunk.arrays = new Godot.Collections.Array[chunk.materials - 1];
        rawChunk.textures = new Texture[chunk.materials - 1];
        rawChunk.colliderFaces = new Vector3[chunk.materials - 1][];

        long a = 16777215 << 8;
        byte b = 255;

        if (chunk.materials > 1) {
            int[][][, , , ] vertices = new int[chunk.materials - 1][][, , , ];
            int count = 0;
            int[] indice = new int[chunk.materials - 1];

            for (int i = 0; i < Constants.CHUNK_SIZE3D; i++) {
                if (count >= Constants.CHUNK_SIZE3D) {
                    break;
                }

                uint bytes = chunk.voxels[i];

                int lenght = (int) (bytes & a) >> 8;
                int objectID = (int) (bytes & b);

                if (objectID == 0) {
                    count += lenght;
                    continue;
                }

                if (vertices[objectID - 1] == null) {
                    vertices[objectID - 1] = new int[6][, , , ];
                    for (int s = 0; s < 6; s++) {
                        vertices[objectID - 1][s] = new int[Constants.CHUNK_SIZE1D, Constants.CHUNK_SIZE1D, 6, 3];
                    }
                }

                int z = count / Constants.CHUNK_SIZE2D;
                int y = count % Constants.CHUNK_SIZE1D;
                int x = (count / Constants.CHUNK_SIZE1D) % Constants.CHUNK_SIZE1D;

                int ax = x + 1;
                int ay = lenght + y;
                int az = z + 1;

                //Front
                int[, , , ] vectors = vertices[objectID - 1][0];

                //1
                vectors[x, z, 0, 0] = x;
                vectors[x, z, 0, 1] = y;
                vectors[x, z, 0, 2] = z;

                //2
                vectors[x, z, 1, 0] = ax;
                vectors[x, z, 1, 1] = y;
                vectors[x, z, 1, 2] = z;

                //3
                vectors[x, z, 2, 0] = ax;
                vectors[x, z, 2, 1] = ay;
                vectors[x, z, 2, 2] = z;

                //4
                vectors[x, z, 3, 0] = ax;
                vectors[x, z, 3, 1] = ay;
                vectors[x, z, 3, 2] = z;

                //5
                vectors[x, z, 4, 0] = x;
                vectors[x, z, 4, 1] = ay;
                vectors[x, z, 4, 2] = z;

                //6
                vectors[x, z, 5, 0] = x;
                vectors[x, z, 5, 1] = y;
                vectors[x, z, 5, 2] = z;

                int pos = 1;
                if (z > 0 && vectors[x, z - pos, 2, 1] > 0) {

                    if (vectors[x, z - pos, 0, 0] < 0) {
                        pos = (int) - vectors[x, z - pos, 0, 0];
                    }

                    if (vectors[x, z - pos, 2, 1] >= ay) {
                        for (int s = 0; s < 6; s++) {
                            vectors[x, z, s, 0] = -pos;
                        }
                        indice[objectID - 1] -= 6;
                    } else if (vectors[x, z - pos, 2, 1] < ay && vectors[x, z - pos, 0, 1] >= y) {
                        vectors[x, z, 0, 1] = vectors[x, z - pos, 2, 1];
                        vectors[x, z, 1, 1] = vectors[x, z - pos, 2, 1];
                        vectors[x, z, 5, 1] = vectors[x, z - pos, 2, 1];
                    }
                }

                indice[objectID - 1] += 6;

                //Back
                vectors = vertices[objectID - 1][1];

                //1
                vectors[x, z, 0, 0] = ax;
                vectors[x, z, 0, 1] = y;
                vectors[x, z, 0, 2] = az;

                //2
                vectors[x, z, 1, 0] = x;
                vectors[x, z, 1, 1] = y;
                vectors[x, z, 1, 2] = az;

                //3
                vectors[x, z, 2, 0] = x;
                vectors[x, z, 2, 1] = ay;
                vectors[x, z, 2, 2] = az;

                //4
                vectors[x, z, 3, 0] = x;
                vectors[x, z, 3, 1] = ay;
                vectors[x, z, 3, 2] = az;

                //5
                vectors[x, z, 4, 0] = ax;
                vectors[x, z, 4, 1] = ay;
                vectors[x, z, 4, 2] = az;

                //6
                vectors[x, z, 5, 0] = ax;
                vectors[x, z, 5, 1] = y;
                vectors[x, z, 5, 2] = az;

                if (z > 0 && vectors[x, z - 1, 2, 1] > 0) {
                    if (vectors[x, z - 1, 2, 1] > ay && vectors[x, z - 1, 0, 1] <= y) {
                        vectors[x, z - 1, 0, 1] = ay;
                        vectors[x, z - 1, 1, 1] = ay;
                        vectors[x, z - 1, 5, 1] = ay;
                    } else if (vectors[x, z - 1, 2, 1] <= ay) {
                        for (int s = 0; s < 6; s++) {
                            vectors[x, z - 1, s, 0] = -147457;
                        }

                        indice[objectID - 1] -= 6;
                    }
                }

                indice[objectID - 1] += 6;

                //Right
                vectors = vertices[objectID - 1][2];

                //1
                vectors[x, z, 0, 0] = ax;
                vectors[x, z, 0, 1] = y;
                vectors[x, z, 0, 2] = z;

                //2
                vectors[x, z, 1, 0] = ax;
                vectors[x, z, 1, 1] = y;
                vectors[x, z, 1, 2] = az;

                //3
                vectors[x, z, 2, 0] = ax;
                vectors[x, z, 2, 1] = ay;
                vectors[x, z, 2, 2] = az;

                //4
                vectors[x, z, 3, 0] = ax;
                vectors[x, z, 3, 1] = ay;
                vectors[x, z, 3, 2] = az;

                //5
                vectors[x, z, 4, 0] = ax;
                vectors[x, z, 4, 1] = ay;
                vectors[x, z, 4, 2] = z;

                //6
                vectors[x, z, 5, 0] = ax;
                vectors[x, z, 5, 1] = y;
                vectors[x, z, 5, 2] = z;

                if (x > 0 && vectors[x - 1, z, 2, 1] > 0) {
                    if (vectors[x - 1, z, 2, 1] > ay && vectors[x - 1, z, 0, 1] <= y) {
                        vectors[x - 1, z, 0, 1] = ay;
                        vectors[x - 1, z, 1, 1] = ay;
                        vectors[x - 1, z, 5, 1] = ay;
                    } else if (vectors[x - 1, z, 2, 1] <= ay) {
                        for (int s = 0; s < 6; s++) {
                            vectors[x - 1, z, s, 0] = -147457;
                        }

                        indice[objectID - 1] -= 6;
                    }
                }

                indice[objectID - 1] += 6;

                //Left
                vectors = vertices[objectID - 1][3];

                //1
                vectors[x, z, 0, 0] = x;
                vectors[x, z, 0, 1] = y;
                vectors[x, z, 0, 2] = az;

                //2
                vectors[x, z, 1, 0] = x;
                vectors[x, z, 1, 1] = y;
                vectors[x, z, 1, 2] = z;

                //3
                vectors[x, z, 2, 0] = x;
                vectors[x, z, 2, 1] = ay;
                vectors[x, z, 2, 2] = z;

                //4
                vectors[x, z, 3, 0] = x;
                vectors[x, z, 3, 1] = ay;
                vectors[x, z, 3, 2] = z;

                //5
                vectors[x, z, 4, 0] = x;
                vectors[x, z, 4, 1] = ay;
                vectors[x, z, 4, 2] = az;

                //6
                vectors[x, z, 5, 0] = x;
                vectors[x, z, 5, 1] = y;
                vectors[x, z, 5, 2] = az;

                pos = 1;
                if (x > 0 && vectors[x - pos, z, 2, 1] > 0) {
                    if (vectors[x - pos, z, 0, 0] < 0) {
                        pos = (int) - vectors[x - pos, z, 0, 0];
                    }

                    if (vectors[x - pos, z, 2, 1] >= ay) {
                        for (int s = 0; s < 6; s++) {
                            vectors[x, z, s, 0] = -pos;
                        }

                        indice[objectID - 1] -= 6;
                    } else if (vectors[x - pos, z, 2, 1] < ay && vectors[x - pos, z, 0, 1] >= y) {
                        vectors[x, z, 0, 1] = vectors[x - pos, z, 2, 1];
                        vectors[x, z, 1, 1] = vectors[x - pos, z, 2, 1];
                        vectors[x, z, 5, 1] = vectors[x - pos, z, 2, 1];
                    }
                }

                indice[objectID - 1] += 6;

                //Top
                vectors = vertices[objectID - 1][4];

                int tx = x;
                if (x > 0 && vectors[x - 1, z, 0, 1] == ay) {
                    tx = vectors[x - 1, z, 0, 0];
                    for (int s = 0; s < 6; s++) {
                        vectors[x - 1, z, s, 0] = -147457;
                    }

                    indice[objectID - 1] -= 6;
                }

                //1
                vectors[x, z, 0, 0] = tx;
                vectors[x, z, 0, 1] = ay;
                vectors[x, z, 0, 2] = z;

                //2
                vectors[x, z, 1, 0] = ax;
                vectors[x, z, 1, 1] = ay;
                vectors[x, z, 1, 2] = z;

                //3
                vectors[x, z, 2, 0] = ax;
                vectors[x, z, 2, 1] = ay;
                vectors[x, z, 2, 2] = az;

                //4 
                vectors[x, z, 3, 0] = ax;
                vectors[x, z, 3, 1] = ay;
                vectors[x, z, 3, 2] = az;

                //5
                vectors[x, z, 4, 0] = tx;
                vectors[x, z, 4, 1] = ay;
                vectors[x, z, 4, 2] = az;

                //6
                vectors[x, z, 5, 0] = tx;
                vectors[x, z, 5, 1] = ay;
                vectors[x, z, 5, 2] = z;

                indice[objectID - 1] += 6;

                //Bottom
                vectors = vertices[objectID - 1][5];

                tx = x;
                if (x > 0 && vectors[x - 1, z, 0, 1] == y) {
                    tx = vectors[x - 1, z, 1, 0];
                    for (int s = 0; s < 6; s++) {
                        vectors[x - 1, z, s, 0] = -147457;
                    }

                    indice[objectID - 1] -= 6;
                }

                //1
                vectors[x, z, 0, 0] = ax;
                vectors[x, z, 0, 1] = y;
                vectors[x, z, 0, 2] = z;

                //2
                vectors[x, z, 1, 0] = tx;
                vectors[x, z, 1, 1] = y;
                vectors[x, z, 1, 2] = z;

                //3
                vectors[x, z, 2, 0] = tx;
                vectors[x, z, 2, 1] = y;
                vectors[x, z, 2, 2] = az;

                //4
                vectors[x, z, 3, 0] = tx;
                vectors[x, z, 3, 1] = y;
                vectors[x, z, 3, 2] = az;

                //5
                vectors[x, z, 4, 0] = ax;
                vectors[x, z, 4, 1] = y;
                vectors[x, z, 4, 2] = az;

                //6
                vectors[x, z, 5, 0] = ax;
                vectors[x, z, 5, 1] = y;
                vectors[x, z, 5, 2] = z;

                indice[objectID - 1] += 6;

                count += lenght;
            }

            for (int t = 0; t < chunk.materials - 1; t++) {
                int maxSize = indice[t];

                Texture texture = reg.SelectByID (t + 1).texture;
                Vector3[] vertice = new Vector3[maxSize];
                int[] indices = new int[maxSize];
                Vector3[] normals = new Vector3[maxSize];
                Vector2[] uvs = new Vector2[maxSize];
                float textureWidth = 2048f / texture.GetWidth ();
                float textureHeight = 2048f / texture.GetHeight ();

                if (maxSize > 0) {

                    int pos = 0;

                    for (int side = 0; side < 6; side++) {
                        int[, , , ] primitives = vertices[t][side];

                        for (int x = 0; x < Constants.CHUNK_SIZE1D; x++) {
                            for (int z = 0; z < Constants.CHUNK_SIZE1D; z++) {

                                if (primitives[x, z, 2, 1] > 0 && primitives[x, z, 0, 0] >= 0) {
                                    for (int s = 0; s < 6; s++) {
                                        if (pos < maxSize) {
                                            vertice[pos].x = primitives[x, z, s, 0] * Constants.VOXEL_SIZE;
                                            vertice[pos].y = primitives[x, z, s, 1] * Constants.VOXEL_SIZE;
                                            vertice[pos].z = primitives[x, z, s, 2] * Constants.VOXEL_SIZE;

                                            indices[pos] = pos;

                                            switch (side) {
                                                case 0:
                                                    normals[pos].x = 0f;
                                                    normals[pos].y = 0f;
                                                    normals[pos].z = -1f;
                                                    uvs[pos].x = vertice[pos].x * textureWidth;
                                                    uvs[pos].y = vertice[pos].y * textureHeight;
                                                    break;
                                                case 1:
                                                    normals[pos].x = 0f;
                                                    normals[pos].y = 0f;
                                                    normals[pos].z = 1f;
                                                    uvs[pos].x = vertice[pos].x * textureWidth;
                                                    uvs[pos].y = vertice[pos].y * textureHeight;
                                                    break;
                                                case 2:
                                                    normals[pos].x = -1f;
                                                    normals[pos].y = 0f;
                                                    normals[pos].z = 0f;
                                                    uvs[pos].x = vertice[pos].z * textureWidth;
                                                    uvs[pos].y = vertice[pos].y * textureHeight;
                                                    break;
                                                case 3:
                                                    normals[pos].x = 1f;
                                                    normals[pos].y = 0f;
                                                    normals[pos].z = 0f;
                                                    uvs[pos].x = vertice[pos].z * textureWidth;
                                                    uvs[pos].y = vertice[pos].y * textureHeight;
                                                    break;
                                                case 4:
                                                    normals[pos].x = 0f;
                                                    normals[pos].y = 1f;
                                                    normals[pos].z = 0f;
                                                    uvs[pos].x = vertice[pos].x * textureWidth;
                                                    uvs[pos].y = vertice[pos].z * textureHeight;
                                                    break;
                                                case 5:
                                                    normals[pos].x = 0f;
                                                    normals[pos].y = -1f;
                                                    normals[pos].z = 0f;
                                                    uvs[pos].x = vertice[pos].x * textureWidth;
                                                    uvs[pos].y = vertice[pos].z * textureHeight;
                                                    break;
                                            }
                                            pos++;
                                        } else {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                Godot.Collections.Array godotArray = new Godot.Collections.Array ();
                godotArray.Resize (9);

                godotArray[0] = vertice;
                godotArray[1] = normals;
                godotArray[4] = uvs;
                godotArray[8] = indices;

                rawChunk.arrays[t] = godotArray;
                rawChunk.textures[t] = texture;
                rawChunk.colliderFaces[t] = vertice;
            }
        } else {
            Godot.Collections.Array godotArray = new Godot.Collections.Array ();
            godotArray.Resize (9);
            rawChunk.arrays = new Godot.Collections.Array[1];
            rawChunk.textures = new Texture[1];
            rawChunk.colliderFaces = new Vector3[1][];
            uint bytes = chunk.voxels[0];
            int objectID = (int) (bytes & b);

            Texture texture = reg.SelectByID (objectID).texture;
            SpatialMaterial material = new SpatialMaterial ();

            float textureWidth = 2048f / texture.GetWidth ();
            float textureHeight = 2048f / texture.GetHeight ();

            Vector3[] vertice = new Vector3[36];
            Vector3[] normals = new Vector3[36];
            Vector2[] uvs = new Vector2[36];

            //FRONT
            vertice[0] = new Vector3 (0, 0, 0);
            vertice[1] = new Vector3 (Constants.CHUNK_LENGHT, 0, 0);
            vertice[2] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);
            vertice[3] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);
            vertice[4] = new Vector3 (0, Constants.CHUNK_LENGHT, 0);
            vertice[5] = new Vector3 (0, 0, 0);

            for (int i = 0; i < 6; i++) {
                normals[i] = new Vector3 (0, 0, -1);
                uvs[i].x = vertice[i].x * textureWidth;
                uvs[i].y = vertice[i].y * textureHeight;
            }

            //BACK
            vertice[6] = new Vector3 (Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);
            vertice[7] = new Vector3 (0, 0, Constants.CHUNK_LENGHT);
            vertice[8] = new Vector3 (0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertice[9] = new Vector3 (0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertice[10] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertice[11] = new Vector3 (Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);

            for (int i = 6; i < 12; i++) {
                normals[i] = new Vector3 (0, 0, 1);
                uvs[i].x = vertice[i].x * textureWidth;
                uvs[i].y = vertice[i].y * textureHeight;
            }

            //LEFT
            vertice[12] = new Vector3 (0, 0, Constants.CHUNK_LENGHT);
            vertice[13] = new Vector3 (0, 0, 0);
            vertice[14] = new Vector3 (0, Constants.CHUNK_LENGHT, 0);
            vertice[15] = new Vector3 (0, Constants.CHUNK_LENGHT, 0);
            vertice[16] = new Vector3 (0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertice[17] = new Vector3 (0, 0, Constants.CHUNK_LENGHT);

            for (int i = 12; i < 18; i++) {
                normals[i] = new Vector3 (1, 0, 0);
                uvs[i].x = vertice[i].z * textureWidth;
                uvs[i].y = vertice[i].y * textureHeight;
            }

            //RIGHT
            vertice[18] = new Vector3 (Constants.CHUNK_LENGHT, 0, 0);
            vertice[19] = new Vector3 (Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);
            vertice[20] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertice[21] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertice[22] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);
            vertice[23] = new Vector3 (Constants.CHUNK_LENGHT, 0, 0);

            for (int i = 18; i < 24; i++) {
                normals[i] = new Vector3 (-1, 0, 0);
                uvs[i].x = vertice[i].z * textureWidth;
                uvs[i].y = vertice[i].y * textureHeight;
            }

            // TOP
            vertice[24] = new Vector3 (0, Constants.CHUNK_LENGHT, 0);
            vertice[25] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);
            vertice[26] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertice[27] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertice[28] = new Vector3 (0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
            vertice[29] = new Vector3 (0, Constants.CHUNK_LENGHT, 0);

            for (int i = 24; i < 30; i++) {
                normals[i] = new Vector3 (0, 1, 0);
                uvs[i].x = vertice[i].x * textureWidth;
                uvs[i].y = vertice[i].z * textureHeight;
            }

            //BOTTOM
            vertice[30] = new Vector3 (Constants.CHUNK_LENGHT, 0, 0);
            vertice[31] = new Vector3 (0, 0, 0);
            vertice[32] = new Vector3 (0, 0, Constants.CHUNK_LENGHT);
            vertice[33] = new Vector3 (0, 0, Constants.CHUNK_LENGHT);
            vertice[34] = new Vector3 (Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);
            vertice[35] = new Vector3 (Constants.CHUNK_LENGHT, 0, 0);

            for (int i = 30; i < 36; i++) {
                normals[i] = new Vector3 (0, -1, 0);
                uvs[i].x = vertice[i].x * textureWidth;
                uvs[i].y = vertice[i].z * textureHeight;
            }

            godotArray[0] = vertice;
            godotArray[1] = normals;
            godotArray[4] = uvs;

            rawChunk.arrays[0] = godotArray;
            rawChunk.textures[0] = texture;
            rawChunk.colliderFaces[0] = vertice;
        }

        MeshInstance meshInstance = new MeshInstance ();
        ArrayMesh mesh = new ArrayMesh ();
        StaticBody body = new StaticBody ();

        for (int t = 0; t < rawChunk.arrays.Count (); t++) {
            Texture texture = rawChunk.textures[t];
            Vector3[] vertice = rawChunk.colliderFaces[t];
            Godot.Collections.Array godotArray = rawChunk.arrays[t];

            SpatialMaterial material = new SpatialMaterial ();
            texture.Flags = 2;
            material.AlbedoTexture = texture;

            ConcavePolygonShape shape = new ConcavePolygonShape ();
            shape.Data = vertice;

            mesh.AddSurfaceFromArrays (Mesh.PrimitiveType.Triangles, godotArray);
            mesh.SurfaceSetMaterial (mesh.GetSurfaceCount () - 1, material);
            CollisionShape colShape = new CollisionShape ();
            colShape.Shape = shape;
            body.AddChild (colShape);
        }

        meshInstance.AddChild (body);
        meshInstance.Mesh = mesh;

        meshInstance.Name = "chunk:" + chunk.x + "," + chunk.y + "," + chunk.z;
        meshInstance.Translation = new Vector3 (chunk.x, chunk.y, chunk.z);

        parent.CallDeferred ("add_child", meshInstance);
    }
}