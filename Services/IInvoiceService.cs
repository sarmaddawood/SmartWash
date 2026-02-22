using QuestPDF.Infrastructure;

namespace SmartWash.Services
{
    public interface IInvoiceService
    {
        byte[] GenerateInvoicePdf(Models.LaundryOrder order);
    }
}
