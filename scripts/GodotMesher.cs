using System.Linq;
using Godot;

public class GodotMesher : Spatial {
    private volatile Registry reg;
    public void Set (Registry reg) {
        this.reg = reg;
    }

    public void MeshChunk (Chunk chunk) {

        RawChunk rawChunk = new RawChunk ();

        rawChunk.arrays = new Godot.Collections.Array[chunk.materials - 1];
        rawChunk.materials = new SpatialMaterial[chunk.materials - 1];
        rawChunk.colliderFaces = new Vector3[chunk.materials - 1][];

        if (chunk.materials > 1) {
            rawChunk = NaiveGreedyMesher (chunk, rawChunk);
        } else {
            rawChunk = FastGodotCube (chunk, rawChunk);
        }

        RID meshID = VisualServer.MeshCreate ();
        //RID body = PhysicsServer.BodyCreate (PhysicsServer.BodyMode.Static);

        for (int t = 0; t < rawChunk.arrays.Count (); t++) {
            SpatialMaterial material = rawChunk.materials[t];

            Vector3[] vertice = rawChunk.colliderFaces[t];
            Godot.Collections.Array godotArray = rawChunk.arrays[t];

            /*  RID shape = PhysicsServer.ShapeCreate (PhysicsServer.ShapeType.ConcavePolygon);
//            PhysicsServer.ShapeSetData (shape, vertice);

            PhysicsServer.BodyAddShape (body, shape, new Transform (Transform.basis, new Vector3 (chunk.x, chunk.y, chunk.z)));
*/
            VisualServer.MeshAddSurfaceFromArrays (meshID, VisualServer.PrimitiveType.Triangles, godotArray);
            VisualServer.MeshSurfaceSetMaterial (meshID, VisualServer.MeshGetSurfaceCount (meshID) - 1, material.GetRid ());
        }

        RID instance = VisualServer.InstanceCreate ();
        VisualServer.InstanceSetBase (instance, meshID);
        VisualServer.InstanceSetTransform (instance, new Transform (Transform.basis, new Vector3 (chunk.x, chunk.y, chunk.z)));
        VisualServer.InstanceSetScenario (instance, GetWorld ().Scenario);
        //   PhysicsServer.BodySetSpace (body, GetWorld ().Space);
    }

    public RawChunk NaiveGreedyMesher (Chunk chunk, RawChunk rawChunk) {
        MeshedValues values = Mesher.NaiveGreedyMeshing (chunk);
        for (int t = 0; t < chunk.materials - 1; t++) {
            int maxSize = values.indices[t];

            SpatialMaterial material = reg.SelectByID (t + 1).material;
            Vector3[] vertice = new Vector3[maxSize];
            int[] indices = new int[maxSize + (maxSize / 2)];
            Vector3[] normals = new Vector3[maxSize];
            Vector2[] uvs = new Vector2[maxSize];
            float textureWidth = 2048f / material.AlbedoTexture.GetWidth ();
            float textureHeight = 2048f / material.AlbedoTexture.GetHeight ();

            if (maxSize > 0) {

                int pos = 0;
                int index = 0;

                for (int side = 0; side < 6; side++) {
                    int[, , , ] primitives = values.vertices[t][side];

                    for (int x = 0; x < Constants.CHUNK_SIZE1D; x++) {
                        for (int z = 0; z < Constants.CHUNK_SIZE1D; z++) {
                            int prevPos = pos;

                            if (primitives[x, z, 2, 1] > 0 && primitives[x, z, 0, 0] >= 0 || primitives[x, z, 0, 0] > 0) {
                                for (int s = 0; s < 4; s++) {
                                    if (pos < maxSize) {
                                        vertice[pos].x = primitives[x, z, s, 0] * Constants.VOXEL_SIZE;
                                        vertice[pos].y = primitives[x, z, s, 1] * Constants.VOXEL_SIZE;
                                        vertice[pos].z = primitives[x, z, s, 2] * Constants.VOXEL_SIZE;

                                        switch (side) {
                                            case 0:
                                                //Front
                                                normals[pos].x = 0f;
                                                normals[pos].y = 0f;
                                                normals[pos].z = -1f;
                                                uvs[pos].x = vertice[pos].x * textureWidth;
                                                uvs[pos].y = vertice[pos].y * textureHeight;
                                                break;
                                            case 1:
                                                //Back
                                                normals[pos].x = 0f;
                                                normals[pos].y = 0f;
                                                normals[pos].z = 1f;
                                                uvs[pos].x = vertice[pos].x * textureWidth;
                                                uvs[pos].y = vertice[pos].y * textureHeight;
                                                break;
                                            case 2:
                                                //Right
                                                normals[pos].x = -1f;
                                                normals[pos].y = 0f;
                                                normals[pos].z = 0f;
                                                uvs[pos].x = vertice[pos].z * textureWidth;
                                                uvs[pos].y = vertice[pos].y * textureHeight;
                                                break;
                                            case 3:
                                                //Left
                                                normals[pos].x = 1f;
                                                normals[pos].y = 0f;
                                                normals[pos].z = 0f;
                                                uvs[pos].x = vertice[pos].z * textureWidth;
                                                uvs[pos].y = vertice[pos].y * textureHeight;
                                                break;
                                            case 4:
                                                //Top
                                                normals[pos].x = 0f;
                                                normals[pos].y = 1f;
                                                normals[pos].z = 0f;
                                                uvs[pos].x = vertice[pos].x * textureWidth;
                                                uvs[pos].y = vertice[pos].z * textureHeight;
                                                break;
                                            case 5:
                                                //Bottom
                                                normals[pos].x = 0f;
                                                normals[pos].y = 0f;
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
                            } else {
                                continue;
                            }

                            if (index < maxSize + maxSize / 2) {
                                indices[index] = prevPos;
                                indices[index + 1] = prevPos + 1;
                                indices[index + 2] = prevPos + 2;
                                indices[index + 3] = prevPos + 2;
                                indices[index + 4] = prevPos + 3;
                                indices[index + 5] = prevPos;

                                index += 6;
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
            rawChunk.materials[t] = material;
            rawChunk.colliderFaces[t] = vertice;
        }
        return rawChunk;
    }

    /*public RawChunk  (Chunk chunk, RawChunk rawChunk) {
        MeshedValues values = Mesher.Mesh (chunk);
        for (int t = 0; t < chunk.materials - 1; t++) {
            int maxSize = values.indices[t];

            SpatialMaterial material = reg.SelectByID (t + 1).material;
            Vector3[] vertice = new Vector3[maxSize];
            int[] indices = new int[maxSize + (maxSize / 2)];
            Vector3[] normals = new Vector3[maxSize];
            Vector2[] uvs = new Vector2[maxSize];
            float textureWidth = 2048f / material.AlbedoTexture.GetWidth ();
            float textureHeight = 2048f / material.AlbedoTexture.GetHeight ();

            if (maxSize > 0) {

                int pos = 0;
                int index = 0;

                for (int side = 0; side < 6; side++) {
                    int[, , , ] primitives = values.vertices[t][side];

                    for (int x = 0; x < Constants.CHUNK_SIZE1D; x++) {
                        for (int z = 0; z < Constants.CHUNK_SIZE1D; z++) {
                            int prevPos = pos;

                                for (int s = 0; s < 4; s++) {
                                        vertice[pos].x = primitives[x, z, s, 0] * Constants.VOXEL_SIZE;
                                        vertice[pos].y = primitives[x, z, s, 1] * Constants.VOXEL_SIZE;
                                        vertice[pos].z = primitives[x, z, s, 2] * Constants.VOXEL_SIZE;

                                        switch (side) {
                                            case 0:
                                                //Front
                                                normals[pos].x = 0f;
                                                normals[pos].y = 0f;
                                                normals[pos].z = -1f;
                                                uvs[pos].x = vertice[pos].x * textureWidth;
                                                uvs[pos].y = vertice[pos].y * textureHeight;
                                                break;
                                            case 1:
                                                //Back
                                                normals[pos].x = 0f;
                                                normals[pos].y = 0f;
                                                normals[pos].z = 1f;
                                                uvs[pos].x = vertice[pos].x * textureWidth;
                                                uvs[pos].y = vertice[pos].y * textureHeight;
                                                break;
                                            case 2:
                                                //Right
                                                normals[pos].x = -1f;
                                                normals[pos].y = 0f;
                                                normals[pos].z = 0f;
                                                uvs[pos].x = vertice[pos].z * textureWidth;
                                                uvs[pos].y = vertice[pos].y * textureHeight;
                                                break;
                                            case 3:
                                                //Left
                                                normals[pos].x = 1f;
                                                normals[pos].y = 0f;
                                                normals[pos].z = 0f;
                                                uvs[pos].x = vertice[pos].z * textureWidth;
                                                uvs[pos].y = vertice[pos].y * textureHeight;
                                                break;
                                            case 4:
                                                //Top
                                                normals[pos].x = 0f;
                                                normals[pos].y = 1f;
                                                normals[pos].z = 0f;
                                                uvs[pos].x = vertice[pos].x * textureWidth;
                                                uvs[pos].y = vertice[pos].z * textureHeight;
                                                break;
                                            case 5:
                                                //Bottom
                                                normals[pos].x = 0f;
                                                normals[pos].y = 0f;
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
                            } else {
                                continue;
                            }

                            if (index < maxSize + maxSize / 2) {
                                indices[index] = prevPos;
                                indices[index + 1] = prevPos + 1;
                                indices[index + 2] = prevPos + 2;
                                indices[index + 3] = prevPos + 2;
                                indices[index + 4] = prevPos + 3;
                                indices[index + 5] = prevPos;

                                index += 6;
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
            rawChunk.materials[t] = material;
            rawChunk.colliderFaces[t] = vertice;
        }

        return rawChunk;
    }*/

    public RawChunk FastGodotCube (Chunk chunk, RawChunk rawChunk) {
        Godot.Collections.Array godotArray = new Godot.Collections.Array ();
        godotArray.Resize (9);
        rawChunk.arrays = new Godot.Collections.Array[1];
        rawChunk.materials = new SpatialMaterial[1];
        rawChunk.colliderFaces = new Vector3[1][];

        int objectID = chunk.voxels[0].value;

        SpatialMaterial material = reg.SelectByID (objectID).material;

        float textureWidth = 2048f / material.AlbedoTexture.GetWidth ();
        float textureHeight = 2048f / material.AlbedoTexture.GetHeight ();

        Vector3[] vertice = new Vector3[24];
        Vector3[] normals = new Vector3[24];
        Vector2[] uvs = new Vector2[24];

        //FRONT
        vertice[0] = new Vector3 (0, 0, 0);
        vertice[1] = new Vector3 (Constants.CHUNK_LENGHT, 0, 0);
        vertice[2] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);
        vertice[3] = new Vector3 (0, Constants.CHUNK_LENGHT, 0);

        for (int i = 0; i < 4; i++) {
            normals[i] = new Vector3 (0, 0, -1);
            uvs[i].x = vertice[i].x * textureWidth;
            uvs[i].y = vertice[i].y * textureHeight;
        }

        //BACK
        vertice[4] = new Vector3 (Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);
        vertice[5] = new Vector3 (0, 0, Constants.CHUNK_LENGHT);
        vertice[6] = new Vector3 (0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
        vertice[7] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);

        for (int i = 4; i < 8; i++) {
            normals[i] = new Vector3 (0, 0, 1);
            uvs[i].x = vertice[i].x * textureWidth;
            uvs[i].y = vertice[i].y * textureHeight;
        }

        //LEFT
        vertice[8] = new Vector3 (0, 0, Constants.CHUNK_LENGHT);
        vertice[9] = new Vector3 (0, 0, 0);
        vertice[10] = new Vector3 (0, Constants.CHUNK_LENGHT, 0);
        vertice[11] = new Vector3 (0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);

        for (int i = 8; i < 12; i++) {
            normals[i] = new Vector3 (1, 0, 0);
            uvs[i].x = vertice[i].z * textureWidth;
            uvs[i].y = vertice[i].y * textureHeight;
        }

        //RIGHT
        vertice[12] = new Vector3 (Constants.CHUNK_LENGHT, 0, 0);
        vertice[13] = new Vector3 (Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);
        vertice[14] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
        vertice[15] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);

        for (int i = 12; i < 16; i++) {
            normals[i] = new Vector3 (-1, 0, 0);
            uvs[i].x = vertice[i].z * textureWidth;
            uvs[i].y = vertice[i].y * textureHeight;
        }

        // TOP
        vertice[16] = new Vector3 (0, Constants.CHUNK_LENGHT, 0);
        vertice[17] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, 0);
        vertice[18] = new Vector3 (Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);
        vertice[19] = new Vector3 (0, Constants.CHUNK_LENGHT, Constants.CHUNK_LENGHT);

        for (int i = 16; i < 20; i++) {
            normals[i] = new Vector3 (0, 1, 0);
            uvs[i].x = vertice[i].x * textureWidth;
            uvs[i].y = vertice[i].z * textureHeight;
        }

        //BOTTOM
        vertice[20] = new Vector3 (0, 0, Constants.CHUNK_LENGHT);
        vertice[21] = new Vector3 (Constants.CHUNK_LENGHT, 0, Constants.CHUNK_LENGHT);
        vertice[22] = new Vector3 (Constants.CHUNK_LENGHT, 0, 0);
        vertice[23] = new Vector3 (0, 0, 0);

        for (int i = 20; i < 24; i++) {
            normals[i] = new Vector3 (0, -1, 0);
            uvs[i].x = vertice[i].x * textureWidth;
            uvs[i].y = vertice[i].z * textureHeight;
        }

        godotArray[0] = vertice;
        godotArray[1] = normals;
        godotArray[4] = uvs;
        int index = 0;
        int[] indice = new int[36];

        for (int i = 0; i < 36; i += 6) {
            indice[i] = index;
            indice[i + 1] = index + 1;
            indice[i + 2] = index + 2;
            indice[i + 3] = index + 2;
            indice[i + 4] = index + 3;
            indice[i + 5] = index;

            index += 4;
        }

        godotArray[8] = indice;

        rawChunk.arrays[0] = godotArray;
        rawChunk.materials[0] = material;
        rawChunk.colliderFaces[0] = vertice;

        return rawChunk;
    }
}