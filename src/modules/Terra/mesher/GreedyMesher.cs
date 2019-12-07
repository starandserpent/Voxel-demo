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

            int origin = x + (z * 64);
            
            float sx = x* 0.25f;
            float sy = y* 0.25f;
            float sz = z* 0.25f;

            float ax = (x + 1) * 0.25f;
            float ay = (lenght + y)* 0.25f;
            float az = (z + 1)* 0.25f;

            //Front
            Vector3[] vectors = vertices[objectID - 1];
            /*            if (z > 0)
            {
                int pos = (x + (z - 1) * 64);
                if (prevBox.deleteIndex[0] > -1)
                {
                    prevBox = boxes[texture].Span[prevBox.deleteIndex[0]];
                }

                if (box.vertice[0][3].y <= prevBox.vertice[0][3].y)
                {
                    box.deleteIndex[0] = x + (z - 1) * 64;
                }

                if (prevBox.vertice[0][3].y < box.vertice[0][3].y && prevBox.vertice[0][0].y >= box.vertice[0][0].y)
                {
                    box.vertice[0][0] =
                        new TerraVector3(box.vertice[0][0].x, prevBox.vertice[0][3].y, box.vertice[0][0].z);
                    box.vertice[0][1] =
                        new TerraVector3(box.vertice[0][1].x, prevBox.vertice[0][3].y, box.vertice[0][1].z);
                    box.vertice[0][5] =
                        new TerraVector3(box.vertice[0][5].x, prevBox.vertice[0][3].y, box.vertice[0][5].z);
                }
            }*/
                        int index = indice[objectID - 1];
            //Back
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            indice[objectID - 1] = index;

             index = indice[objectID - 1];
            //Back
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;
            indice[objectID - 1] = index;


             index = indice[objectID - 1];
            //Back
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;
            indice[objectID - 1] = index;

             index = indice[objectID - 1];
            //Back
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;
            indice[objectID - 1] = index;

             index = indice[objectID - 1];
            //Back
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

                        indice[objectID - 1] = index;


             index = indice[objectID - 1];
            //Back
            vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            vectors[index].x = sx;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

             vectors[index].x = sx;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = ay;
            vectors[index].z = az;
            index++;

           vectors[index].x = ax;
            vectors[index].y = sy;
            vectors[index].z = az;
            index++;

            indice[objectID - 1] = index;
        /*
            //Right
            box.deleteIndex[2] = -1;
            box.vertice[2] = new TerraVector3[6]
            {
                new TerraVector3(x, y, z + 1) / 4, new TerraVector3(x, y, z) / 4,
                new TerraVector3(x, y + lenght, z) / 4, new TerraVector3(x, y + lenght, z) / 4,
                new TerraVector3(x, y + lenght, z + 1) / 4, new TerraVector3(x, y, z + 1) / 4
            };
            if (x > 0)
            {
                int pos = (x - 1) + z * 64;
                Box prevBox = boxes[texture].Span[pos];
                if (prevBox.deleteIndex[2] > -1)
                {
                    prevBox = boxes[texture].Span[prevBox.deleteIndex[2]];
                }

                if (box.vertice[2][3].y <= prevBox.vertice[2][3].y)
                {
                    box.deleteIndex[2] = pos;
                }

                if (prevBox.vertice[2][3].y < box.vertice[2][3].y && prevBox.vertice[2][0].y >= box.vertice[2][0].y)
                {
                    box.vertice[2][0] =
                        new TerraVector3(box.vertice[2][0].x, prevBox.vertice[2][3].y, box.vertice[2][0].z);
                    box.vertice[2][1] =
                        new TerraVector3(box.vertice[2][1].x, prevBox.vertice[2][3].y, box.vertice[2][1].z);
                    box.vertice[2][5] =
                        new TerraVector3(box.vertice[2][5].x, prevBox.vertice[2][3].y, box.vertice[2][5].z);
                }
            }


            //Left
            box.deleteIndex[3] = -1;
            box.vertice[3] = new TerraVector3[6]
            {
                new TerraVector3(x + 1, y, z) / 4, new TerraVector3(x + 1, y, z + 1) / 4,
                new TerraVector3(x + 1, y + lenght, z + 1) / 4, new TerraVector3(x + 1, y + lenght, z + 1) / 4,
                new TerraVector3(x + 1, y + lenght, z) / 4, new TerraVector3(x + 1, y, z) / 4
            };

            if (x > 0)
            {
                int pos = (x - 1) + z * 64;
                Box prevBox = boxes[texture].Span[pos];
                if (prevBox.vertice[3][3].y > box.vertice[3][3].y && prevBox.vertice[3][0].y <= box.vertice[3][0].y)
                {
                    prevBox.vertice[3][0] = new TerraVector3(prevBox.vertice[3][0].x, box.vertice[3][3].y,
                        prevBox.vertice[3][0].z);
                    prevBox.vertice[3][1] = new TerraVector3(prevBox.vertice[3][1].x, box.vertice[3][3].y,
                        prevBox.vertice[3][1].z);
                    prevBox.vertice[3][5] = new TerraVector3(prevBox.vertice[3][5].x, box.vertice[3][3].y,
                        prevBox.vertice[3][5].z);
                    boxes[texture].Span[pos] = prevBox;
                }
                else if (prevBox.vertice[3][3].y <= box.vertice[3][3].y)
                {
                    prevBox.deleteIndex[3] = origin;
                    boxes[texture].Span[pos] = prevBox;
                }
            }

            //Top
            box.deleteIndex[4] = -1;
            box.vertice[4] = new TerraVector3[6]
            {
                new TerraVector3(x, y + lenght, z) / 4, new TerraVector3(x + 1, y + lenght, z) / 4,
                new TerraVector3(x + 1, y + lenght, z + 1) / 4, new TerraVector3(x + 1, y + lenght, z + 1) / 4,
                new TerraVector3(x, y + lenght, z + 1) / 4, new TerraVector3(x, y + lenght, z) / 4
            };

            //Bottom        
            box.deleteIndex[5] = -1;
            box.vertice[5] = new TerraVector3[6]
            {
                new TerraVector3(x + 1, y, z) / 4, new TerraVector3(x, y, z) / 4, new TerraVector3(x, y, z + 1) / 4,
                new TerraVector3(x, y, z + 1) / 4, new TerraVector3(x + 1, y, z + 1) / 4,
                new TerraVector3(x + 1, y, z) / 4
            };^&*/
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

            Vector3[] vertice = new Vector3[indice[t]];
            Vector3[] normals = new Vector3[indice[t]];
            Vector2[] uvs = new Vector2[indice[t]];

            float textureWidth = 2048f / texture.GetWidth();
            float textureHeight = 2048f / texture.GetHeight();

                Vector3[] primitives = vertices[t];
            for (int i = 0; i < indice[t]; i ++)    
            { 
                int s = i%6;
                Vector3 vector = primitives[i];

                if(vector != null){
                vertice[i] = vector;

                switch(s){
                    case 0:
                    normals[i].x = 0f;
                normals[i].y = 0f;
                normals[i].z = 1f;
                    uvs[i].x = vector.x * textureWidth;
                    uvs[i].y = vector.y * textureHeight;
                    break;
                    case 1:
                    normals[i].x = 0f;
                normals[i].y = 0f;
                normals[i].z = -1f;
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