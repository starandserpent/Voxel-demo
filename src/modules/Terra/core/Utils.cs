using System;

public class Utils
{
    public static double calculateLayers(uint size){
        return Math.Log(size, 8);
    }
}