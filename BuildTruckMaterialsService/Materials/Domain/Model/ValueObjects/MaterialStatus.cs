using System;
using System.Collections.Generic;

namespace BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects
{
    public record MaterialStatus
    {
        public string Value { get; init; }

        private static readonly HashSet<string> ValidStatuses = new()
        {
            "PENDING",
            "CONFIRMED",
            "CANCELLED",
            "IN_PROCESS",
            "COMPLETED"
        };

        public MaterialStatus(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Material status cannot be null or empty", nameof(value));

            var normalizedValue = value.Trim().ToUpper();

            if (!ValidStatuses.Contains(normalizedValue))
                throw new ArgumentException($"Invalid material status: {value}. Valid statuses are: {string.Join(", ", ValidStatuses)}", nameof(value));

            Value = normalizedValue;
        }

        public static implicit operator string(MaterialStatus status) => status.Value;
        public static implicit operator MaterialStatus(string value) => new(value);

        public override string ToString() => Value switch
        {
            "PENDING" => "Pendiente",
            "CONFIRMED" => "Confirmado",
            "CANCELLED" => "Cancelado",
            "IN_PROCESS" => "En proceso",
            "COMPLETED" => "Completado",
            _ => Value
        };

        // Estados predefinidos
        public static MaterialStatus Pending => new("PENDING");
        public static MaterialStatus Confirmed => new("CONFIRMED");
        public static MaterialStatus Cancelled => new("CANCELLED");
        public static MaterialStatus InProcess => new("IN_PROCESS");
        public static MaterialStatus Completed => new("COMPLETED");

        // Métodos de conveniencia
        public bool IsPending => Value == "PENDING";
        public bool IsConfirmed => Value == "CONFIRMED";
        public bool IsCancelled => Value == "CANCELLED";
        public bool IsInProcess => Value == "IN_PROCESS";
        public bool IsCompleted => Value == "COMPLETED";

        // Transiciones válidas
        public bool CanTransitionTo(MaterialStatus newStatus)
        {
            return Value switch
            {
                "PENDING" => newStatus.Value is "CONFIRMED" or "CANCELLED",
                "CONFIRMED" => newStatus.Value is "IN_PROCESS" or "CANCELLED",
                "IN_PROCESS" => newStatus.Value is "COMPLETED" or "CANCELLED",
                "COMPLETED" => false,
                "CANCELLED" => newStatus.Value == "PENDING",
                _ => false
            };
        }
    }
}
