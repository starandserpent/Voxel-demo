using Godot;

public class PrimitiveResources
{
    public static void register(TerraModule module){
        Texture nativeTexture = (Texture)GD.Load("res://assets/textures/NorthenForestDirt256px.png");
        module.newMaterial().name("dirt").texture(nativeTexture);
        
        nativeTexture = (Texture)GD.Load("res://assets/textures/NorthenForestGrass256px.png");
        module.newMaterial().name("grass").texture(nativeTexture);
    }
}
