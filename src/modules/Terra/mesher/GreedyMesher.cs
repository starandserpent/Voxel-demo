using System.Linq;
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

    public GreedyMesher(Registry registry, bool profile)
    {
        this.profile = profile;
        this.registry = registry;
        addingMeasures = new List<long>();
        meshingMeasures = new List<long>();
    }

    public Dictionary<Texture, GodotArray> cull(Chunk chunk)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();

        Dictionary<Texture, Memory<Box>> boxes = new Dictionary<Texture, Memory<Box>>();
        long a = 16777215 << 8;
        byte b = 255;
        int count = 0;

        for (int i = 0; i < chunk.voxels.Span.Length; i++)
        {
            uint bytes = chunk.voxels.Span[i];

            int lenght = (int) (bytes & a) >> 8;
            int objectID = (int) (bytes & b);

            if (objectID == 0)
            {
                count += lenght;
                continue;
            }

            Texture texture = registry.SelectByID(objectID).texture;

            if (!boxes.ContainsKey(texture))
            {
                boxes.Add(texture, new Memory<Box>(new Box[4096]));
            }

            int z = count / 4096;
            int y = count % 64;
            int x = (count - 4096 * z) / 64;

            int origin = x + (z * 64);

            //Front
            Box box = new Box();
            box.deleteIndex = new int[6];
            box.vertice = new TerraVector3[6][];

            box.deleteIndex[0] = -1;
            box.vertice[0] = new TerraVector3[6]
            {
                new TerraVector3(x, y, z) / 4, new TerraVector3(x + 1, y, z) / 4,
                new TerraVector3(x + 1, y + lenght, z) / 4, new TerraVector3(x + 1, y + lenght, z) / 4,
                new TerraVector3(x, y + lenght, z) / 4, new TerraVector3(x, y, z) / 4
            };
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

            boxes[texture].Span[origin] = box;

            count += lenght;
        }

        watch.Stop();
        meshingMeasures.Add(watch.ElapsedMilliseconds);
        watch.Reset();
        watch.Start();

        Dictionary<Texture, GodotArray> arrays = new Dictionary<Texture, GodotArray>();
        Texture[] textures = boxes.Keys.ToArray();

        for (int t = 0; t < textures.Count(); t++)
        {
            GodotArray godotArray = new GodotArray();
            godotArray.Resize(9);

            Texture texture1 = textures[t];
            Memory<Box> offheap = boxes[texture1];
            int size = offheap.Length;

            List<Vector3> vertice = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i < size; i++)
            {
                Box box = boxes[texture1].Span[i];

                for (int s = 0; s < 6; s++)
                {
                    if (box.deleteIndex[s] == -1)
                    {
                        uvs.AddRange(SetTextureCoords(texture1, box, s));
                        for (int f = 0; f < 6; f++)
                        {
                            vertice.Add(box.vertice[s][f].ToVector3());
                            switch (s)
                            {
                                case 0:
                                    normals.Add(new Vector3(0, 0, 1));
                                    break;
                                case 1:
                                    normals.Add(new Vector3(0, 0, -1));
                                    break;
                                case 2:
                                    normals.Add(new Vector3(1, 0, 0));
                                    break;
                                case 3:
                                    normals.Add(new Vector3(-1, 0, 0));
                                    break;
                                case 4:
                                    normals.Add(new Vector3(0, 1, 0));
                                    break;
                                case 5:
                                    normals.Add(new Vector3(0, -1, 0));
                                    break;
                            }
                        }
                    }
                }
            }

            godotArray[0] = vertice.ToArray();
            godotArray[1] = normals.ToArray();
            godotArray[4] = uvs.ToArray();

            arrays.Add(texture1, godotArray);
        }

        boxes.Clear();

        watch.Stop();

        addingMeasures.Add(watch.ElapsedMilliseconds);
        return arrays;
    }

    private static Vector2[] SetTextureCoords(Texture texture, Box face, int side)
    {
        Vector2[] uvs = new Vector2[6];
        float textureWidth = 2048f / texture.GetWidth();
        float textureHeight = 2048f / texture.GetHeight();

        switch (side)
        {
            case 2:
                for (int t = 0; t < 6; t++)
                {
                    uvs[t] = new Vector2(face.vertice[2][t].z * textureWidth,
                        face.vertice[2][t].y * textureHeight);
                }

                break;
            case 3:
                for (int t = 0; t < 6; t++)
                {
                    uvs[t] = new Vector2(face.vertice[3][t].z * textureWidth,
                        face.vertice[3][t].y * textureHeight);
                }

                break;
            case 0:
                for (int t = 0; t < 6; t++)
                {
                    uvs[t] = new Vector2(face.vertice[0][t].z * textureWidth,
                        face.vertice[0][t].y * textureHeight);
                }

                break;
            case 1:
                for (int t = 0; t < 6; t++)
                {
                    uvs[t] = new Vector2(face.vertice[1][t].x * textureWidth,
                        face.vertice[1][t].z * textureHeight);
                }

                break;
            case 4:
                for (int t = 0; t < 6; t++)
                {
                    uvs[t] = new Vector2(face.vertice[4][t].z * textureWidth,
                        face.vertice[4][t].y * textureHeight);
                }

                break;
            case 5:
                for (int t = 0; t < 6; t++)
                {
                    uvs[t] = new Vector2(face.vertice[5][t].x * textureWidth,
                        face.vertice[5][t].y * textureHeight);
                }

                break;
        }

        return uvs;
    }

    public List<long> GetAddingMeasures(){
        return addingMeasures;
    }

    public List<long> GetMesherMeasures(){
        return meshingMeasures;
    }
}