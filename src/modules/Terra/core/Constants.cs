using System;

public class Constants
{
    public const int CHUNK_SIZE1D = 32;
    public const int CHUNK_SIZE2D = CHUNK_SIZE1D * CHUNK_SIZE1D;
    public const int CHUNK_SIZE3D = CHUNK_SIZE1D * CHUNK_SIZE1D * CHUNK_SIZE1D;
    public static readonly int CHUNK_EXPONENT = (int) (Math.Log(CHUNK_LENGHT) / Math.Log(2));
    public const float CHUNK_LENGHT = CHUNK_SIZE1D * VOXEL_SIZE;
    public const float VOXEL_SIZE = 0.25f;
}