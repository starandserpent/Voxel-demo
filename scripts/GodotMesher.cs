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

            Stack<Position[]> vertices = new Stack<Position[]> (maxSize);
            SpatialMaterial material = reg.SelectByID (t + 1).material;
            Stack<Vector3> vertice = new Stack<Vector3> (maxSize);
            Stack<int> indices = new Stack<int> (maxSize + (maxSize / 2));
            Stack<Vector3> normals = new Stack<Vector3> (maxSize);
            Stack<Vector2> uvs = new Stack<Vector2> (maxSize);
            float textureWidth = 2048f / material.AlbedoTexture.GetWidth ();
            float textureHeight = 2048f / material.AlbedoTexture.GetHeight ();

            if (maxSize > 0) {
                for (int side = 0; side < 6; side++) {
                    Stack<Position[]> stack = Mesher.GreedyMeshing (side, values.vertices[t][side], vertices);
                    int size = stack.Count;
                    for (int i = 0; i < size; i++) {
                        Position[] position = stack.Pop ();

                        indices.Push (vertice.Count);
                        indices.Push (vertice.Count + 1);
                        indices.Push (vertice.Count + 2);
                        indices.Push (vertice.Count + 2);
                        indices.Push (vertice.Count + 3);
                        indices.Push (vertice.Count);

                        for (int s = 0; s < 4; s++) {
                            Vector3 vector = new Vector3 ();
                            vector.x = position[s].x * Constants.VOXEL_SIZE;
                            vector.y = position[s].y * Constants.VOXEL_SIZE;
                            vector.z = position[s].z * Constants.VOXEL_SIZE;

                            vertice.Push (vector);
                            Vector2 uv = new Vector2 ();
                            Vector3 normal = new Vector3 ();
                            switch (side) {
                                case 0:
                                    //Front
                                    normal.x = 0f;
                                    normal.y = 0f;
                                    normal.z = -1f;
                                    normals.Push (normal);

                                    uv.x = vector.x * textureWidth;
                                    uv.y = vector.y * textureHeight;
                                    uvs.Push (uv);
                                    break;
                                case 1:
                                    //Back
                                    normal.x = 0f;
                                    normal.y = 0f;
                                    normal.z = 1f;
                                    normals.Push (normal);

                                    uv.x = vector.x * textureWidth;
                                    uv.y = vector.y * textureHeight;
                                    uvs.Push (uv);

                                    break;
                                case 2:
                                    //Right
                                    normal.x = -1f;
                                    normal.y = 0f;
                                    normal.z = 0f;
                                    normals.Push (normal);

                                    uv.x = vector.z * textureWidth;
                                    uv.y = vector.y * textureHeight;
                                    uvs.Push (uv);
                                    break;
                                case 3:
                                    //Left
                                    normal.x = 1f;
                                    normal.y = 0f;
                                    normal.z = 0f;
                                    normals.Push (normal);

                                    uv.x = vector.z * textureWidth;
                                    uv.y = vector.y * textureHeight;
                                    uvs.Push (uv);
                                    break;
                                case 4:
                                    //Top
                                    normal.x = 0f;
                                    normal.y = 1f;
                                    normal.z = 0f;
                                    normals.Push (normal);

                                    uv.x = vector.x * textureWidth;
                                    uv.y = vector.z * textureHeight;
                                    uvs.Push (uv);
                                    break;
                                case 5:
                                    //Bottom
                                    normal.x = 0f;
                                    normal.y = 0f;
                                    normal.z = 0f;
                                    normals.Push (normal);

                                    uv.x = vector.x * textureWidth;
                                    uv.y = vector.z * textureHeight;
                                    uvs.Push (uv);
                                    break;
                            }
                        }
                    }
                }
            }

            Godot.Collections.Array godotArray = new Godot.Collections.Array ();
            godotArray.Resize (9);

            godotArray[0] = vertice.ToArray ();
            godotArray[1] = normals.ToArray ();
            godotArray[4] = uvs.ToArray ();
            godotArray[8] = indices.ToArray ();

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