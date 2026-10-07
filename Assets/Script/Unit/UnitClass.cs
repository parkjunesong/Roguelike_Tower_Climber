public enum UnitClass
{
    None,
    Knight,
    Archer,
    Assassin,
    Caster,
    Shaman
}

public static class UnitClassNames
{
    public static string GetName(UnitClass value) => value switch
    {
        UnitClass.Knight => "나이트",
        UnitClass.Archer => "아처",
        UnitClass.Assassin => "어쌔신",
        UnitClass.Caster => "캐스터",
        UnitClass.Shaman => "샤먼",
        _ => "미분류"
    };
}
