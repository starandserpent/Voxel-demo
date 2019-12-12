public class Weltschmerz
{
    private volatile Noise noise;

    public Weltschmerz(int seed, int terrainMP, int avgTerrain, int maxElevation, float frequency)
    {
        noise = new Noise(seed, terrainMP, avgTerrain, maxElevation, frequency, null);
    }

    public double GetElevation(int posX, int posY)
    {
        return noise.getNoise(posX, posY);
    }

    public int GetMaxElevation(){
        return noise.GetMaxElevation();
    }
}