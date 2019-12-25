using Godot;

public class PrimitiveResources
{
    public static void register(Registry registry)
    {
        Texture nativeTexture = (Texture) GD.Load("res://assets/textures/NorthenForestDirt256px.png");
        TerraObject dirt = new TerraObject("dirt", nativeTexture);
        registry.RegisterObject(dirt);

        nativeTexture = (Texture) GD.Load("res://assets/textures/NorthenForestGrass256px.png");
        TerraObject grass = new TerraObject("grass", nativeTexture);
        registry.RegisterObject(grass);
    }
}