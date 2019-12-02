using Godot;
using GodotArray = Godot.Collections.Array;
using System.Collections.Generic;
public class GreedyMesher
{
    private volatile Registry registry;
    public GreedyMesher(Registry registry) {
        this.registry = registry;
    }

    public void cull(Chunk chunk, Node parent, MeshInstance meshInstance){
        int lastX = 0;
        int lastY = 0;
        int lastZ = 0;
        List<Vector3> shapeFaces = new List<Vector3>();
        List<Vector3> verticeArrays = new List<Vector3>();
        List<Vector3> normalsArrays = new List<Vector3>();
        List<Vector2> textureCoordArrays = new List<Vector2>();
        List<int> indexArrays = new List<int>();
        ArrayMesh mesh = new ArrayMesh();

        for(int i = 0; i < chunk.voxels.Span.Length; i++){
            uint bytes = chunk.voxels.Span[i];

            long a = 16777215 << 8;
            long count = (bytes & a) >> 8;
            long b = 255;
            int id = (int)(bytes & b);

            TerraObject terraObject = registry.SelectByID(id);

            if(terraObject.texture == null || id == 0){
                continue;
            }

            int z = i / 4096;
            int y = i % 64;
            int x = (i - 4096 * z) / 64;

           // verticeArrays.Add(new Vector3[4]{new Vector3(), new Vector3(), new Vector3(), new Vector3()};

            meshInstance.Name = "chunk:" + chunk.x + "," + chunk.y + "," + chunk.z;
            meshInstance.Translate(new Vector3(chunk.x, chunk.y, chunk.z));

            GodotArray arrays = new GodotArray();
            arrays.Resize(9);
            Texture texture = terraObject.texture;
            SpatialMaterial material = new SpatialMaterial();
            texture.Flags = 2;
            material.AlbedoTexture = texture;

            //FRONT
            verticeArrays.Add(new Vector3(x, y, z));
            verticeArrays.Add(new Vector3(x + 1, y, z));
            verticeArrays.Add(new Vector3(x, y, z));
            verticeArrays.Add(new Vector3(x, y, z));
            verticeArrays.Add(new Vector3(x, y, z));
            verticeArrays.Add(new Vector3(x, y, z));
            //BACK
            //TOP
            //BOTTOM
            //RIGHT
            //LEFT
            
//            arrays[0] = verticeArrays;
            //arrays[1] = normalsArrays;
            //arrays[4] = textureCoordArrays;
            //arrays[8] = indexArrays;
       /*     mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
            mesh.SurfaceSetMaterial(mesh.GetSurfaceCount() - 1, material);

            verticeArrays.Clear();
            normalsArrays.Clear();
            textureCoordArrays.Clear();
            indexArrays.Clear();
        }

        ConcavePolygonShape shape = new ConcavePolygonShape();
        //shape.SetFaces(shapeFaces.ToArray());
        StaticBody body = new StaticBody();
        CollisionShape colShape = new CollisionShape();
        colShape.SetShape(shape);
        body.AddChild(colShape);
        
        meshInstance.SetMesh(mesh);
        meshInstance.AddChild(body);
        /*  foreach(Node node in parent.GetChildren()){
            if(node.Name.Equals(meshInstance.Name)){
                parent.RemoveChild(node);
            }
        }

        parent.AddChild(meshInstance);*/
        }
    }

       private static Face SetTextureCoords(Face completeFace, int side) {
            completeFace.UVs = new Vector2[4];
            switch (side) {
                case 0:
                case 1:
                    completeFace.UVs[0]= new Vector2(completeFace.vector3s[0].z * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[0].y * 2048f / completeFace.terraObject.texture.GetHeight());
                    completeFace.UVs[1]= new Vector2(completeFace.vector3s[1].z * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[1].y * 2048f / completeFace.terraObject.texture.GetHeight());
                    completeFace.UVs[2]= new Vector2(completeFace.vector3s[2].z * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[2].y * 2048f / completeFace.terraObject.texture.GetHeight());
                    completeFace.UVs[3]= new Vector2(completeFace.vector3s[3].z * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[3].y * 2048f / completeFace.terraObject.texture.GetHeight());
                    return completeFace;

                case 2:
                case 3:
                    completeFace.UVs[0]= new Vector2(completeFace.vector3s[0].x * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[0].z * 2048f / completeFace.terraObject.texture.GetHeight());
                    completeFace.UVs[1]= new Vector2(completeFace.vector3s[1].x * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[1].z * 2048f / completeFace.terraObject.texture.GetHeight());
                    completeFace.UVs[2]= new Vector2(completeFace.vector3s[2].x * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[2].z * 2048f / completeFace.terraObject.texture.GetHeight());
                    completeFace.UVs[3]= new Vector2(completeFace.vector3s[3].x * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[3].z * 2048f / completeFace.terraObject.texture.GetHeight());
                    return completeFace;

                case 4:
                case 5:
                    completeFace.UVs[0]= new Vector2(completeFace.vector3s[0].x * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[0].y * 2048f / completeFace.terraObject.texture.GetHeight());
                    completeFace.UVs[1]= new Vector2(completeFace.vector3s[1].x * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[1].y * 2048f / completeFace.terraObject.texture.GetHeight());
                    completeFace.UVs[2]= new Vector2(completeFace.vector3s[2].x * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[2].y * 2048f / completeFace.terraObject.texture.GetHeight());
                    completeFace.UVs[3]= new Vector2(completeFace.vector3s[3].x * 2048f / completeFace.terraObject.texture.GetWidth(), completeFace.vector3s[3].y * 2048f / completeFace.terraObject.texture.GetHeight());
                    return completeFace;
            }
            return completeFace;
        }

        private static Vector3[] GetVertice(Face completeFace){
            Vector3[] list = new Vector3[4];
            for(int i = 0; i < 4; i ++){
                list[i] = completeFace.vector3s[i];
            }
            return list;
        }

        private static Vector3[] GetNormals(Face completeFace){
            Vector3[] list = new Vector3[4];
               for(int i = 0; i < 4; i ++){
                    list[i] = completeFace.normal;
                }
            return list;
        }
        private static Vector2[] GetTextureCoords(Face completeFace){
             Vector2[] list = new Vector2[4];

             for(int i = 0; i < 4; i ++){
                    list[i] = completeFace.UVs[i];
            }

             return list;
        }

         private static  Vector3[] GetShapeFaces(Face completeFace){
                Vector3[] list = new  Vector3[6];
                
                list[0] = completeFace.vector3s[0];
                list[1] = completeFace.vector3s[1];
                list[2] = completeFace.vector3s[2];
                list[3] = completeFace.vector3s[2];
                list[4] = completeFace.vector3s[3];
                list[5] = completeFace.vector3s[0];

             return list;
        }
}
