using Godot;
using System;

[GlobalClass]
public partial class Buff : Resource
{
    public string BuffName;
    public virtual string Description { get; }
    public Stats StatChanges;
    public float Duration; // Duration in seconds, 0 for permanent

    public virtual void Setup(Character character)
    {
        character.OnAttack += OnAttack;
    }

    public virtual void Teardown(Character character)
    {
        character.OnAttack -= OnAttack;
    }

    protected virtual void OnAttack(AttackData attackData) { }
}
