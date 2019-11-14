using Godot;
using System;

public struct TerraObject
{
    public static Builder builder(){
        return new Builder();
    }

    public int worldID{get; set;}
    public string fullName{get; set;}
    public string name{get; set;}

    public TerraModule mod{get; set;}
    public Texture texture{get; set;}
    public TerraMesh mesh{get; set;}

    public class Builder{
    private TerraObject terraObject;
    public Builder(){
        terraObject = new TerraObject();
    }

    public Builder module(TerraModule mod){
        terraObject.mod = mod;
        return this;
    }

    public Builder name(String name){
        terraObject.name = name;
        return this;
    }

    public Builder model(TerraMesh mesh){
        terraObject.mesh = mesh;
        return this;
    }

    public Builder texture (Texture texture){
        terraObject.texture = texture;
        return this;
    }

    public TerraObject build(){
        return terraObject;
    }
}
}
