using Godot;
using System;

public class Weltschmerz
{
    private Noise noise;

    public Weltschmerz(int seed, int terrainMP, int avgTerrain)
    {
        noise = new Noise(seed, terrainMP, avgTerrain, null);
    }

    public double getElevation(int posX, int posY)
    {
        return noise.getNoise(posX, posY);
    }
}