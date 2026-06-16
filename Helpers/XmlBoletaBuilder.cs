using System.Xml.Linq;
using HotelGenericoApi.Helpers;

namespace HotelGenericoApi.Helpers;

public static class XmlBoletaBuilder
{
    private static readonly XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
    private static readonly XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
    private static readonly XNamespace ds = "http://www.w3.org/2000/09/xmldsig#";
    private static readonly XNamespace sac = "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1";
    private static readonly XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

    public static XDocument BuildBoleta(
        string serieCorrelativo,
        string tipoComprobante,
        DateTime fechaEmision,
        decimal total,
        decimal igv,
        decimal baseImponible,
        string clienteTipoDoc,
        string clienteDocNum,
        string clienteNombre,
        string emisorRuc,
        string emisorRazonSocial,
        string emisorNombreComercial,
        string emisorDireccion,
        string emisorUbigeo,
        string emisorDepartamento,
        string emisorProvincia,
        string emisorDistrito,
        string emisorUrbanizacion,
        string moneda,
        List<(string descripcion, int cantidad, decimal precioUnitario, decimal subtotal)> items,
        bool aplicarLeyendaAmazonia = false,
        string? leyendaAmazonia = null)
    {
        string invoiceTypeCode = tipoComprobante == "01" ? "01" : "03";
        string tipoDocCliente = clienteTipoDoc == "6" ? "6" : "1";

        var montoLetras = MontoEnLetrasHelper.Convertir(total);
        int lineCount = items.Count;

        var notes = new List<XElement>
        {
            new XElement(cbc + "Note",
                new XAttribute("languageLocaleID", "1000"),
                montoLetras
            )
        };

        if (aplicarLeyendaAmazonia)
        {
            string textoLeyenda = !string.IsNullOrEmpty(leyendaAmazonia)
                ? leyendaAmazonia
                : "BIENES TRANSFERIDOS/SERVICIOS PRESTADOS EN LA REGION DE SELVA PARA SER CONSUMIDOS EN LA MISMA";
            notes.Add(new XElement(cbc + "Note",
                new XAttribute("languageLocaleID", "2002"),
                textoLeyenda
            ));
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(cac + "Invoice",
                new XAttribute(XNamespace.Xmlns + "cac", cac.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "cbc", cbc.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "ext", ext.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "ds", ds.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "sac", sac.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                new XAttribute("xmlns", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"),

                new XElement(ext + "UBLExtensions",
                    new XElement(ext + "UBLExtension",
                        new XElement(ext + "ExtensionContent",
                            new XComment(" Firma digital pendiente ")
                        )
                    )
                ),

                new XElement(cbc + "UBLVersionID", "2.1"),
                new XElement(cbc + "CustomizationID", "2.0"),
                new XElement(cbc + "ID", serieCorrelativo),
                new XElement(cbc + "IssueDate", fechaEmision.ToString("yyyy-MM-dd")),
                new XElement(cbc + "IssueTime", fechaEmision.ToString("HH:mm:ss")),
                new XElement(cbc + "InvoiceTypeCode",
                    new XAttribute("listID", "0101"),
                    new XAttribute("listAgencyName", "PE:SUNAT"),
                    new XAttribute("listName", "Tipo de Documento"),
                    new XAttribute("listURI", "urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01"),
                    invoiceTypeCode
                ),
                notes,
                new XElement(cbc + "DocumentCurrencyCode",
                    new XAttribute("listID", "ISO 4217 Alpha"),
                    new XAttribute("listAgencyName", "United Nations Economic Commission for Europe"),
                    new XAttribute("listName", "Currency"),
                    moneda
                ),
                new XElement(cbc + "LineCountNumeric", lineCount),

                new XElement(cac + "Signature",
                    new XElement(cbc + "ID", "IDSignSIGHRN"),
                    new XElement(cac + "SignatoryParty",
                        new XElement(cac + "PartyIdentification",
                            new XElement(cbc + "ID", emisorRuc)
                        ),
                        new XElement(cac + "PartyName",
                            new XElement(cbc + "Name", emisorRazonSocial)
                        )
                    ),
                    new XElement(cac + "DigitalSignatureAttachment",
                        new XElement(cac + "ExternalReference",
                            new XElement(cbc + "URI", "#signature")
                        )
                    )
                ),

                new XElement(cac + "AccountingSupplierParty",
                    new XElement(cbc + "CustomerAssignedAccountID", emisorRuc),
                    new XElement(cbc + "AdditionalAccountID", "6"),
                    new XElement(cac + "Party",
                        new XElement(cac + "PartyName",
                            new XElement(cbc + "Name",
                                !string.IsNullOrEmpty(emisorNombreComercial) ? emisorNombreComercial : emisorRazonSocial
                            )
                        ),
                        new XElement(cac + "PostalAddress",
                            new XElement(cbc + "ID", emisorUbigeo),
                            new XElement(cbc + "StreetName", emisorDireccion),
                            new XElement(cbc + "CitySubdivisionName", emisorUrbanizacion),
                            new XElement(cbc + "CityName", emisorProvincia),
                            new XElement(cbc + "CountrySubentity", emisorDepartamento),
                            new XElement(cbc + "District", emisorDistrito),
                            new XElement(cac + "Country",
                                new XElement(cbc + "IdentificationCode", "PE")
                            )
                        ),
                        new XElement(cac + "PartyLegalEntity",
                            new XElement(cbc + "RegistrationName", emisorRazonSocial)
                        )
                    )
                ),

                new XElement(cac + "AccountingCustomerParty",
                    new XElement(cbc + "CustomerAssignedAccountID", clienteDocNum),
                    new XElement(cbc + "AdditionalAccountID", tipoDocCliente),
                    new XElement(cac + "Party",
                        new XElement(cac + "PartyLegalEntity",
                            new XElement(cbc + "RegistrationName", clienteNombre)
                        )
                    )
                ),

                new XElement(cac + "TaxTotal",
                    new XElement(cbc + "TaxAmount",
                        new XAttribute("currencyID", moneda),
                        igv
                    ),
                    new XElement(cac + "TaxSubtotal",
                        new XElement(cbc + "TaxableAmount",
                            new XAttribute("currencyID", moneda),
                            baseImponible
                        ),
                        new XElement(cbc + "TaxAmount",
                            new XAttribute("currencyID", moneda),
                            igv
                        ),
                        new XElement(cac + "TaxCategory",
                            new XElement(cac + "TaxScheme",
                                new XElement(cbc + "ID", "1000"),
                                new XElement(cbc + "Name", "IGV"),
                                new XElement(cbc + "TaxTypeCode", "VAT")
                            )
                        )
                    )
                ),

                new XElement(cac + "LegalMonetaryTotal",
                    new XElement(cbc + "PayableAmount",
                        new XAttribute("currencyID", moneda),
                        total
                    )
                ),

                ItemsXml(items, moneda)
            )
        );

        return doc;
    }

    private static XElement[] ItemsXml(
        List<(string descripcion, int cantidad, decimal precioUnitario, decimal subtotal)> items,
        string moneda)
    {
        var elements = new List<XElement>();
        int index = 1;

        foreach (var item in items)
        {
            elements.Add(
                new XElement(cac + "InvoiceLine",
                    new XElement(cbc + "ID", index),
                    new XElement(cbc + "InvoicedQuantity",
                        new XAttribute("unitCode", "NIU"),
                        item.cantidad
                    ),
                    new XElement(cbc + "LineExtensionAmount",
                        new XAttribute("currencyID", moneda),
                        item.subtotal
                    ),
                    new XElement(cac + "PricingReference",
                        new XElement(cac + "AlternativeConditionPrice",
                            new XElement(cbc + "PriceAmount",
                                new XAttribute("currencyID", moneda),
                                item.precioUnitario
                            ),
                            new XElement(cbc + "PriceTypeCode", "01")
                        )
                    ),
                    new XElement(cac + "TaxTotal",
                        new XElement(cbc + "TaxAmount",
                            new XAttribute("currencyID", moneda),
                            0.00m
                        ),
                        new XElement(cac + "TaxSubtotal",
                            new XElement(cbc + "TaxableAmount",
                                new XAttribute("currencyID", moneda),
                                item.subtotal
                            ),
                            new XElement(cbc + "TaxAmount",
                                new XAttribute("currencyID", moneda),
                                0.00m
                            ),
                            new XElement(cac + "TaxCategory",
                                new XElement(cbc + "ID", "10"),
                                new XElement(cac + "TaxScheme",
                                    new XElement(cbc + "ID", "1000"),
                                    new XElement(cbc + "Name", "IGV"),
                                    new XElement(cbc + "TaxTypeCode", "VAT")
                                )
                            )
                        )
                    ),
                    new XElement(cac + "Item",
                        new XElement(cbc + "Description", item.descripcion)
                    ),
                    new XElement(cac + "Price",
                        new XElement(cbc + "PriceAmount",
                            new XAttribute("currencyID", moneda),
                            item.precioUnitario
                        )
                    )
                )
            );
            index++;
        }

        return elements.ToArray();
    }
}
