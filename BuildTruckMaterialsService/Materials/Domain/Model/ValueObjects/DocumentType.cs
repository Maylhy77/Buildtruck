using System;
using System.Collections.Generic;

namespace BuildTruckMaterialsService.Materials.Domain.Model.ValueObjects
{
    public record DocumentType
    {
        public string Value { get; init; }

        private static readonly HashSet<string> ValidTypes = new()
        {
            "INVOICE",
            "RECEIPT",
            "GUIDE",
            "ORDER_NOTE"
        };

        public DocumentType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Document type cannot be null or empty", nameof(value));

            var normalizedValue = value.Trim().ToUpper().Replace(" ", "_");

            if (!ValidTypes.Contains(normalizedValue))
                throw new ArgumentException($"Invalid document type: {value}. Valid types are: {string.Join(", ", ValidTypes)}", nameof(value));

            Value = normalizedValue;
        }

        public static implicit operator string(DocumentType documentType) => documentType.Value;
        public static implicit operator DocumentType(string value) => new(value);

        public override string ToString() =>
            Value switch
            {
                "INVOICE" => "Factura",
                "RECEIPT" => "Boleta",
                "GUIDE" => "Guía de Remisión",
                "ORDER_NOTE" => "Nota de Pedido",
                _ => Value
            };

        public static DocumentType Invoice => new("INVOICE");
        public static DocumentType Receipt => new("RECEIPT");
        public static DocumentType Guide => new("GUIDE");
        public static DocumentType OrderNote => new("ORDER_NOTE");

        public bool IsInvoice => Value == "INVOICE";
        public bool IsReceipt => Value == "RECEIPT";
    }
}