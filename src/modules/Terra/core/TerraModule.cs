using System.Linq;
using Godot;
using System;

public class TerraModule
{
    private string uniqueID{get; set;}

    private bool registred = false;

    private List<TerraObject> objects;

    public TerraModule(string id){
        this.uniqueID = id;
        this.textures = new List<TerraObject>();
    }
    public TerraObject.Builder newMaterial(){
        TerraObject.Builder builder = TerraObject.builder().module(this);
        objects.Append(builder.build());
    }

    public void RegisterObjects(Registry reg){
        foreach(TerraObject terraObject in objects){
            reg.RegisterObject(terraObject, this);
        }
    }
}
