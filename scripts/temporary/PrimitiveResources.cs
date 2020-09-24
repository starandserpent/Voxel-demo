using Godot;

public class PrimitiveResources
{
    public static void Register(Registry registry)
    {
        SpatialMaterial material = new SpatialMaterial();
        Texture nativeTexture = (Texture) GD.Load("res://assets/textures/blocks/NorthenForestDirt256px.png");
        nativeTexture.Flags = 2;
        material.AlbedoTexture = nativeTexture;
        TerraObject dirt = new TerraObject("dirt", material, false);
        registry.RegisterObject(dirt);

        material = new SpatialMaterial();
        nativeTexture = (Texture) GD.Load("res://assets/textures/blocks/NorthenForestGrass256px.png");
        nativeTexture.Flags = 2;
        material.AlbedoTexture = nativeTexture;
        TerraObject grass = new TerraObject("grass", material, true);
        registry.RegisterObject(grass);
    }
}