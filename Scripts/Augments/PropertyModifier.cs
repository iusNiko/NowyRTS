using System;

public class PropertyModifier
{
    public string Name;
    public float PercentageModifier;
    public float FlatModifier;
    public PropertyModifier(string name, float percentageModifier = 1f, float flatModifier = 0) {
        Name = name;
        PercentageModifier = percentageModifier;
        FlatModifier = flatModifier;
    }
}