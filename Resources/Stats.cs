using Godot;
using System;

[GlobalClass]
public partial class Stats : Resource
{
    [Export] public int Might { get; set; }
    [Export] public int Endurance { get; set; }
    [Export] public int Legerity { get; set; }
    [Export] public int Cunning { get; set; }

    public Stats()
    {
        Might = 1;
        Endurance = 1;
        Legerity = 1;
        Cunning = 1;
    }

    public Stats(Stats _s) 
    {
        Might = _s.Might;
        Endurance = _s.Endurance;
        Legerity = _s.Legerity;
        Cunning = _s.Cunning;
    }

    public override string ToString()
    {
        return "Stats:\n" +
                    $"\tMight: {Might}\n" +
                    $"\tEndurance: {Endurance}\n" +
                    $"\tLegerity: {Legerity}\n" +
                    $"\tCunning: {Cunning}\n";
    }
}
