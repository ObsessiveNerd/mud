using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Class : Resource
{
    [Export]
    public virtual string ClassName { get; set; }

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; }

    [Export]
    public Stats BaseStats { get; set; }

    [Export]
    public Array<Buff> StartingBuffs;

    public Class() { }

    public Class(Class _c)
    {
        ClassName = _c.ClassName;
        Description = _c.Description;
        BaseStats = new Stats(_c.BaseStats);
        if (_c.StartingBuffs != null)
            StartingBuffs = new Array<Buff>(_c.StartingBuffs);
    }

    public string GetFullDescription()
    {
        string desc = Description + "\n";
        if(StartingBuffs != null && StartingBuffs.Count > 0)
        {
            desc += "Attributes:\n";
            foreach(var buff in StartingBuffs)
                desc += $"\t{buff.Description}\n";
        }
        return desc;
    }
}
