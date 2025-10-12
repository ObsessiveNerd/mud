using Godot;
using System;

[GlobalClass]
public partial class WeaponSpecificBuff : Buff
{
    [Export]
    public int AttackBonus = 1;

    [Export]
    WeaponType weaponType = WeaponType.Unarmed;

    public WeaponSpecificBuff()
    {
        BuffName = "Unarmed Combatant";
        StatChanges = new Stats();
        Duration = 0; // Permanent
    }

    public override string Description
    {
        get
        {
            return $"+{AttackBonus} to hit when using {weaponType}";
        }
    }

    protected override void OnAttack(AttackData attackData)
    {
        if(attackData.Weapon.Type == weaponType)
        {
            attackData.RollToHit += AttackBonus;
        }
    }
}
