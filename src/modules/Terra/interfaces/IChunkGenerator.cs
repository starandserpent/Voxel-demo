public interface IChunkGenerator {
    Chunk getChunk(float posX, float posY, float posZ);

    void setMaterials(Registry reg);
}