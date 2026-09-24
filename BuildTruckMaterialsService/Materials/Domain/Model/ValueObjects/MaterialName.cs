using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects
{
    public record MaterialName
    {
        public string Value { get; init; }

        public MaterialName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Material name cannot be null or empty", nameof(value));
            
            if (value.Length > 100)
                throw new ArgumentException("Material name cannot exceed 100 characters", nameof(value));
            
            Value = value.Trim();
        }

        public static implicit operator string(MaterialName materialName) => materialName.Value;
        public static implicit operator MaterialName(string value) => new(value);

        public override string ToString() => Value;
    }
}