using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
public class GameMesher
{
    private List<MeshInstance> instances;
    private GreedyMesher greedyMesher;

    public GameMesher(List<MeshInstance> instances, Registry reg){
        this.instances = instances;
        greedyMesher = new GreedyMesher(reg);
    }

    public void ChunkLoaded(Chunk chunk){
        int verticeIndex = 0;
        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        SpatialMaterial material = new SpatialMaterial();
       surfaceTool.AddColor(new Color(0, 255, 0, 127));
        surfaceTool.SetMaterial(material);

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
                       // GetIndexes(surfaceTool, verticeIndex);
                        Face completeFace = faces[faceKey];
                        surfaceTool.AddVertex(completeFace.vector3s[0]);
                        surfaceTool.AddVertex(completeFace.vector3s[1]);
                        surfaceTool.AddVertex(completeFace.vector3s[2]);
                        surfaceTool.AddVertex(completeFace.vector3s[2]);
                        surfaceTool.AddVertex(completeFace.vector3s[3]);
                        surfaceTool.AddVertex(completeFace.vector3s[0]);
                        verticeIndex += 4;
                        sector[key].Remove(faceKey);
                    }
                    faces.Clear();
                }
            }

            surfaceTool.GenerateNormals();

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
        
        
        Mesh mesh = surfaceTool.Commit();
        MeshInstance meshInstance = new MeshInstance();
        meshInstance.Name = "chunk:" + chunk.x + "," + chunk.y + "," + chunk.z;
        meshInstance.Translate(new Vector3(chunk.x, chunk.y, chunk.z));
        meshInstance.Mesh = mesh;
      //  meshInstance.MaterialOverride = material;
        instances.Add(meshInstance);
        surfaceTool.Clear();
    }
    }

        /*public void setTextureCoords(List<Face> faces, SurfaceTool surfaceTool, int side) {
        foreach (Face completeFace in faces) {
            switch (side) {
                case 0:
                case 1:
                    surfaceTool.setTextureCoords(completeFace.getVector3fs()[0].z * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[0].y * 2048f / completeFace.getObject().getTexture().getHeight(), 0);
                    completeFace.setTextureCoords(completeFace.getVector3fs()[0].z * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[2].y * 2048f / completeFace.getObject().getTexture().getHeight(), 1);
                    completeFace.setTextureCoords(completeFace.getVector3fs()[2].z * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[2].y * 2048f / completeFace.getObject().getTexture().getHeight(), 2);
                    completeFace.setTextureCoords(completeFace.getVector3fs()[2].z * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[0].y * 2048f / completeFace.getObject().getTexture().getHeight(), 3);
                    break;

                case 2:
                case 3:
                    completeFace.setTextureCoords(completeFace.getVector3fs()[0].x * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[0].z * 2048f / completeFace.getObject().getTexture().getHeight(), 0);
                    completeFace.setTextureCoords(completeFace.getVector3fs()[0].x * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[2].z * 2048f / completeFace.getObject().getTexture().getHeight(), 1);
                    completeFace.setTextureCoords(completeFace.getVector3fs()[2].x * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[2].z * 2048f / completeFace.getObject().getTexture().getHeight(), 2);
                    completeFace.setTextureCoords(completeFace.getVector3fs()[2].x * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[0].z * 2048f / completeFace.getObject().getTexture().getHeight(), 3);
                    break;

                case 4:
                case 5:
                    completeFace.setTextureCoords(completeFace.getVector3fs()[0].x * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[0].y * 2048f / completeFace.getObject().getTexture().getWidth(), 0);
                    completeFace.setTextureCoords(completeFace.getVector3fs()[0].x * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[2].y * 2048f / completeFace.getObject().getTexture().getWidth(), 1);
                    completeFace.setTextureCoords(completeFace.getVector3fs()[2].x * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[2].y * 2048f / completeFace.getObject().getTexture().getWidth(), 2);
                    completeFace.setTextureCoords(completeFace.getVector3fs()[2].x * 2048f / completeFace.getObject().getTexture().getWidth(), completeFace.getVector3fs()[0].y * 2048f / completeFace.getObject().getTexture().getWidth(), 3);
                    break;
            }
        }
    }*/

    private void JoinReversed(Dictionary<int, Face> faces, int index, int side) {
        int neighbor = 64;
        switch (side) {
            case 2:
            case 3:
                neighbor = 4096;
                break;
        }

        if (faces.ContainsKey(index - neighbor)) {
            return;
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

    private void GetIndexes(SurfaceTool surfaceTool, int verticeIndex) {
        surfaceTool.AddIndex(verticeIndex);
        surfaceTool.AddIndex(verticeIndex + 2);
        surfaceTool.AddIndex(verticeIndex + 3);
        surfaceTool.AddIndex(verticeIndex + 2);
        surfaceTool.AddIndex(verticeIndex);
        surfaceTool.AddIndex(verticeIndex + 1);
    }
}