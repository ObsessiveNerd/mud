using Godot;
using Godot.Collections;
using System;

public partial class AttackData : Node
{
    public Weapon Weapon;
    public int Damage;
    public int RollToHit;
}

[GlobalClass]
public partial class Character : Resource
{
    [Export] public string Name;
    [Export] public Class CharacterClass;
    [Export] public int Level;
    [Export] public int Experience;
    [Export] public int ExperienceToNextLevel;
    [Export] public Stats Stats;
    [Export] public Array<Buff> Buffs = new Array<Buff>();

    [Signal]
    public delegate void OnAttackEventHandler(AttackData attackData);

    public Character()
    {
        Level = 1;
        ExperienceToNextLevel = (int)Mathf.Pow(Level, 2) * 10;
    }

    public void SetClass(Class _c)
    {
        Stats = _c.BaseStats;
        CharacterClass = new Class(_c);
        if (_c.StartingBuffs != null)
        {
            foreach (Buff buff in _c.StartingBuffs)
            {
                Buffs.Add(buff);
                buff.Setup(this);
            }
        }
    }
    
    public Character(string _name, int _level, int _exp, Stats _stats, Array<Buff> _activeBuffs)
    {
        Name = _name;
        Level = _level;
        Experience = _exp;
        ExperienceToNextLevel = (int)Mathf.Pow(Level, 2) * 10;
        Stats = _stats;
        Buffs = new Array<Buff>(_activeBuffs);

        foreach (Buff buff in Buffs)
            buff.Setup(this);
    }

    public override string ToString()
    {
        string result = Name + "\n";
        result += CharacterClass.ClassName + "\n";
        result += $"Level: {Level}\n";
        result += $"Experience: {Experience}/{ExperienceToNextLevel}\n";
        result += $"{Stats.ToString()}";
        result += "Attributes:\n";
        foreach (var buff in Buffs)
            result += $"\t{buff.Description}\n";
        return result;
    }
}
