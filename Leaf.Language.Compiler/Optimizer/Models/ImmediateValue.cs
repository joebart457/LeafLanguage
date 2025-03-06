
namespace Leaf.Language.Compiler.Optimizer.Models;


internal class ImmediateValue
{
    public int? IntValue { get; set; }
    public string? AddressValue { get; set; }
    public float? FloatValue { get; set; }
    public byte? ByteValue { get; set; }

    public bool IsInt => IntValue != null;
    public bool IsAddress => AddressValue != null;
    public bool IsFloat => FloatValue != null;
    public bool IsByte => ByteValue != null;


    public static ImmediateValue Create(int value) => new() { IntValue = value };
    public static ImmediateValue Create(string value) => new() { AddressValue = value };
    public static ImmediateValue Create(float value) => new() { FloatValue = value };
    public static ImmediateValue Create(byte value) => new() { ByteValue = value };

    public override int GetHashCode()
    {
        return IntValue?.GetHashCode() ?? 0;
    }

    public override bool Equals(object? obj)
    {
        if (obj is ImmediateValue imm)
        {
            return IntValue == imm.IntValue
                && AddressValue == imm.AddressValue
                && FloatValue == imm.FloatValue
                && ByteValue == imm.ByteValue;
        }
        return false;
    }
}