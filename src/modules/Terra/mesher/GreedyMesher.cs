using Godot;

using System.Collections.Generic;
public class GreedyMesher
{
    private Dictionary<int, Dictionary<int, Face>> sectors;
    private Registry registry;
    public GreedyMesher(Registry registry) {
        sectors = new Dictionary<int, Dictionary<int, Face>>();
        for (int i = 0; i < 7; i++){
            sectors.Add(i, new Dictionary<int, Face>());
        }
        this.registry = registry;
    }

    public Dictionary<int, Dictionary<int, Face>> cull(Chunk chunk){
        for(int i = 0; i < 262144; i++){
            int objectID = chunk.voxels.Span[i];
            TerraObject terraObject = registry.SelectByID(objectID);

            if(terraObject.texture == null || objectID == 0){
                continue;
            }

            int z = i / 4096;
            int y = (i - 4096 * z) / 64;
            int x = i % 64;

            //LEFT
            if (x == 0 || chunk.voxels.Span[i - 1] != objectID){
                Face face = new Face();
                Dictionary<int, Face> side1 = sectors[0];
                face.terraObject = terraObject;
                face.normal = new Vector3(-1, 0, 0);
                face.vector3s = new Vector3[4]{new Vector3(x, y, z + 1)/4, new Vector3(x, y, z)/4, new Vector3(x, y + 1, z)/4, new Vector3(x, y + 1, z + 1)/4};
                side1.Add(i, face);

                //Naive Greedy Meshing
                if (i > 4096) {
                    if (side1.ContainsKey(i - 4096)) {
                    Face previousFace = side1[i - 4096];
                      if(previousFace.terraObject == face.terraObject) {
                       if(previousFace.terraObject == face.terraObject) {
                        if (face.vector3s[1] == previousFace.vector3s[0] &&
                                face.vector3s[2] == previousFace.vector3s[3]) {
                            face.vector3s[1] = previousFace.vector3s[1];
                            face.vector3s[2] = previousFace.vector3s[2];
                            side1.Remove(i - 4096);
                        }
                    }
                    }
                    }
                }
            }

            // RIGHT
            if (x == 63 || chunk.voxels.Span[i + 1] != objectID) {
                Face face = new Face();
                face.terraObject = terraObject;
                face.normal = new Vector3(1, 0, 0);
                Dictionary<int, Face> side2 = sectors[1];
                face.vector3s = new Vector3[4]{new Vector3(x + 1, y, z)/4, new Vector3(x + 1, y, z + 1)/4, new Vector3(x + 1, y + 1, z + 1)/4, new Vector3(x + 1, y + 1, z)/4};
                side2.Add(i, face);

                //Naive Greedy Meshing
                if (i > 4096) {
                     if (side2.ContainsKey(i - 4096)){
                    Face previousFace = side2[i - 4096];
                   if(previousFace.terraObject == face.terraObject) {
                        if (face.vector3s[0] == previousFace.vector3s[1] &&
                                face.vector3s[3] == previousFace.vector3s[2]) {
                            face.vector3s[0] = previousFace.vector3s[0];
                            face.vector3s[3] = previousFace.vector3s[3];
                            side2.Remove(i - 4096);
                        }
                    }
                    }
                }
            }

            // TOP
            if (y == 63 || chunk.voxels.Span[i + 64] != objectID) {
                Face face = new Face();
                face.terraObject = terraObject;
                face.normal = new Vector3(0, 1, 0);
                Dictionary<int, Face> side3 = sectors[2];
                face.vector3s = new Vector3[4]{new Vector3(x, y + 1, z)/4, new Vector3(x + 1, y + 1, z)/4, new Vector3(x + 1, y + 1, z + 1)/4, new Vector3(x, y + 1, z + 1)/4};
                side3.Add(i, face);

                //Naive Greedy Meshing
                if (i > 1) {
                    if (side3.ContainsKey(i - 1)) {
                    Face previousFace = side3[i - 1];
                    if(previousFace.terraObject == face.terraObject) {
                        if (face.vector3s[3] == previousFace.vector3s[2] &&
                                face.vector3s[0] == previousFace.vector3s[1]) {
                            face.vector3s[3] = previousFace.vector3s[3];
                            face.vector3s[0] = previousFace.vector3s[0];
                            side3.Remove(i - 1);
                        }
                    }
                    }
                }
            }

            // BOTTOM
            if (y == 0 || y > 0 && chunk.voxels.Span[i - 64] != objectID) {
                Face face = new Face();
                face.terraObject = terraObject;
                face.normal = new Vector3(0, -1, 0);
                Dictionary<int, Face> side4 = sectors[3];
                face.vector3s = new Vector3[4]{new Vector3(x + 1, y, z)/4, new Vector3(x, y, z)/4,  new Vector3(x, y, z + 1)/4, new Vector3(x + 1, y, z + 1)/4};
                side4.Add(i, face);

                //Naive Greedy Meshing
                if (i > 1) {
                    if (side4.ContainsKey(i - 1)){
                    Face previousFace = side4[i - 1];
                    if (previousFace.terraObject == (face.terraObject)) {
                            if (face.vector3s[2] == (previousFace.vector3s[3]) &&
                                    face.vector3s[1] == (previousFace.vector3s[0])) {
                                face.vector3s[2] = previousFace.vector3s[2];
                                face.vector3s[1] = previousFace.vector3s[1];
                                side4.Remove(i - 1);
                            }
                        }
                    }
                }
            }
            // BACK
                if (z == 63 || chunk.voxels.Span[i + 4096] != objectID) {
                    Face face = new Face();
                    face.terraObject = terraObject;
                    face.normal = new Vector3(0, 0, -1);
                    Dictionary<int, Face> side5 = sectors[4];    
                    face.vector3s = new Vector3[4]{new Vector3(x + 1, y, z + 1)/4, new Vector3(x, y, z + 1)/4,  new Vector3(x, y + 1, z + 1)/4, new Vector3(x + 1, y + 1, z + 1)/4};        
                    side5.Add(i, face);

                    //Naive Greedy Meshing
                    if (i > 1) {
                        if (side5.ContainsKey(i - 1)){
                        Face previousFace = side5[i - 1];
                         if (previousFace.terraObject == (face.terraObject)) {
                            if (face.vector3s[2] == (previousFace.vector3s[3]) &&
                                    face.vector3s[1] == (previousFace.vector3s[0])) {
                                face.vector3s[2] = previousFace.vector3s[2];
                                face.vector3s[1] = previousFace.vector3s[1];
                                side5.Remove(i - 1);
                            }
                        }
                    }
                    }
                }

            // FRONT
            if (z == 0 || chunk.voxels.Span[i - 4096] != objectID) {
                Face face = new Face();
                face.terraObject = terraObject;
                face.normal = new Vector3(0, 0, 1);
                Dictionary<int, Face> side6 = sectors[5];
                face.vector3s = new Vector3[4]{new Vector3(x, y, z)/4, new Vector3(x + 1, y, z)/4, new Vector3(x + 1, y + 1, z)/4, new Vector3(x, y + 1, z)/4};  
                side6.Add(i, face);

                //Naive Greedy Meshing
                if (i > 1) {
                    if (side6.ContainsKey(i - 1)){
                    Face previousFace = side6[i - 1];
                     if(previousFace.terraObject == face.terraObject) {
                        if (face.vector3s[3] == previousFace.vector3s[2] &&
                                face.vector3s[0] == previousFace.vector3s[1]) {
                            face.vector3s[3] = previousFace.vector3s[3];
                            face.vector3s[0] = previousFace.vector3s[0];
                            side6.Remove(i - 1);
                        }
                    }
                    }
                }
            }
        }

        return sectors;
    }
}
