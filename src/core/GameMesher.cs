using System.Linq;
using System;
using System.Collections.Generic;
using Godot;
using GodotArray = Godot.Collections.Array;
public class GameMesher
{
    private List<MeshInstance> instances;
    private GreedyMesher greedyMesher;
    private SplatterMesher splatterMesher;

    public GameMesher(List<MeshInstance> instances, Registry reg){
        this.instances = instances;
        ShaderMaterial shaderMat = new ShaderMaterial();
        shaderMat.SetShader(GD.Load("res://assets/shaders/splatvoxel.shader") as Shader);
        greedyMesher = new GreedyMesher(reg);
        splatterMesher = new SplatterMesher(shaderMat, reg);
    }

    public void ChunkLoaded(Chunk chunk, bool splatter){
        MeshInstance meshInstance = new MeshInstance();
        if(!splatter){
            GreedMeshing(meshInstance, chunk);
        }else{
            meshInstance = splatterMesher.CreateChunkMesh(chunk);
        }

        instances.Add(meshInstance);
    }

    public void GreedMeshing(MeshInstance meshInstance, Chunk chunk){
        Dictionary<Texture, List<Vector3>> verticeArrays = new Dictionary<Texture,  List<Vector3>>();
        Dictionary<Texture, List<Vector2>> textureCoordArrays = new Dictionary<Texture,  List<Vector2>>();
        Dictionary<int, Dictionary<int, Face>> sector = greedyMesher.cull(chunk);
        // Reset buffer to starting position

        if (sector.Count > 0) {
            //Finishing greedy meshing
            foreach (int key in sector.Keys) {
                if (key != 6) {
                    Dictionary<int, Face> faces = sector[key];
                    int[] keys = faces.Keys.ToArray();
                    Array.Sort(keys);
                    for (int i = keys.Length - 1; i >= 0; i--) {
                        int index = keys[i];
                        JoinReversed(faces, index, key);
                    }
                }
            }

            foreach (int key in sector.Keys) {
                if (key != 6) {

                    Dictionary<int, Face> faces = sector[key];

                    foreach (int faceKey in faces.Keys.ToArray()) {
                        Face completeFace = faces[faceKey];

                        Texture texture = completeFace.terraObject.texture;

                        if(!verticeArrays.ContainsKey(texture) || !textureCoordArrays.ContainsKey(texture)){
                            verticeArrays.Add(texture, new List<Vector3>());
                            textureCoordArrays.Add(texture, new List<Vector2>());
                        }

                        List<Vector2> textureCoords = textureCoordArrays[texture];
                        List<Vector3> vector3s = verticeArrays[texture];
            
                        textureCoords.Add(new Vector2(0, 0));
                        textureCoords.Add(new Vector2(texture.GetWidth()/2048f, 0));
                        textureCoords.Add(new Vector2(texture.GetWidth()/2048f, texture.GetHeight()/2048f));
                        textureCoords.Add(new Vector2(texture.GetWidth()/2048f, texture.GetHeight()/2048f));
                        textureCoords.Add(new Vector2(0, texture.GetHeight()/2048f));
                        textureCoords.Add(new Vector2(0, 0));

                        vector3s.Add(completeFace.vector3s[0]);
                        vector3s.Add(completeFace.vector3s[1]);
                        vector3s.Add(completeFace.vector3s[2]);
                        vector3s.Add(completeFace.vector3s[2]);
                        vector3s.Add(completeFace.vector3s[3]);
                        vector3s.Add(completeFace.vector3s[0]);

                        sector[key].Remove(faceKey);
                    }
                    faces.Clear();
                }
            }

            //Unusual meshes
            Dictionary<int, Face> side = sector[6];
           /* for (Integer i : side.keySet()) {
                Face face = side.get(i);
                Integer id = face.getObject().getWorldId();

                if (unsualMeshSize.get(id) == null) {
                    unsualMeshSize.put(id, 1);
                    TerraObject object = reg.getForWorldId(id);
                    int posZ = i / 4096;
                    int posY = (i - (4096 * posZ)) / 64;
                    int posX = i % 64;
                    object.position(
                            (posX * 0.25f) + chunk.getPosX() + (object.getMesh().getDefaultDistanceX() * 0.25f) / 2f,
                            (posY * 0.25f) + chunk.getPosY(),
                            (posZ * 0.25f) + chunk.getPosZ() + (object.getMesh().getDefaultDistanceZ() * 0.25f) / 2f);
                } else {
                    int s = unsualMeshSize.get(id);
                    s += 1;
                    unsualMeshSize.replace(id, s);
                }

                Integer[] currentKeys = new Integer[unsualMeshSize.keySet().size()];
                unsualMeshSize.keySet().toArray(currentKeys);
                for (Integer objectId : currentKeys) {
                    if (objectId != null) {
                        TerraObject object = reg.getForWorldId(objectId);
                        if (object.getMesh().getSize() == unsualMeshSize.get(objectId)) {
                            unsualMeshSize.remove(objectId);

                            Spatial asset = modelLoader3D.getMesh(object.getMesh().getAsset());

                            while (asset instanceof Node) {
                                asset = ((Node) asset).getChild(0);
                            }

                            Geometry assetGeom = (Geometry) asset;

                            FloatBuffer normalBuffer = FloatBuffer.allocate(6 * assetGeom.getTriangleCount());

                            for (int n = 0; n < normalBuffer.capacity(); n++) {
                                normalBuffer.put(0);
                            }

                            texCoordsBuffer = BufferUtils.createFloatBuffer(
                                    ((assetGeom.getMesh().getFloatBuffer(VertexBuffer.Type.TexCoord).capacity()
                                            / 2) * 3));
                            for (int t = 0; t < assetGeom.getMesh().getFloatBuffer(VertexBuffer.Type.TexCoord).capacity();
                                 t += 2) {
                                texCoordsBuffer.put(assetGeom.getMesh().getFloatBuffer(VertexBuffer.Type.TexCoord).get(t));
                                texCoordsBuffer.put(assetGeom.getMesh().getFloatBuffer(VertexBuffer.Type.TexCoord).get(t + 1));
                                texCoordsBuffer.put(object.getTexture().getPosition());
                            }

                            Mesh newMash = new Mesh();
                            newMash.setBuffer(assetGeom.getMesh().getBuffer(VertexBuffer.Type.Position));
                            newMash.setBuffer(assetGeom.getMesh().getBuffer(VertexBuffer.Type.Index));
                            newMash.setBuffer(VertexBuffer.Type.TexCoord, 3, texCoordsBuffer);
                            newMash.setBuffer(VertexBuffer.Type.Normal, 3, normalBuffer);

                            assetGeom.setMesh(newMash);
                            assetGeom.setLocalTranslation(object.getX(), object.getY(), object.getZ());
                            assetGeom.setMaterial(mat);
                            assetGeom.updateModelBound();
                            node.attachChild(assetGeom);
                        }
                    }
                }
            }*/

        ArrayMesh mesh = new ArrayMesh();

        meshInstance.Name = "chunk:" + chunk.x + "," + chunk.y + "," + chunk.z;
        meshInstance.Translate(new Vector3(chunk.x, chunk.y, chunk.z));

        foreach(Texture texture1 in verticeArrays.Keys.ToArray()){
            GodotArray arrays = new GodotArray();
            arrays.Resize(9);
            
            SpatialMaterial material = new SpatialMaterial();
            material.SetTexture(SpatialMaterial.TextureParam.Albedo, texture1);
            
            arrays[0] = verticeArrays[texture1].ToArray();;
            arrays[4] = textureCoordArrays[texture1].ToArray();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
            mesh.SurfaceSetMaterial(mesh.GetSurfaceCount() -1, material);
            arrays.Clear();
        }

        meshInstance.SetMesh(mesh);
        verticeArrays.Clear();
        textureCoordArrays.Clear();
        }
    }

    private static void JoinReversed(Dictionary<int, Face> faces, int index, int side) {
        int neighbor = 64;
        switch (side) {
            case 2:
            case 3:
                neighbor = 4096;
                break;
        }

        if(faces.ContainsKey(index - neighbor) && faces.ContainsKey(index)){
        Face nextFace = faces[index - neighbor];

        Face face = faces[index];
        if (face.terraObject == (nextFace.terraObject)) {
            if (nextFace.vector3s[2] == (face.vector3s[3]) && nextFace.vector3s[1] == (face.vector3s[0])) {
                nextFace.vector3s[1] = face.vector3s[1];
                nextFace.vector3s[2] = face.vector3s[2];
                faces.Remove(index);
            } else if (nextFace.vector3s[3] == (face.vector3s[2]) && nextFace.vector3s[0] == (face.vector3s[1])) {
                nextFace.vector3s[3] = face.vector3s[3];
                nextFace.vector3s[0] = face.vector3s[0];
                faces.Remove(index);
            }
        }
        }
    }
}