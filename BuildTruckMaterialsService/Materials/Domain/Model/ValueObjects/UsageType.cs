using System;
using System.Collections.Generic;

namespace BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects
{
    public record UsageType
    {
        public string Value { get; init; }

        private static readonly HashSet<string> ValidTypes = new()
        {
            "CONSTRUCCION",
            "MANTENIMIENTO",
            "REPARACION",
            "INSTALACION",
            "ACABADOS",
            "ESTRUCTURAL",
            "SANITARIO",
            "ELECTRICO",
            "HERRAMIENTAS",
            "LIMPIEZA"
        };

        public UsageType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Usage type cannot be null or empty", nameof(value));

            var normalizedValue = value.Trim().ToUpper();

            if (!ValidTypes.Contains(normalizedValue))
                throw new ArgumentException($"Invalid usage type: {value}. Valid types are: {string.Join(", ", ValidTypes)}", nameof(value));

            Value = normalizedValue;
        }

        public static implicit operator string(UsageType usageType) => usageType.Value;
        public static implicit operator UsageType(string value) => new(value);

        public override string ToString() => Value switch
        {
            "CONSTRUCCION" => "Construcción",
            "MANTENIMIENTO" => "Mantenimiento",
            "REPARACION" => "Reparación",
            "INSTALACION" => "Instalación",
            "ACABADOS" => "Acabados",
            "ESTRUCTURAL" => "Estructural",
            "SANITARIO" => "Sanitario",
            "ELECTRICO" => "Eléctrico",
            "HERRAMIENTAS" => "Herramientas",
            "LIMPIEZA" => "Limpieza",
            _ => Value
        };

        // Tipos de uso predefinidos
        public static UsageType Construction => new("CONSTRUCCION");
        public static UsageType Maintenance => new("MANTENIMIENTO");
        public static UsageType Repair => new("REPARACION");
        public static UsageType Installation => new("INSTALACION");
        public static UsageType Finishing => new("ACABADOS");
        public static UsageType Structural => new("ESTRUCTURAL");
        public static UsageType Sanitary => new("SANITARIO");
        public static UsageType Electrical => new("ELECTRICO");
        public static UsageType Tools => new("HERRAMIENTAS");
        public static UsageType Cleaning => new("LIMPIEZA");
    }
}
