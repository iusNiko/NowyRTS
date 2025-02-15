using Godot;
using System;
using System.Linq;

public partial class Augment
{
    public int Team = 1;
    public string[] RequiredFlags = Array.Empty<string>();
    public PropertyModifier[] PropertyModifiers = Array.Empty<PropertyModifier>();
    public Augment(int team = 1) {
        Team = team;
        OneTimeSetup();
    }

    public virtual void OneTimeSetup() {
        foreach(Unit unit in World.Instance.Units) {
            if(unit.Team == Team && unit.HasAllFlags(RequiredFlags)) {
                foreach(PropertyModifier property in PropertyModifiers) {
                    unit.PropertyModifiers = unit.PropertyModifiers.Append(property).ToArray();
                }
            }
        }
    }

    public virtual void OnUnitSpawn(Unit unit) {
        if(unit.Team == Team && unit.HasAllFlags(RequiredFlags)) {
            foreach(PropertyModifier property in PropertyModifiers) {
                unit.PropertyModifiers = unit.PropertyModifiers.Append(property).ToArray();
            }
        }
    }
}
