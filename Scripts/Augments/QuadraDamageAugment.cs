using Godot;
using System;

public partial class QuadraDamageAugment : Augment {
    public QuadraDamageAugment(int team = 1) : base(team){
        PropertyModifiers = new PropertyModifier[] {
            new PropertyModifier("AttackDamage", 4f, 0f)
        };
    }
}