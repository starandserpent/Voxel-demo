using Godot;
using System;

public class Noise
{
    private static readonly bool USE_EARTH = false;
    private static readonly int DIFFERENCE = 500;

    private FastNoise noise;
    private int worldWidth = 1000;
    private int worldHeight = 1000;
    private int samples;
    private Image earth;

    public Noise(int seed, Image earth){
        this.earth = earth;
        noise = new FastNoise(seed);
        noise.SetNoiseType(FastNoise.NoiseType.Simplex);
        noise.SetFrequency(0.45F);
    }

    public double getNoise(int x, int y){
        if(!USE_EARTH) {
            float s = x / (float) worldWidth;
            float t = y / (float) worldHeight;
            double nx = Math.Cos(s * 2 * Math.PI) * 1.0 / (2 * Math.PI);
            double ny = Math.Cos(t * 2 * Math.PI) * 1.0 / (2 * Math.PI);
            double nz = Math.Sin(s * 2 * Math.PI) * 1.0 / (2 * Math.PI);
            double nw = Math.Sin(t * 2 * Math.PI) * 1.0 / (2 * Math.PI);
            return Math.Max((noise.GetSimplex((float)nx, (float)ny, (float)nz, (float)nw) * 100) + 10, 1);
        }else{
            //return new Color(earth(x, y)).getRed();
            return 0.0;
        }
    }
}
