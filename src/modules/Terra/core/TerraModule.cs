using System.Collections.Generic;

public class TerraModule
{
    public string uniqueID{get; set;}

    private bool registred = false;

    private List<TerraObject> textures;

    public TerraModule(string id){
        this.uniqueID = id;
        this.textures = new List<TerraObject>();
    }
    public TerraObject.Builder newMaterial(){
        TerraObject.Builder builder = TerraObject.builder().module(this);
        textures.Add(builder.build());
        return builder;
    }

    public void RegisterObjects(Registry reg){
        foreach(TerraObject terraObject in textures){
            reg.RegisterObject(terraObject, this);
        }
    }
}
