using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartWash.Models;

namespace SmartWash.Services
{
    public class InvoiceService : IInvoiceService
    {
        public byte[] GenerateInvoicePdf(LaundryOrder order)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SmartWash").FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("Professional laundry services you can trust.").FontSize(10).Italic();
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"Invoice #{order.Id.ToString().Substring(0, 8)}").FontSize(16).SemiBold();
                            col.Item().Text($"Date: {order.OrderDate:MMM dd, yyyy}");
                        });
                    });

                    page.Content().PaddingVertical(25).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Customer Details").Bold();
                                c.Item().Text(order.Customer?.FullName ?? "N/A");
                                c.Item().Text(order.Customer?.PhoneNumber ?? "");
                                c.Item().Text(order.Customer?.Address ?? "");
                            });
                            
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Payment Status").Bold();
                                var statusColor = order.Payment?.Status == "Paid" ? Colors.Green.Medium : Colors.Red.Medium;
                                c.Item().Text(order.Payment?.Status ?? "Unpaid").SemiBold().FontColor(statusColor);
                            });
                        });

                        col.Item().PaddingTop(20).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Item / Service");
                                header.Cell().Element(CellStyle).AlignRight().Text("Qty");
                                header.Cell().Element(CellStyle).AlignRight().Text("Price");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            foreach (var item in order.Items)
                            {
                                table.Cell().Element(CellStyle).Text($"{item.ItemType} ({item.ServiceType})");
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"₱{item.Price:N2}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"₱{(item.Quantity * item.Price):N2}");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                }
                            }
                        });

                        col.Item().AlignRight().PaddingTop(10).Text(x =>
                        {
                            x.Span("Total Amount: ").FontSize(14);
                            x.Span($"₱{order.TotalPrice:N2}").FontSize(14).Bold();
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
