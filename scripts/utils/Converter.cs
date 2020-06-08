using Godot;
public class Converter {
    public static TerraVector3 ConvertVector (Vector3 godotOrigin) {
        return new TerraVector3 (godotOrigin.x, godotOrigin.y, godotOrigin.z);
    }

    public static TerraBasis ConvertBasis (Basis godotBasis) {
        TerraBasis basis = TerraBasis.InitEmpty();
        basis.matrix[0] = new TerraVector3 (godotBasis[0].x, godotBasis[0].y, godotBasis[0].z);
        basis.matrix[1] = new TerraVector3 (godotBasis[1].x, godotBasis[1].y, godotBasis[1].z);
        basis.matrix[2] = new TerraVector3 (godotBasis[2].x, godotBasis[2].y, godotBasis[2].z);
        return basis;
    }
}