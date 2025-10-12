using Godot;
using System;

[GlobalClass]
public partial class Dice : Resource
{
    [Export]
    int TotalDice = 0;

    [Export]
    int DiceType = 0;

    RandomNumberGenerator m_RandomNumberGenerator = new RandomNumberGenerator();

    public Dice(string dice)
    {
        var split = dice.Split('d');
        TotalDice = int.Parse(split[0]);
        DiceType = int.Parse(split[1]);
    }

    public int Roll(int mod = 0)
    {
        int total = mod;

        for (int i = 0; i < TotalDice; i++)
            total += m_RandomNumberGenerator.RandiRange(1, DiceType);

        return total;
    }
}

public static class DiceFactory
{
    public static Dice d2 = new Dice("1d2");
    public static Dice d4 = new Dice("1d4");
    public static Dice d6 = new Dice("1d6");
    public static Dice d8 = new Dice("1d8");
    public static Dice d12 = new Dice("1d12");
    public static Dice d20 = new Dice("1d20");
}
