using Godot;
using System;

public enum WeaponType
{
    Unarmed,
    Sword,
    Axe,
    Bow,
    Dagger,
    Staff,
    Mace
}

[GlobalClass]
public partial class Weapon : Resource
{
    [Export]
    public string WeaponName;
    [Export(PropertyHint.MultilineText)]
    public string Description;
    [Export]
    public WeaponType Type;
    [Export]
    public Dice Damage;
}
