using System.Collections.Generic;
using Godot;

public class Registry
{
    private volatile List<TerraObject> objects;
    private Dictionary<string, TerraObject> nameToObject;

    public Registry()
    {
        objects = new List<TerraObject>();
        nameToObject = new Dictionary<string, TerraObject>();
        RegisterDefaultObjects();
    }

    private void RegisterDefaultObjects()
    {
        TerraObject air = new TerraObject("air", null);
        RegisterObject(air);
    }

    public void RegisterObject(TerraObject terraObject)
    {
        objects.Add(terraObject);
        int worldID = objects.IndexOf(terraObject);

        terraObject.worldID = worldID;

        string fullName = terraObject.name;
        terraObject.fullName = fullName;
        nameToObject.Add(fullName, terraObject);

        GD.Print(terraObject.fullName);
    }

    public TerraObject SelectByName(string fullName)
    {
        return nameToObject[fullName];
    }

    public TerraObject SelectByID(int id)
    {
        return objects[id];
    }

    public List<TerraObject> GetAllMaterials()
    {
        return objects;
    }
}