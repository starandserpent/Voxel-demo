using Godot;
using System;

public class Weltschmerz
{
    private Noise noise;
    public Weltschmerz(){
        noise = new Noise(1234, null);
    }
    public double getElevation(int posX, int posY) {
        return noise.getNoise(posX, posY);
    }
}
