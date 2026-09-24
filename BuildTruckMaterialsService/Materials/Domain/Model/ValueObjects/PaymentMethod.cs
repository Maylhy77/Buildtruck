using System;
using System.Collections.Generic;

namespace BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects
{
    public record PaymentMethod
    {
        public string Value { get; init; }

        private static readonly HashSet<string> ValidMethods = new()
        {
            "CASH",
            "CREDIT",
            "TRANSFER",
            "CHECK"
        };

        public PaymentMethod(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Payment method cannot be null or empty", nameof(value));

            var normalizedValue = value.Trim().ToUpper();

            if (!ValidMethods.Contains(normalizedValue))
                throw new ArgumentException($"Invalid payment method: {value}. Valid methods are: {string.Join(", ", ValidMethods)}", nameof(value));

            Value = normalizedValue;
        }

        public static implicit operator string(PaymentMethod paymentMethod) => paymentMethod.Value;
        public static implicit operator PaymentMethod(string value) => new(value);

        public override string ToString() =>
            Value switch
            {
                "CASH" => "Efectivo",
                "CREDIT" => "Crédito",
                "TRANSFER" => "Transferencia",
                "CHECK" => "Cheque",
                _ => Value
            };

        // Métodos de pago predefinidos
        public static PaymentMethod Cash => new("CASH");
        public static PaymentMethod Credit => new("CREDIT");
        public static PaymentMethod Transfer => new("TRANSFER");
        public static PaymentMethod Check => new("CHECK");

        public bool IsCash => Value == "CASH";
        public bool IsCredit => Value == "CREDIT";
    }
}