using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects
{
    public record MaterialType
    {
        public string Value { get; init; }

        // Tipos predefinidos (para el dropdown del frontend)
        private static readonly HashSet<string> PredefinedTypes = new()
        {
            "CEMENTO",
            "ACERO", 
            "PINTURA",
            "HERRAMIENTA",
            "LIMPIEZA",
            "OTRO"
        };

        public MaterialType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Material type cannot be null or empty", nameof(value));

            // Normalizar el valor
            var normalized = value.Trim().ToUpper();
            
            // Validar longitud para tipos personalizados
            if (normalized.Length > 50)
                throw new ArgumentException("Material type cannot exceed 50 characters", nameof(value));
                
            // Permitir caracteres válidos para tipos personalizados
            if (!IsValidCustomType(normalized))
                throw new ArgumentException("Material type contains invalid characters. Only letters, numbers, spaces and basic punctuation allowed", nameof(value));

            Value = normalized;
        }

        private static bool IsValidCustomType(string value)
        {
            // Permitir letras, números, espacios, guiones y algunos caracteres especiales
            return value.All(c => char.IsLetterOrDigit(c) || 
                                 char.IsWhiteSpace(c) || 
                                 c == '-' || 
                                 c == '_' || 
                                 c == '.' ||
                                 c == '/' ||
                                 c == '(' ||
                                 c == ')');
        }

        public static implicit operator string(MaterialType materialType) => materialType.Value;
        public static implicit operator MaterialType(string value) => new(value);

        public override string ToString() =>
            Value switch
            {
                "CEMENTO" => "Cemento",
                "ACERO" => "Acero", 
                "PINTURA" => "Pintura",
                "HERRAMIENTA" => "Herramienta",
                "LIMPIEZA" => "Limpieza",
                "OTRO" => "Otro",
                _ => FormatCustomType(Value) // Para tipos personalizados
            };

        private static string FormatCustomType(string value)
        {
            // Convertir de MAYÚSCULAS a formato título para mostrar
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(value.ToLower().Replace('_', ' '));
        }

        // Métodos de conveniencia para tipos predefinidos
        public static MaterialType Cement => new("CEMENTO");
        public static MaterialType Steel => new("ACERO");
        public static MaterialType Paint => new("PINTURA");
        public static MaterialType Tool => new("HERRAMIENTA");
        public static MaterialType Cleaning => new("LIMPIEZA");
        public static MaterialType Other => new("OTRO");

        // Método para verificar si es un tipo predefinido
        public bool IsPredefined => PredefinedTypes.Contains(Value);

        // Método para obtener todos los tipos predefinidos (útil para el frontend)
        public static IEnumerable<string> GetPredefinedTypes() => PredefinedTypes.AsEnumerable();

        // Método para crear un tipo personalizado de forma explícita
        public static MaterialType CreateCustom(string customTypeName)
        {
            if (string.IsNullOrWhiteSpace(customTypeName))
                throw new ArgumentException("Custom type name cannot be empty");
                
            return new MaterialType(customTypeName);
        }
    }
}