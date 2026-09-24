using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects
{
    public record MaterialUnit
    {
        public string Value { get; init; }

        public MaterialUnit(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Material unit cannot be null or empty", nameof(value));
            
            if (value.Length > 20)
                throw new ArgumentException("Material unit cannot exceed 20 characters", nameof(value));
            
            Value = value.Trim().ToUpper();
        }

        public static implicit operator string(MaterialUnit materialUnit) => materialUnit.Value;
        public static implicit operator MaterialUnit(string value) => new(value);

        public override string ToString() => Value;

        // Unidades comunes predefinidas
        public static MaterialUnit Kilogram => new("KG");
        public static MaterialUnit Gram => new("G");
        public static MaterialUnit Meter => new("M");
        public static MaterialUnit Centimeter => new("CM");
        public static MaterialUnit SquareMeter => new("M2");
        public static MaterialUnit CubicMeter => new("M3");
        public static MaterialUnit Liter => new("L");
        public static MaterialUnit Milliliter => new("ML");
        public static MaterialUnit Unit => new("UND");
        public static MaterialUnit Box => new("CAJA");
        public static MaterialUnit Bag => new("SACO");
        public static MaterialUnit Roll => new("ROLLO");
        public static MaterialUnit Gallon => new("GAL");
        public static MaterialUnit Ton => new("TON");
    }
}