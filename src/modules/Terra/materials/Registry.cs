using System.Threading.Tasks.Dataflow;
using System.Collections.Generic;
using Godot;

public class Registry
{
    private List<TerraObject> objects;
    private Dictionary<string, TerraObject> nameToObject;

    public Registry(){
        objects = new List<TerraObject>();
        nameToObject = new Dictionary<string, TerraObject>();
        RegisterDefaultObjects();
    }

    private void RegisterDefaultObjects(){
        TerraModule module = new TerraModule("base");
        RegisterObject(module.newMaterial().name("air").build(), module);
    }

    public void RegisterObject(TerraObject terraObject, TerraModule mod){
        string fullName = mod.uniqueID + ":" + terraObject.name;
        terraObject.fullName = fullName;

        objects.Add(terraObject);
        int worldID = objects.IndexOf(terraObject);

        terraObject.worldID = worldID;
        nameToObject.Add(fullName, material);

        GD.Print(terraObject.fullName +":"+worldID);   
    }

    
    public TerraObject SelectByName(string fullName){
        return nameToObject.TryGetValue(fullName);
    }


    public TerraObject SelectByName(TerraModule module, String name){
        return nameToObject.TryGetValue(module.uniqueID + ":" + name);
    }

    public TerraObject SelectByID(int id){
        return objects.Find(id);
    }

    public List<TerraObject> GetAllMaterials(){
        return objects;
    }
}
