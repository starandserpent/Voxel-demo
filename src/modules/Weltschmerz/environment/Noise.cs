using Godot;
using System;

public class Noise
{
    private static readonly bool USE_EARTH = false;
    private static readonly int DIFFERENCE = 500;

    private volatile FastNoise noise;
    private int worldWidth = 1000;
    private int worldHeight = 1000;
    private int samples;
    private Image earth;
    private int terrainMP;
    private int avgTerrain;
    private volatile int maxElevation;
    public Noise(int seed, int terrainMP, int avgTerrain, int maxElevation, float frequency, Image earth)
    {
        this.earth = earth;
        noise = new FastNoise(seed);
        this.terrainMP = terrainMP;
        this.avgTerrain = avgTerrain;
        noise.SetNoiseType(FastNoise.NoiseType.Simplex);
        noise.SetFrequency(frequency);
        this.maxElevation = maxElevation;
    }

    public double getNoise(int x, int y)
    {
        if (!USE_EARTH)
        {
            float s = x / (float) worldWidth;
            float t = y / (float) worldHeight;
            double nx = Math.Cos(s * 2 * Math.PI) * 1.0 / (2 * Math.PI);
            double ny = Math.Cos(t * 2 * Math.PI) * 1.0 / (2 * Math.PI);
            double nz = Math.Sin(s * 2 * Math.PI) * 1.0 / (2 * Math.PI);
            double nw = Math.Sin(t * 2 * Math.PI) * 1.0 / (2 * Math.PI);
            return Math.Min(Math.Max((noise.GetSimplex((float) nx, (float) ny, (float) nz, (float) nw) * terrainMP) + avgTerrain, 1), maxElevation);
        }
        else
        {
            //return new Color(earth(x, y)).getRed();
            return 0.0;
        }
    }

    public int GetMaxElevation(){
        return maxElevation;
    }
}