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
    private ArrayPool<float> memory;
    private int lol = 0;

    public GreedyMesher(Registry registry, bool profile)
    {
        this.profile = profile;
        this.registry = registry;

        memory = ArrayPool<float>.Create();

        addingMeasures = new List<long>();
        meshingMeasures = new List<long>();
    }

    public Dictionary<Texture, GodotArray> cull(Chunk chunk)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        float[][][] vertices = new float[2][][];
        long a = 16777215 << 8;
        byte b = 255;
        int count = 0;
        int allCount = 0;
        int[][] indicesPT = new int[2][];

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
                vertices[objectID - 1] = new float[6][];
                indicesPT[objectID - 1] = new int[6];
                for(int s = 0; s < 6; s++){
                    float[] buffer =  memory.Rent(chunk.voxels.Length * 6);
                    vertices[objectID - 1][s] = buffer;
                }                
            }

            int z = count / 4096;
            int y = count % 64;
            int x = (count - 4096 * z) / 64;

            int origin = x + (z * 64);

            int[] indices = indicesPT[objectID - 1];
            
            float sx = x* 0.25f;
            float sy = y* 0.25f;
            float sz = z* 0.25f;

            float ax = (x + 1) * 0.25f;
            float ay = (lenght + y)* 0.25f;
            float az = (z + 1)* 0.25f;

            //Front
            float[] points = vertices[objectID - 1][0];
            int index = indices[0];
            for(int p = 2; p < 18; p += 3){
                points[index + p] = sz;
            }

            for(int p = 3; p < 12; p += 3){
                points[index + p] = ax;
            }

            for(int p = 7; p < 16; p += 3){
                points[index + p] = ay;
            }

            //1
            points[index] = sx;
            points[index + 1] = sy;

            //2
            points[index + 4] = sy;

            //5
            points[index + 12] = sx;

            //6
            points[index + 15] = sx;
            points[index + 16] = sy;

            indices[0] += 18;       

             //Back
             index = indices[1];
            points = vertices[objectID - 1][1];

            for(int p = 2; p < 18; p += 3){
                points[index + p] = az;
            }

            for(int p = 3; p < 12; p += 3){
                points[index + p] = sx;
            }

            for(int p = 7; p < 14; p += 3){
                points[index + p] = ay;
            }

            //1
            points[index] = ax;
            points[index + 1] = sy;

            //2
            points[index + 4] = sy;
            
            //5
            points[index + 12] = ax;

            //6
            points[index + 15] = ax;
            points[index + 16] = sy;

            indices[1] += 18;    

             //Right
             index = indices[2];
             points = vertices[objectID - 1][2];

            for(int p = 0; p < 18; p += 3){
                points[index + p] = ax;
            }

            for(int p = 5; p < 12; p += 3){
                points[index + p] = az;
            }

            for(int p = 7; p < 14; p += 3){
                points[index + p] = ay;
            }

            //1
            points[index + 1] = sy;
            points[index + 2] = sz;

            //2
            points[index + 4] = sy;

            //5
            points[index + 14] = sz;

            //6
            points[index + 16] = sy;
            points[index + 17] = sz;

            indices[2] += 18;   

            //Left 
            index = indices[3];
            points = vertices[objectID - 1][3];

            for(int p = 0; p < 18; p += 3){
                points[index + p] = sx;
            }

            for(int p = 5; p < 12; p += 3){
                points[index + p] = sz;
            }

            for(int p = 7; p < 14; p += 3){
                points[index + p] = ay;
            }
           
            //1
            points[index + 1] = sy;
            points[index + 2] = az;
            
            //2
            points[index + 4] = sy;

            //5
            points[index + 14] = az;

            //6
            points[index + 16] = sy;
            points[index + 17] = az;

            indices[3] += 18;    

             //Top
             index = indices[4];
             points = vertices[objectID - 1][4];
            for(int p = 1; p < 18; p += 3){
                points[index + p] = ay;
            }

            for(int p = 3; p < 12; p += 3){
                points[index + p] = ax;
            }

            for(int p = 8; p < 15; p += 3){
                points[index + p] = az;
            }

            //1
            points[index] = sx;
            points[index + 2] = sz;

            //2
            points[index + 5] = sz;
            
            //5
            points[index + 12] = sx;

            //6
            points[index + 15] = sx;
            points[index + 17] = sz;

            indices[4] += 18;    

             //Bottom
             index = indices[5];
             points = vertices[objectID - 1][5];
            for(int p = 1; p < 18; p += 3){
                points[index + p] = sy;
            }

            for(int p = 3; p < 12; p += 3){
                points[index + p] = sx;
            }

            for(int p = 8; p < 15; p += 3){
                points[index + p] = az;
            }

            //1
            points[index] = ax;
            points[index + 2] = sz;

            //2
            points[index + 5] = sz;
            
            //5
            points[index + 12] = ax;

            //6
            points[index + 15] = ax;
            points[index + 17] = sz;

            indices[5] += 18;    
            /*
            if (z > 0)
            {
                int pos = (x + (z - 1) * 64);
                Box prevBox = boxes[texture].Span[pos];
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
            }

            //Back
            box.deleteIndex[1] = -1;
            box.vertice[1] = new TerraVector3[6]
            {
                new TerraVector3(x + 1, y, z + 1) / 4, new TerraVector3(x, y, z + 1) / 4,
                new TerraVector3(x, y + lenght, z + 1) / 4, new TerraVector3(x, y + lenght, z + 1) / 4,
                new TerraVector3(x + 1, y + lenght, z + 1) / 4, new TerraVector3(x + 1, y, z + 1) / 4
            };
            if (z > 0)
            {
                int pos = x + ((z - 1) * 64);
                Box prevBox = boxes[texture].Span[pos];
                if (prevBox.vertice[1][3].y > box.vertice[1][3].y && prevBox.vertice[1][0].y <= box.vertice[1][0].y)
                {
                    prevBox.vertice[1][0] = new TerraVector3(prevBox.vertice[1][0].x, box.vertice[1][3].y,
                        prevBox.vertice[1][0].z);
                    prevBox.vertice[1][1] = new TerraVector3(prevBox.vertice[1][1].x, box.vertice[1][3].y,
                        prevBox.vertice[1][1].z);
                    prevBox.vertice[1][5] = new TerraVector3(prevBox.vertice[1][5].x, box.vertice[1][3].y,
                        prevBox.vertice[1][5].z);
                    boxes[texture].Span[pos] = prevBox;
                }
                else if (prevBox.vertice[1][3].y <= box.vertice[1][3].y)
                {
                    prevBox.deleteIndex[1] = origin;
                    boxes[texture].Span[pos] = prevBox;
                }
            }

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
            };
            }*/
            allCount += 54;
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

            Vector3[] vertice = new Vector3[allCount];
            Vector3[] normals = new Vector3[allCount];
            Vector2[] uvs = new Vector2[allCount];

            float textureWidth = 2048f / texture.GetWidth();
            float textureHeight = 2048f / texture.GetHeight();

            int pos = 0;
            for(int s = 0; s < 6; s++){
                int index = indicesPT[t][s];
                float[] primitives = vertices[t][s];
            for (int i = 0; i < index; i += 3)    
            { 
                float x = primitives[i];
                float y = primitives[i + 1];
                float z = primitives[i + 2];
            
                vertice[pos].x = x;
                vertice[pos].y = y;
                vertice[pos].z = z;

                switch(s){
                    case 0:
                    normals[pos].x = 0f;
                normals[pos].y = 0f;
                normals[pos].z = 1f;
                    uvs[pos].x = x * textureWidth;
                    uvs[pos].y = y * textureHeight;
                    break;
                    case 1:
                    normals[pos].x = 0f;
                normals[pos].y = 0f;
                normals[pos].z = -1f;
                    uvs[pos].x = x * textureWidth;
                    uvs[pos].y = y * textureHeight;
                    break;
                     case 2:
                     normals[pos].x = -1f;
                normals[pos].y = 0f;
                normals[pos].z = 0f;
                    uvs[pos].x = z * textureWidth;
                    uvs[pos].y = y * textureHeight;
                    break;
                     case 3:
                     normals[pos].x = 1f;
                normals[pos].y = 0f;
                normals[pos].z = 0f;
                    uvs[pos].x = z * textureWidth;
                    uvs[pos].y = y * textureHeight;
                    break;
                     case 4:
                     normals[pos].x = 0f;
                normals[pos].y = 1f;
                normals[pos].z = 0f;
                    uvs[pos].x = x * textureWidth;
                    uvs[pos].y = z * textureHeight;
                    break;
                     case 5:
                     normals[pos].x = 0f;
                normals[pos].y = -1f;
                normals[pos].z = 0f;
                    uvs[pos].x = x * textureWidth;
                    uvs[pos].y = z * textureHeight;
                    break;
                }
                pos ++;
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