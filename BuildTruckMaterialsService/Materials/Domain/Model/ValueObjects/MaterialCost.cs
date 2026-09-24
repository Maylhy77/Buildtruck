using System;

namespace BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects
{
    public record MaterialCost
    {
        public decimal Value { get; init; }
        public string Currency { get; init; }

        public MaterialCost(decimal value, string currency = "PEN")
        {
            if (value < 0)
                throw new ArgumentException("Material cost cannot be negative", nameof(value));
            
            if (value > 9999999.99m)
                throw new ArgumentException("Material cost cannot exceed 9,999,999.99", nameof(value));
            
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be null or empty", nameof(currency));
            
            Value = Math.Round(value, 2);
            Currency = currency.ToUpper();
        }

        public static implicit operator decimal(MaterialCost cost) => cost.Value;
        
        public MaterialCost Add(MaterialCost other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot add costs with different currencies");
            
            return new MaterialCost(Value + other.Value, Currency);
        }

        public MaterialCost Multiply(decimal factor) => new(Value * factor, Currency);
        
        public MaterialCost CalculateTotal(MaterialQuantity quantity) => new(Value * quantity.Value, Currency);

        public bool IsZero => Value == 0;

        public override string ToString() => $"{Currency} {Value:N2}";

        // Monedas comunes
        public static MaterialCost InSoles(decimal value) => new(value, "PEN");
        public static MaterialCost InDollars(decimal value) => new(value, "USD");
    }
}