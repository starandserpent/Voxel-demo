using Godot;
using System;

public struct TerraObject
{
    public static Builder builder(){
        return new Builder();
    }

    private int worldID{get; set;}
    private string fullName{get; set;}
    private string name{get; set;}

    private TerraModule mod{get; set;}
    private Texture texture{get; set;}
    private TerraMesh mesh{get; set;}
}

public static class Builder{
    private TerraObject terraObject;
    private Builder(){
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
