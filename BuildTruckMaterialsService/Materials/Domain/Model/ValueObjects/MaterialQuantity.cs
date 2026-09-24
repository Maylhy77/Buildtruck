using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects
{
    public record MaterialQuantity
    {
        public decimal Value { get; init; }

        public MaterialQuantity(decimal value)
        {
            if (value < 0)
                throw new ArgumentException("Material quantity cannot be negative", nameof(value));
            
            if (value > 999999.99m)
                throw new ArgumentException("Material quantity cannot exceed 999,999.99", nameof(value));
            
            Value = Math.Round(value, 2);
        }

        public static implicit operator decimal(MaterialQuantity quantity) => quantity.Value;
        public static implicit operator MaterialQuantity(decimal value) => new(value);

        public MaterialQuantity Add(MaterialQuantity other) => new(Value + other.Value);
        public MaterialQuantity Subtract(MaterialQuantity other) => new(Value - other.Value);
        public MaterialQuantity Multiply(decimal factor) => new(Value * factor);

        public bool IsZero => Value == 0;
        public bool IsPositive => Value > 0;

        public override string ToString() => Value.ToString("N2");
    }
}