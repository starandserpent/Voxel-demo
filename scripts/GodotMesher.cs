using System.Collections.Generic;
using System.Linq;
using Godot;

public class GodotMesher : Spatial {
    private volatile Registry reg;
    public void Set (Registry reg) {
        this.reg = reg;
    }

    public void MeshChunk (Chunk chunk) {

        RawChunk rawChunk = new RawChunk ();

        rawChunk.arrays = new Godot.Collections.Array[chunk.Materials - 1];
        rawChunk.materials = new SpatialMaterial[chunk.Materials - 1];
        rawChunk.colliderFaces = new Vector3[chunk.Materials - 1][];

        if (chunk.Materials > 1) {
            rawChunk = Meshing (chunk, rawChunk);
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

    public RawChunk Meshing (Chunk chunk, RawChunk rawChunk) {
        var values = Mesher.NaiveGreedyMeshing (chunk);
        Stack<Position[]>[][] stacks = new Stack<Position[]>[6][];
        int[] size = new int[chunk.Materials - 1];

        for (int side = 0; side < 6; side++) {
            stacks[side] = new Stack<Position[]>[chunk.Materials - 1];
            for (int t = 0; t < chunk.Materials - 1; t++) {
                stacks[side][t] = new Stack<Position[]> ();
            }

            Mesher.GreedyMeshing (values, side, stacks[side]);
            for (int t = 0; t < chunk.Materials - 1; t++) {
                size[t] += stacks[side][t].Count;
            }
        }

        for (int t = 0; t < chunk.Materials - 1; t++) {
            size[t] *= 4;
            SpatialMaterial material = reg.SelectByID (t + 1).material;
            Vector3[] vertice = new Vector3[size[t]];
            int[] indices = new int[size[t] + (size[t] / 2)];
            Vector3[] normals = new Vector3[size[t]];
            Vector2[] uvs = new Vector2[size[t]];
            float textureWidth = 2048f / material.AlbedoTexture.GetWidth ();
            float textureHeight = 2048f / material.AlbedoTexture.GetHeight ();

            if (size[t] > 0) {
                int index = 0;
                int i = 0;
                for (int side = 0; side < 6; side++) {
                    for (; i < size[t]; i += 4) {
                        if (stacks[side][t].Count > 0) {
                            Position[] position = stacks[side][t].Pop ();

                            indices[index] = i;
                            indices[index + 1] = i + 1;
                            indices[index + 2] = i + 2;
                            indices[index + 3] = i + 2;
                            indices[index + 4] = i + 3;
                            indices[index + 5] = i;
                            index += 6;

                            for (int s = 0; s < 4; s++) {
                                Vector3 vector = new Vector3 ();
                                vector.x = position[s].x * Constants.VOXEL_SIZE;
                                vector.y = position[s].y * Constants.VOXEL_SIZE;
                                vector.z = position[s].z * Constants.VOXEL_SIZE;
                                vertice[i + s] = vector;

                                Vector2 uv = new Vector2 ();
                                Vector3 normal = new Vector3 ();
                                switch (side) {
                                    case 0:
                                        //Front
                                        normal.x = 0f;
                                        normal.y = 0f;
                                        normal.z = -1f;
                                        normals[i + s] = normal;

                                        uv.x = vector.x * textureWidth;
                                        uv.y = vector.y * textureHeight;
                                        uvs[i + s] = uv;
                                        break;
                                    case 1:
                                        //Back
                                        normal.x = 0f;
                                        normal.y = 0f;
                                        normal.z = 1f;
                                        normals[i + s] = normal;

                                        uv.x = vector.x * textureWidth;
                                        uv.y = vector.y * textureHeight;
                                        uvs[i + s] = uv;

                                        break;
                                    case 2:
                                        //Right
                                        normal.x = -1f;
                                        normal.y = 0f;
                                        normal.z = 0f;
                                        normals[i + s] = normal;

                                        uv.x = vector.z * textureWidth;
                                        uv.y = vector.y * textureHeight;
                                        uvs[i + s] = uv;
                                        break;
                                    case 3:
                                        //Left
                                        normal.x = 1f;
                                        normal.y = 0f;
                                        normal.z = 0f;
                                        normals[i + s] = normal;

                                        uv.x = vector.z * textureWidth;
                                        uv.y = vector.y * textureHeight;
                                        uvs[i + s] = uv;
                                        break;
                                    case 4:
                                        //Top
                                        normal.x = 0f;
                                        normal.y = 1f;
                                        normal.z = 0f;
                                        normals[i + s] = normal;

                                        uv.x = vector.x * textureWidth;
                                        uv.y = vector.z * textureHeight;
                                        uvs[i + s] = uv;
                                        break;
                                    case 5:
                                        //Bottom
                                        normal.x = 0f;
                                        normal.y = 0f;
                                        normal.z = 0f;
                                        normals[i + s] = normal;

                                        uv.x = vector.x * textureWidth;
                                        uv.y = vector.z * textureHeight;
                                        uvs[i + s] = uv;
                                        break;
                                }
                            }
                        } else {
                            break;
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
        }
        return rawChunk;
    }

    public RawChunk FastGodotCube (Chunk chunk, RawChunk rawChunk) {
        Godot.Collections.Array godotArray = new Godot.Collections.Array ();
        godotArray.Resize (9);
        rawChunk.arrays = new Godot.Collections.Array[1];
        rawChunk.materials = new SpatialMaterial[1];
        rawChunk.colliderFaces = new Vector3[1][];

        int objectID = chunk.Voxels[0].value;

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