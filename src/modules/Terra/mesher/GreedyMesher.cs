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
        Dictionary<Texture, Dictionary<Tuple<int, int>, Face>> faces = new  Dictionary<Texture, Dictionary <Tuple<int, int>, Face>>();
        long a = 16777215 << 8;
        byte b = 255;

        int count = 0;

        for(int i = 0; i < chunk.voxels.Span.Length; i++){
            uint bytes = chunk.voxels.Span[i];

            int lenght = (int)(bytes & a) >> 8;
            int objectID = (int) (bytes & b);

            Texture texture = registry.SelectByID(objectID).texture;

            if(texture == null || objectID == 0){
                count += lenght;
                continue;
            }

            if(!faces.ContainsKey(texture)){
                faces.Add(texture, new Dictionary<Tuple<int, int>, Face>());
            }
        
            int z = count / 4096;
            int y = count % 64;
            int x = (count - 4096 * z)/64;

            //Front
          /*  if(!vertice[texture].ContainsKey(0)){
                vertice[texture].Add(0, new List<Vector3>());
                normals[texture].Add(0, new List<Vector3>());
                uvs[texture].Add(0, new List<Vector2>());

                vertice[texture][0].AddRange(new Vector3[6]{new Vector3(x + 1, y, z + 1)/4, new Vector3(x, y, z + 1)/4,  new Vector3(x, y + lenght, z + 1)/4, new Vector3(x, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y, z + 1)/4});  
                SetTextureCoords(texture, uvs[texture][0], vertice[texture][0], 2);

                for(int n = 0; n < 6; n ++){
                    normals[texture][0].Add(new Vector3(0, 0, -1));
                }  
            }else{
                int size = vertice[texture][0].Count;
                    if(z > 0){
                        if(vertice[texture][0][size - 382].y <= lenght){

                            for(int r = 0; r < 6; r ++){
                                vertice[texture][0].RemoveAt(size - 384 + r); 
                                normals[texture][0].RemoveAt(size - 384 + r); 
                                uvs[texture][0].RemoveAt(size - 384 + r); 
                            }
                        }else{
                            vertice[texture][0][size - 384].Set(x + 1, y + lenght, z - 1); 
                            vertice[texture][0][size - 383].Set(x, y + lenght, z - 1); 
                           vertice[texture][0][size - 379].Set(x + 1, y + lenght, z - 1); 
                        }

                        vertice[texture][0].AddRange(new Vector3[6]{new Vector3(x + 1, y, z + 1)/4, new Vector3(x, y, z + 1)/4,  new Vector3(x, y + lenght, z + 1)/4, new Vector3(x, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y, z + 1)/4});  
                        SetTextureCoords(texture, uvs[texture][0], vertice[texture][0], 2);

                        for(int n = 0; n < 6; n ++){
                            normals[texture][0].Add(new Vector3(0, 0, -1));
                        }
                    }else{
                        vertice[texture][0].AddRange(new Vector3[6]{new Vector3(x + 1, y, z + 1)/4, new Vector3(x, y, z + 1)/4,  new Vector3(x, y + lenght, z + 1)/4, new Vector3(x, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y, z + 1)/4});  
                    SetTextureCoords(texture, uvs[texture][0], vertice[texture][0], 2);

                    for(int n = 0; n < 6; n ++){
                        normals[texture][0].Add(new Vector3(0, 0, -1));
                    }
                }
            }*/

            //Back
            if(faces.ContainsKey(texture)){
                    if(z > 0){      
                        Face prevFace = faces[texture][new Tuple<int, int>(1, x + ((z - 1) * 64))];
                        if(prevFace.vertice[3].y <= (lenght - y)/4){
                            faces[texture].Remove(new Tuple<int, int>(1, x + ((z - 1) * 64))); 
                        }else if (prevFace.vertice[3].y > (lenght - y)/4 && prevFace.vertice[0].y < (lenght - y)/4){
                         prevFace.vertice[0] = new Vector3(prevFace.vertice[0].x, (y + lenght)/4, prevFace.vertice[0].z); 
                            prevFace.vertice[1] = new Vector3( prevFace.vertice[1].x,  (y + lenght)/4,  prevFace.vertice[1].z); 
                           prevFace.vertice[5] = new Vector3( prevFace.vertice[5].x,  (y + lenght)/4,  prevFace.vertice[5].z); 
                        }
                    }
            }

            Face face = new Face();
            face.side = 1;
            face.vertice = new Vector3[6]{new Vector3(x + 1, y, z + 1)/4, new Vector3(x, y, z + 1)/4,  new Vector3(x, y + lenght, z + 1)/4, new Vector3(x, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y, z + 1)/4};
            faces[texture].Add(new Tuple<int, int>(1, x + (z * 64)), face);

            /*
            sideID = 0;
            pos = i;
            for(int m = 0; m < 64 - lenght; m++){
                if(pos < chunk.voxels.Span.Length){
                    bytes = chunk.voxels.Span[pos];
                    int sideLenght = (int)(bytes & a) >> 8;
                    sideID = (int) (bytes & b);
                    pos ++;
                    m += sideLenght;
                }else{
                    break;
                }
            }

            //Right
            if(objectID != sideID){
                vertice[texture].AddRange(new Vector3[6]{new Vector3(x + 1, y, z)/4, new Vector3(x + 1, y, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z)/4, new Vector3(x + 1, y, z)/4});  
                SetTextureCoords(texture, uvs[texture], vertice[texture], 0);

                for(int n = 0; n < 6; n ++){
                    normals[texture].Add(new Vector3(1, 0, 0));
                }   
            }

            sideID = 0;
            pos = i;
            for(int m = 0; m < 64 - lenght; m++){
                if(pos > 0){
                    bytes = chunk.voxels.Span[pos];
                    int sideLenght = (int)(bytes & a) >> 8;
                    sideID = (int) (bytes & b);
                    pos --;
                    m += sideLenght;
                }else{
                    break;
                }
            }
            
            //Left
            if(objectID != sideID){
                vertice[texture].AddRange(new Vector3[6]{new Vector3(x, y, z + 1)/4, new Vector3(x, y, z)/4, new Vector3(x, y + lenght, z)/4,  new Vector3(x, y + lenght, z)/4, new Vector3(x, y + lenght, z + 1)/4, new Vector3(x, y, z + 1)/4});  
                SetTextureCoords(texture, uvs[texture], vertice[texture], 0);

                for(int n = 0; n < 6; n ++){
                    normals[texture].Add(new Vector3(-1, 0, 0));
                }   
            }
            */

        /*
         if(!vertice[texture].ContainsKey(4)){
                vertice[texture].Add(4, new List<Vector3>());
                normals[texture].Add(4, new List<Vector3>());
                uvs[texture].Add(4, new List<Vector2>());
            }
            //Top
            vertice[texture][4].AddRange(new Vector3[6]{new Vector3(x, y + lenght, z)/4, new Vector3(x + 1, y + lenght, z)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x + 1, y + lenght, z + 1)/4, new Vector3(x, y + lenght, z + 1)/4, new Vector3(x, y + lenght, z)/4});  
            SetTextureCoords(texture, uvs[texture][4], vertice[texture][4], 1);

            for(int n = 0; n < 6; n ++){
                normals[texture][4].Add(new Vector3(0, 1, 0));
            }*/

            //Bottom
            /*
            vertice[texture][4].AddRange(new Vector3[6]{new Vector3(x + 1, y, z)/4, new Vector3(x, y, z)/4, new Vector3(x, y, z + 1)/4,  new Vector3(x, y, z + 1)/4, new Vector3(x + 1, y, z + 1)/4,new Vector3(x + 1, y, z)/4});  
            SetTextureCoords(texture, uvs[texture][4], vertice[texture][4], 1);

            for(int n = 0; n < 6; n ++){
                normals[texture][4].Add(new Vector3(0, -1, 0));
            }    */
            count += lenght;
        
        }

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
                    case 1:
                        normal = new Vector3(0, 0, 1);
                        break;
                }

                for(int f = 0; f < 6; f++){
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

        return arrays;
    }

        private static Vector2[] SetTextureCoords(Texture texture, Face face) {
            Vector2[] uvs = new Vector2[6];
            switch (face.side) {
                case 0:
                for(int t = 0; t < 6; t++){
                  uvs[t] = new Vector2(face.vertice[t].z * 2048f / texture.GetWidth(), face.vertice[t].y * 2048f / texture.GetHeight());
                }
                break;
                case 1:
                    for(int t = 0; t < 6; t++){
                         uvs[t] = new Vector2(face.vertice[t].x * 2048f / texture.GetWidth(), face.vertice[t].z * 2048f / texture.GetHeight());
                    }
                break;
                case 2:
                for(int t = 0; t < 6; t++){
                     uvs[t] = new Vector2(face.vertice[t].x * 2048f / texture.GetWidth(), face.vertice[t].y * 2048f / texture.GetHeight());
                }
                break;
            }
            return uvs;
        }
}
