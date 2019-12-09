using System.Buffers;
using System.Diagnostics;
using System;
using Godot;
using GodotArray = Godot.Collections.Array;
using System.Collections.Generic;

public class GreedyMesher
{
    private bool profile;
    private List<long> addingMeasures;
    private List<long> meshingMeasures;
    private volatile Registry registry;
    private ArrayPool<Vector3> memory;
    private int lol = 0;

    public GreedyMesher(Registry registry, bool profile)
    {
        this.profile = profile;
        this.registry = registry;

        memory = ArrayPool<Vector3>.Create();

        addingMeasures = new List<long>();
        meshingMeasures = new List<long>();
    }

    public MeshInstance cull(Chunk chunk)
    {
         MeshInstance meshInstance = new MeshInstance();
        if(chunk.materials > 1){
        Stopwatch watch = new Stopwatch();
        watch.Start();
        Vector3[][] vertices = new Vector3[chunk.materials][];
        long a = 16777215 << 8;
        byte b = 255;
        int count = 0;
        int[] indice = new int[chunk.materials];
        int[] arraySize = new int[chunk.materials];

        for (int i = 0; i < Constants.CHUNK_SIZE3D/chunk.materials; i++)
        {
            if(count >=Constants.CHUNK_SIZE3D - 1){
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
                Vector3[] buffer =  memory.Rent((Constants.CHUNK_SIZE3D/chunk.materials) * 18);
                vertices[objectID - 1] = buffer;                                
                indice[objectID - 1]  = 0;
            }

            int z = count / Constants.CHUNK_SIZE2D;
            int y = count % Constants.CHUNK_SIZE1D;
            int x = (count / Constants.CHUNK_SIZE1D)%Constants.CHUNK_SIZE1D;
            
            float sx = x * Constants.VOXEL_SIZE;
            float sy = y * Constants.VOXEL_SIZE;
            float sz = z * Constants.VOXEL_SIZE;

            float ax = (x + 1) * Constants.VOXEL_SIZE;
            float ay = (lenght + y)* Constants.VOXEL_SIZE;
            float az = (z + 1)* Constants.VOXEL_SIZE;

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

            if(index > (36 * Constants.CHUNK_SIZE1D)){     
                      int pos = index - (36 * Constants.CHUNK_SIZE1D) + 2;
                     if(vectors[pos].x < 0){
                         pos = (int)-vectors[pos].x;
                     }

                    if(vectors[index + 2].y <= vectors[pos].y){
                     for(int s = 0; s < 6; s ++){
                        vectors[index + s].x = -pos;
                    }
                        arraySize[objectID - 1] -= 6;
                    }

                    if (vectors[pos].y  < vectors[index + 2].y && vectors[pos + 2].y >= vectors[index].y){
                        vectors[index].y = vectors[pos].y ; 
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

            if (index > (36 * Constants.CHUNK_SIZE1D))
            {

                int pos = index - (36 * Constants.CHUNK_SIZE1D);
                if (vectors[pos + 2].y > ay && vectors[pos].y <= sy)
                {
                    vectors[pos].y = ay;
                    vectors[pos + 1].y = ay;
                    vectors[pos + 5].y = ay;
                }
                else if (vectors[pos + 2].y <= ay)
                {
                    for(int s = 0; s < 6; s ++){
                        vectors[index - (36 * Constants.CHUNK_SIZE1D + s)].x = -147457;
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
                else if (vectors[index - 34].y <= ay)
                {
                    for(int s = 0; s < 6; s ++){
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

            if(x > 0 && index > 36){     
                      int pos = index - 34;
                     if(vectors[pos].x < 0){
                        pos = (int)-vectors[pos].x;
                     }

                    if(vectors[index + 2].y <= vectors[pos].y){
                     for(int s = 0; s < 6; s ++){
                        vectors[index + s].x = -pos;
                    }
                        arraySize[objectID - 1] -= 6;
                    }

                    if (vectors[pos].y  < vectors[index + 2].y && vectors[pos + 2].y >= vectors[index].y){
                        vectors[index].y = vectors[pos].y; 
                        vectors[index + 1].y = vectors[pos].y;
                        vectors[index + 5].y = vectors[pos].y;
                    }
            }

            index += 6;

            //Top
             float tx = sx;
             float tz = sz;
                if (x > 0 && index > 36 && vectors[index - 36].y == ay  && vectors[index - 35].x == sx && vectors[index - 34].x == sx)
                {
                    tx = vectors[index - 36].x;
                    for(int s = 0; s < 6; s ++){
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
                if (x > 0 && index > 36 && vectors[index - 35].y == sy  && vectors[index - 36].x == sx  && vectors[index - 32].x == sx)
                {
                    tx = vectors[index - 35].x;
                    for(int s = 0; s < 6; s ++){
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
        
        watch.Stop();
        meshingMeasures.Add(watch.ElapsedMilliseconds);
        watch.Reset();
        watch.Start();

        ArrayMesh mesh = new ArrayMesh();
        StaticBody body = new StaticBody();
        GodotArray godotArray = new GodotArray();
        godotArray.Resize(9);


        for (int t = 0; t < chunk.materials - 1; t++)
        {
            Texture texture = registry.SelectByID(t + 1).texture;

            int size = indice[t];
            Vector3[] vertice = new Vector3[size + arraySize[t]];
            Vector3[] normals = new Vector3[size + arraySize[t]];
            Vector2[] uvs = new Vector2[size + arraySize[t]];

            float textureWidth = 2048f / texture.GetWidth();
            float textureHeight = 2048f / texture.GetHeight();

            Vector3[] primitives = vertices[t];
            int pos = 0;
            for (int i = 0; i < size; i ++)    
            { 
                Vector3 vector = primitives[i];
                if(vector.x >= 0 && pos < size + arraySize[t]){
                int s = ((i%36))/6;
                vertice[pos] = vector;
                switch(s) {
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
                pos ++;
                }
            }
            godotArray[0] = vertice;
           godotArray[1] = normals;
            godotArray[4] = uvs;

            SpatialMaterial material = new SpatialMaterial();
            CollisionShape colShape = new CollisionShape();
            ConcavePolygonShape shape = new ConcavePolygonShape();
            texture.Flags = 2;
            material.AlbedoTexture = texture;

            shape.SetFaces(vertice);
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, godotArray);
            mesh.SurfaceSetMaterial(t, material);
            colShape.SetShape(shape);
            body.AddChild(colShape);
        }

        meshInstance.AddChild(body);
        
        watch.Stop();
        addingMeasures.Add(watch.ElapsedMilliseconds);
        meshInstance.Mesh = mesh;
        lol++;
        GD.Print(lol);
        return meshInstance;
        }
        return meshInstance;
    }

    public List<long> GetAddingMeasures(){
        return addingMeasures;
    }

    public List<long> GetMesherMeasures(){
        return meshingMeasures;
    }
}