using Godot;
public class Picker
{
    private Terra terra;
    private GameMesher mesher;
    public Picker(Terra terra, GameMesher mesher){
        this.terra = terra;
        this.mesher = mesher;
    }

    public void Pick(Vector3 pos){
        float posX = pos.x;
        float posY = pos.y;
        float posZ = pos.z;
        Chunk chunk = terra.traverseOctree((int) posX/16,(int) posY/16,(int) posZ);

        int posx = (int)((posX - chunk.x) * 4);
        int posy = (int)((posY - chunk.y) * 4);
        int posz = (int)((posZ - chunk.z) * 4);

        chunk.voxels.Span[posx + (posy * 64) + (posz * 64 * 64)] = 0;

        mesher.MeshChunk(chunk, false);
    }

    public void Place(){
        
    }
}