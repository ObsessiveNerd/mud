using Godot;
using Godot.Collections;
using System;

public partial class Factory : Node
{
    [Export]
    Array<Class> classes;

    static Dictionary<string, Class> classDict = new Dictionary<string, Class>();

    public override void _Ready()
    {
        foreach (var c in classes)
            classDict[c.ClassName] = c;
    }

    public static Class GetClassByName(string className)
    {
        if (classDict.ContainsKey(className))
            return classDict[className];
        return null;
    }

    public static Array<Class> GetAllClasses()
    {
        return new Array<Class>(classDict.Values);
    }
}
