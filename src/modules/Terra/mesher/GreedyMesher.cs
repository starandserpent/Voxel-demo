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

    public Dictionary<Texture, GodotArray> cull(Chunk chunk)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        Vector3[][] vertices = new Vector3[2][];
        long a = 16777215 << 8;
        byte b = 255;
        int count = 0;
        int[] indice = new int[2];

        for (int i = 0; i < chunk.voxels.Length; i++)
        {
            if(count >=4096*64){
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
                Vector3[] buffer =  memory.Rent(chunk.voxels.Length * 6);
                vertices[objectID - 1] = buffer;
            }

            int z = count / 4096;
            int y = count % 64;
            int x = (count - 4096 * z) / 64;
            
            float sx = x* 0.25f;
            float sy = y* 0.25f;
            float sz = z* 0.25f;

            float ax = (x + 1) * 0.25f;
            float ay = (lenght + y)* 0.25f;
            float az = (z + 1)* 0.25f;

            //Front
            Vector3[] vectors = vertices[objectID - 1];

            int index = indice[objectID - 1];
            //Front
            //1
            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = sz;
            index++;

            //2
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = sz;
            index++;

            //3
             vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = sz;
            index++;

            //4
             vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = sz;
            index++;

            //5
           vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = sz;
            index++;

            //6
           vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = sz;
            index++;

            //Back
            //1
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            //2
            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            //3
             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

            //4
             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

            //5
           vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

            //6
           vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            //Left
            //1
            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            //2
            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = sz;
            index++;

            //3
             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = sz;
            index++;

            //4
            vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = sz;
            index++;

            //5
           vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

            //6
            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            //Right
           //1
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = sz;
            index++;

            //2
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            //3
             vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

            //4
            vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

            //5
           vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = sz;
            index++;

            //6
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = sz;
            index++;

            //Top
            //1
            vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = sz;
            index++;

            //2
            vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = sz;
            index++;

            //3
             vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;
                
            //4 
             vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

            //5
           vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

            //6
            vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = sz;
            index++;

            //Bottom
            //1
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = sz;
            index++;

            //2
            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = sz;
            index++;

            //3
             vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            //4
            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            //5
           vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

        //6
           vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = sz;
            index++;

            indice[objectID - 1] = index;
            count += lenght;
        }
        
        watch.Stop();
        meshingMeasures.Add(watch.ElapsedMilliseconds);
        watch.Reset();
        watch.Start();

        Dictionary<Texture, GodotArray> arrays = new Dictionary<Texture, GodotArray>();
        for (int t = 0; t < 2; t++)
        {
            GodotArray godotArray = new GodotArray();
            godotArray.Resize(9);

            Texture texture = registry.SelectByID(t + 1).texture;

            int size = indice[t];
            Vector3[] vertice = new Vector3[size];
            Vector3[] normals = new Vector3[size];
            Vector2[] uvs = new Vector2[size];

            float textureWidth = 2048f / texture.GetWidth();
            float textureHeight = 2048f / texture.GetHeight();

            Vector3[] primitives = vertices[t];
            for (int i = 0; i < size; i ++)    
            { 
                Vector3 vector = primitives[i];
                if(vector != null){
                int s = ((i%36))/6;
                vertice[i] = vector;
                switch(s) {
                    case 0:
                    normals[i].x = 0f;
                normals[i].y = 0f;
                normals[i].z = -1f;
                    uvs[i].x = vector.x * textureWidth;
                    uvs[i].y = vector.y * textureHeight;
                    break;
                    case 1:
                    normals[i].x = 0f;
                normals[i].y = 0f;
                normals[i].z = 1f;
                    uvs[i].x = vector.x * textureWidth;
                    uvs[i].y = vector.y * textureHeight;
                    break;
                     case 2:
                     normals[i].x = -1f;
                normals[i].y = 0f;
                normals[i].z = 0f;
                    uvs[i].x = vector.z * textureWidth;
                    uvs[i].y = vector.y * textureHeight;
                    break;
                     case 3:
                     normals[i].x = 1f;
                normals[i].y = 0f;
                normals[i].z = 0f;
                    uvs[i].x = vector.z * textureWidth;
                    uvs[i].y = vector.y * textureHeight;
                    break;
                     case 4:
                     normals[i].x = 0f;
                normals[i].y = 1f;
                normals[i].z = 0f;
                    uvs[i].x = vector.x * textureWidth;
                    uvs[i].y = vector.z * textureHeight;
                    break;
                     case 5:
                     normals[i].x = 0f;
                normals[i].y = -1f;
                normals[i].z = 0f;
                    uvs[i].x = vector.x * textureWidth;
                    uvs[i].y = vector.z * textureHeight;
                    break;
                }
                }
            }
            godotArray[0] = vertice;
            godotArray[1] = normals;
            godotArray[4] = uvs;

            arrays.Add(texture, godotArray);
        }

        watch.Stop();
        lol++;
        GD.Print(lol);
        addingMeasures.Add(watch.ElapsedMilliseconds);
        return arrays;
    }

    public List<long> GetAddingMeasures(){
        return addingMeasures;
    }

    public List<long> GetMesherMeasures(){
        return meshingMeasures;
    }
}