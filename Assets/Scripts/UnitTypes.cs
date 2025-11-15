using UnityEngine;

public class UnitType
{
    public string Name;
    public int DefaultSize;
    public int Damage;
    public int MeleeArmor;
    public int RangedArmor;
    public int Speed;

    public UnitType(string name, int defaultSize, int damage, int meleeArmor, int rangedArmor, int speed)
    {
        Name = name;
        DefaultSize = defaultSize;
        Damage = damage;
        MeleeArmor = meleeArmor;
        RangedArmor = rangedArmor;
        Speed = speed;
    }
}