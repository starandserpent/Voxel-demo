using System.Linq;
using System.Diagnostics;
using System;
using Godot;
using GodotArray = Godot.Collections.Array;
using System.Collections.Generic;
public class GreedyMesher
{
    private volatile Registry registry;
    public GreedyMesher(Registry registry) {
        this.registry = registry;
    }

    public Dictionary<Texture, GodotArray> cull(Chunk chunk){  
        Stopwatch watch = new Stopwatch();  
        watch.Start();

        Dictionary<Texture, SortedDictionary<ValueTuple<int, int, int>, Face>> faces = new  Dictionary<Texture, SortedDictionary <ValueTuple<int, int, int>, Face>>();
        long a = 16777215 << 8;
        byte b = 255;
        int count = 0;

        for(int i = 0; i < chunk.voxels.Span.Length; i++){
            uint bytes = chunk.voxels.Span[i];
            watch.Start();

            int lenght = (int)(bytes & a) >> 8;
            int objectID = (int) (bytes & b);

            Texture texture = registry.SelectByID(objectID).texture;

            if(texture == null || objectID == 0){
                count += lenght;
                continue;
            }

            if(!faces.ContainsKey(texture)){
                faces.Add(texture, new SortedDictionary<ValueTuple<int, int, int>, Face>());
            }
        
            int z = count / 4096;
            int y = count % 64;
            int x = (count - 4096 * z)/64;

            //Front
            Face face = new Face();
            face.side = 0;
            face.deleteIndex = -1;
            face.vertice = new TerraVector3[6]{new TerraVector3(x, y, z)/4, new TerraVector3(x + 1, y, z)/4, new TerraVector3(x + 1, y + lenght, z)/4, new TerraVector3(x + 1, y + lenght, z)/4, new TerraVector3(x, y + lenght, z)/4, new TerraVector3(x, y, z)/4};  
            ValueTuple<int, int, int> origin = ValueTuple.Create(0, x, z);
            faces[texture].Add(origin, face);
           
            if(z > 0){     
                    ValueTuple<int, int, int> pos = ValueTuple.Create(0, x, z - 1);
                    Face prevFace = faces[texture][pos];
                    if(prevFace.deleteIndex > -1){
                        prevFace = faces[texture][ValueTuple.Create(0, x, prevFace.deleteIndex)];
                        faces[texture][pos] = prevFace;
                    }

                    if(face.vertice[3].y <= prevFace.vertice[3].y){
                        face.deleteIndex = z - 1;
                        faces[texture][origin] = face;
                    }

                    if (prevFace.vertice[3].y < face.vertice[3].y && prevFace.vertice[0].y >= face.vertice[0].y){
                         face.vertice[0] = new TerraVector3(face.vertice[0].x, prevFace.vertice[3].y, face.vertice[0].z); 
                            face.vertice[1] = new TerraVector3( face.vertice[1].x, prevFace.vertice[3].y,  face.vertice[1].z); 
                           face.vertice[5] = new TerraVector3( face.vertice[5].x, prevFace.vertice[3].y,  face.vertice[5].z); 
                        faces[texture][origin] = face;
                    }
            }

            //Back
            face.side = 1;
            face.deleteIndex = -1;
            face.vertice = new TerraVector3[6]{new TerraVector3(x + 1, y, z + 1)/4, new TerraVector3(x, y, z + 1)/4,  new TerraVector3(x, y + lenght, z + 1)/4, new TerraVector3(x, y + lenght, z + 1)/4, new TerraVector3(x + 1, y + lenght, z + 1)/4, new TerraVector3(x + 1, y, z + 1)/4};
            faces[texture].Add(ValueTuple.Create(1, x, z), face);

            if(z > 0){      
                    ValueTuple<int, int, int> pos = ValueTuple.Create(1, x, z - 1);
                    Face prevFace = faces[texture][pos];
                    if (prevFace.vertice[3].y > face.vertice[3].y && prevFace.vertice[0].y <= face.vertice[0].y){
                         prevFace.vertice[0] = new TerraVector3(prevFace.vertice[0].x, face.vertice[3].y, prevFace.vertice[0].z); 
                            prevFace.vertice[1] = new TerraVector3( prevFace.vertice[1].x, face.vertice[3].y,  prevFace.vertice[1].z); 
                           prevFace.vertice[5] = new TerraVector3( prevFace.vertice[5].x, face.vertice[3].y,  prevFace.vertice[5].z); 
                        faces[texture][pos] = prevFace;
                        }else if(prevFace.vertice[3].y <= face.vertice[3].y){
                            faces[texture].Remove(pos); 
                        }
            }
             
            //Right
            face.side = 2;
            face.deleteIndex = -1;
            face.vertice = new TerraVector3[6]{new TerraVector3(x, y, z + 1)/4, new TerraVector3(x, y, z)/4, new TerraVector3(x, y + lenght, z)/4, new TerraVector3(x, y + lenght, z)/4, new TerraVector3(x, y + lenght, z + 1)/4, new TerraVector3(x, y, z + 1)/4}; 
            origin = ValueTuple.Create(2, x, z);
            faces[texture].Add(origin, face);

            if(x > 0){     
                    ValueTuple<int, int, int> pos = ValueTuple.Create(2, x - 1, z);
                    Face prevFace = faces[texture][pos];
                    if(prevFace.deleteIndex > -1){
                        prevFace = faces[texture][ValueTuple.Create(2,  prevFace.deleteIndex, z)];
                    }

                    if(face.vertice[3].y <= prevFace.vertice[3].y){
                        face.deleteIndex = x - 1;  
                      faces[texture][origin] = face;
                    }

                    if (prevFace.vertice[3].y < face.vertice[3].y && prevFace.vertice[0].y >= face.vertice[0].y){
                        face.vertice[0] = new TerraVector3(face.vertice[0].x, prevFace.vertice[3].y, face.vertice[0].z); 
                        face.vertice[1] = new TerraVector3( face.vertice[1].x, prevFace.vertice[3].y,  face.vertice[1].z); 
                        face.vertice[5] = new TerraVector3( face.vertice[5].x, prevFace.vertice[3].y,  face.vertice[5].z); 
                        faces[texture][origin] = face;
                    }
            }
           
            //Left
            face.side = 3;
            face.deleteIndex = -1;
            face.vertice = new TerraVector3[6]{new TerraVector3(x + 1, y, z)/4, new TerraVector3(x + 1, y, z + 1)/4, new TerraVector3(x + 1, y + lenght, z + 1)/4, new TerraVector3(x + 1, y + lenght, z + 1)/4, new TerraVector3(x + 1, y + lenght, z)/4, new TerraVector3(x + 1, y, z)/4}; 
            faces[texture].Add(ValueTuple.Create(3, x, z), face);

            if(x > 0){      
                    ValueTuple<int, int, int> pos = ValueTuple.Create(3, x - 1, z);
                    Face prevFace = faces[texture][pos];
                    if (prevFace.vertice[3].y > face.vertice[3].y && prevFace.vertice[0].y <= face.vertice[0].y){
                         prevFace.vertice[0] = new TerraVector3(prevFace.vertice[0].x, face.vertice[3].y, prevFace.vertice[0].z); 
                            prevFace.vertice[1] = new TerraVector3( prevFace.vertice[1].x, face.vertice[3].y,  prevFace.vertice[1].z); 
                           prevFace.vertice[5] = new TerraVector3( prevFace.vertice[5].x, face.vertice[3].y,  prevFace.vertice[5].z); 
                           faces[texture][pos] = prevFace;
                        }else if(prevFace.vertice[3].y <= face.vertice[3].y){
                            faces[texture].Remove(pos); 
                        }
                }
    
            //Top
            face.side = 4;
            face.deleteIndex = -1;
            face.vertice = new TerraVector3[6]{new TerraVector3(x, y + lenght, z)/4, new TerraVector3(x + 1, y + lenght, z)/4, new TerraVector3(x + 1, y + lenght, z + 1)/4, new TerraVector3(x + 1, y + lenght, z + 1)/4, new TerraVector3(x, y + lenght, z + 1)/4, new TerraVector3(x, y + lenght, z)/4};
            faces[texture].Add(ValueTuple.Create(4, x, z), face);
            
            //Bottom            
            face.side = 5;
            face.deleteIndex = -1;
            face.vertice = new TerraVector3[6]{new TerraVector3(x + 1, y, z)/4, new TerraVector3(x, y, z)/4, new TerraVector3(x, y, z + 1)/4,  new TerraVector3(x, y, z + 1)/4, new TerraVector3(x + 1, y, z + 1)/4,new TerraVector3(x + 1, y, z)/4};
            faces[texture].Add(ValueTuple.Create(5, x, z), face);

            count += lenght;
        }

        GD.Print("Chunk meshed :" + watch.ElapsedMilliseconds);
        GD.Print(watch.ElapsedTicks);

        watch.Reset();

        Dictionary<Texture, GodotArray> arrays =  new Dictionary<Texture, GodotArray>();
        Texture[] textures = faces.Keys.ToArray();

        for(int t = 0; t < textures.Count(); t ++){
            GodotArray godotArray = new GodotArray();
            godotArray.Resize(9);
            
            Texture texture1 = textures[t];
            ValueTuple<int, int, int>[] keys = faces[texture1].Keys.ToArray();
            int size = keys.Count();
            Vector3[] vertice = new Vector3[size * 6];
            Vector3[] normals = new Vector3[size * 6];
            Vector2[] uvs = new Vector2[size * 6];

            for(int i = 0; i < size; i ++){
                Face face = faces[texture1][keys[i]];
                if(face.deleteIndex == -1){
                face.uvs = SetTextureCoords(texture1, face);
                Vector3 normal = new Vector3();
                switch(face.side){
                     case 0:
                        normal = new Vector3(0, 0, 1);
                        break;
                    case 1:
                        normal = new Vector3(0, 0, -1);
                        break;
                    case 2:
                        normal = new Vector3(1, 0, 0);
                        break;
                    case 3:
                        normal = new Vector3(-1, 0, 0);
                        break;
                    case 4:
                        normal = new Vector3(0, 1, 0);
                        break;
                    case 5:
                        normal = new Vector3(0, -1, 0);
                        break;
                }

                for(int f = 0; f < 6; f++){
                    watch.Start();
                    int index = (i * 6) + f;
                    vertice[index] = face.vertice[f].ToVector3();
                    normals[index] = normal;
                    uvs[index] = face.uvs[f].ToVector2();
                }
                faces[texture1].Remove(keys[i]);
                }
            }

            godotArray[0] = vertice;
            godotArray[1] = normals;
            godotArray[4] = uvs;
            faces[texture1].Clear();

            arrays.Add(texture1, godotArray);
        }

        faces.Clear();

        watch.Stop();

        GD.Print("Chunk Added :" + watch.ElapsedMilliseconds);
        GD.Print(watch.ElapsedTicks);

        return arrays;
    }

        private static TerraVector2[] SetTextureCoords(Texture texture, Face face) {
            TerraVector2[] uvs = new TerraVector2[6];
            float textureWidth = 2048f / texture.GetWidth();
            float textureHeight = 2048f / texture.GetHeight();

            switch (face.side) {
                case 2:
                case 3:
                for(int t = 0; t < 6; t++){
                  uvs[t] = new TerraVector2(face.vertice[t].z *textureWidth, face.vertice[t].y * textureHeight);
                }
                break;
                case 0:
                case 1:
                    for(int t = 0; t < 6; t++){
                         uvs[t] = new TerraVector2(face.vertice[t].x * textureWidth, face.vertice[t].z * textureHeight);
                    }
                break;
                case 4:
                case 5:
                for(int t = 0; t < 6; t++){
                     uvs[t] = new TerraVector2(face.vertice[t].x * textureWidth, face.vertice[t].y * textureHeight);
                }
                break;
            }
            return uvs;
        }
}
