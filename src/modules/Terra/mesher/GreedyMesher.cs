using System.Collections;
using System.Diagnostics;
using System;
using System.Linq;
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
        Dictionary<Texture, SortedDictionary<Tuple<int, int, int>, Face>> faces = new  Dictionary<Texture, SortedDictionary <Tuple<int, int, int>, Face>>();
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
                faces.Add(texture, new SortedDictionary<Tuple<int, int, int>, Face>());
            }
        
            int z = count / 4096;
            int y = count % 64;
            int x = (count - 4096 * z)/64;

            //Front
            Face face = new Face();
            face.side = 0;
            face.vertice = new Vector3[6]{new Vector3(x, y, z)/4, new Vector3(x + 1, y, z)/4, new Vector3(x + 1, y + 1, z)/4, new Vector3(x + 1, y + 1, z)/4, new Vector3(x, y + 1, z)/4, new Vector3(x, y, z)/4};  
            Tuple<int, int, int>  pos = new Tuple<int, int, int>(0, x, z);
            faces[texture].Add(pos, face);
            if(z > 0){     
                if(z > 1){ 
                    Tuple<int, int, int> prevPos = new Tuple<int, int, int>(0, x, z - 1);
                    Face prev2Face = faces[texture][new Tuple<int, int, int>(0, x, z - 2)];            
                    Face prevFace = faces[texture][prevPos];
                    if(prevFace.vertice[3].y <= prev2Face.vertice[3].y){
                        faces[texture].Remove(prevPos); 
                    }
                }

                   /* if (prevFace.vertice[3].y > face.vertice[3].y && prevFace.vertice[0].y >= face.vertice[0].y){
                         face.vertice[0] = new Vector3(face.vertice[0].x, prevFace.vertice[3].y, face.vertice[0].z); 
                            face.vertice[1] = new Vector3( face.vertice[1].x, prevFace.vertice[3].y,  face.vertice[1].z); 
                           face.vertice[5] = new Vector3( face.vertice[5].x, prevFace.vertice[3].y,  face.vertice[5].z); 
                    }*/
            }

            /*//Back
            face = new Face();
            face.side = 1;
            face.vertice = new Vector3[6]{new Vector3(x + 1, y, z + 1)/4, new Vector3(x, y, z + 1)/4,  new Vector3(x, y + lenght, z + 1)/4, new Vector3(x, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y, z + 1)/4};
            faces[texture].Add(new Tuple<int, int, int>(1, x, z), face);
            if(z > 0){      
                     pos = new Tuple<int, int, int>(1, x, z - 1);
                    Face prevFace = faces[texture][pos];
                    if (prevFace.vertice[3].y > face.vertice[3].y && prevFace.vertice[0].y <= face.vertice[0].y){
                         prevFace.vertice[0] = new Vector3(prevFace.vertice[0].x, face.vertice[3].y, prevFace.vertice[0].z); 
                            prevFace.vertice[1] = new Vector3( prevFace.vertice[1].x, face.vertice[3].y,  prevFace.vertice[1].z); 
                           prevFace.vertice[5] = new Vector3( prevFace.vertice[5].x, face.vertice[3].y,  prevFace.vertice[5].z); 
                        }else if(prevFace.vertice[3].y <= face.vertice[3].y){
                            faces[texture].Remove(pos); 
                        }
                    }
             

            //Right
            face = new Face();
            face.side = 2;
            face.vertice = new Vector3[6]{new Vector3(x + 1, y, z)/4, new Vector3(x + 1, y, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z)/4, new Vector3(x + 1, y, z)/4};
            faces[texture].Add(new Tuple<int, int, int>(2, x, z), face);
            
            //Left
            face = new Face();
            face.side = 3;
            face.vertice = new Vector3[6]{new Vector3(x, y, z + 1)/4, new Vector3(x, y, z)/4, new Vector3(x, y + lenght, z)/4,  new Vector3(x, y + lenght, z)/4, new Vector3(x, y + lenght, z + 1)/4, new Vector3(x, y, z + 1)/4};
            faces[texture].Add(new Tuple<int, int, int>(3, x, z), face);

            //Top
            face = new Face();
            face.side = 4;
            face.vertice = new Vector3[6]{new Vector3(x, y + lenght, z)/4, new Vector3(x + 1, y + lenght, z)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x, y + lenght, z + 1)/4, new Vector3(x, y + lenght, z)/4};
            faces[texture].Add(new Tuple<int, int, int>(4, x, z), face);

            
            //Bottom            
            face = new Face();
            face.side = 5;
            face.vertice = new Vector3[6]{new Vector3(x + 1, y, z)/4, new Vector3(x, y, z)/4, new Vector3(x, y, z + 1)/4,  new Vector3(x, y, z + 1)/4, new Vector3(x + 1, y, z + 1)/4,new Vector3(x + 1, y, z)/4};
            faces[texture].Add(new Tuple<int, int, int>(5, x, z), face);
            */

            count += lenght;
        }

        GD.Print("Chunk meshed :" + watch.ElapsedMilliseconds);
        GD.Print(watch.ElapsedTicks);

        watch.Reset();

        Dictionary<Texture, GodotArray> arrays =  new Dictionary<Texture, GodotArray>();

        foreach(Texture texture1 in faces.Keys.ToArray()){
            GodotArray godotArray = new GodotArray();
            godotArray.Resize(9);

            Face[] simpleFaces = faces[texture1].Values.ToArray();

            int size = simpleFaces.Count();

            Vector3[] vertice = new Vector3[size * 6];
            Vector3[] normals = new Vector3[size * 6];
            Vector2[] uvs = new Vector2[size * 6];


            for(int i = 0; i < size; i ++){
                Face face = simpleFaces[i];
                face.uvs = SetTextureCoords(texture1, face);
                Vector3 normal = new Vector3();
                switch(face.side){
                     case 0:
                        normal = new Vector3(0, 0, -1);
                        break;
                    case 1:
                        normal = new Vector3(0, 0, 1);
                        break;

                    case 4:
                        normal = new Vector3(0, 1, 0);
                        break;
                }

                for(int f = 0; f < 6; f++){
                    watch.Start();
                    vertice[(i * 6) + f] = face.vertice[f];
                    normals[(i * 6) + f] = normal;
                    uvs[(i * 6) + f] = face.uvs[f];
                }
            }

            godotArray[0] = vertice;
            godotArray[1] = normals;
            godotArray[4] = uvs;

            arrays.Add(texture1, godotArray);
        }

        faces.Clear();

        watch.Stop();

        GD.Print("Chunk Added :" + watch.ElapsedMilliseconds);
        GD.Print(watch.ElapsedTicks);

        return arrays;
    }

        private static Vector2[] SetTextureCoords(Texture texture, Face face) {
            Vector2[] uvs = new Vector2[6];
            switch (face.side) {
                case 2:
                case 3:
                for(int t = 0; t < 6; t++){
                  uvs[t] = new Vector2(face.vertice[t].z * 2048f / texture.GetWidth(), face.vertice[t].y * 2048f / texture.GetHeight());
                }
                break;
                case 0:
                case 1:
                    for(int t = 0; t < 6; t++){
                         uvs[t] = new Vector2(face.vertice[t].x * 2048f / texture.GetWidth(), face.vertice[t].z * 2048f / texture.GetHeight());
                    }
                break;
                case 4:
                case 5:
                for(int t = 0; t < 6; t++){
                     uvs[t] = new Vector2(face.vertice[t].x * 2048f / texture.GetWidth(), face.vertice[t].y * 2048f / texture.GetHeight());
                }
                break;
            }
            return uvs;
        }
}
