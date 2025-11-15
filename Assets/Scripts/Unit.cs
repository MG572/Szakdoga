using UnityEngine;

public class Unit
{
    public UnitType Type;
    public int Size;
    public Unit(UnitType type, int size)
    {
        Type = type;
        Size = size;
    }
    public Unit(UnitType type)
    {
        Type = type;
        Size = type.DefaultSize;
    }
}