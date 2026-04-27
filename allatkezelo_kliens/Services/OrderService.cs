using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hotcakes.CommerceDTO.v1.Orders;
using Hotcakes.CommerceDTO.v1.Contacts;
using System.ComponentModel;

namespace allatkezelo_kliens.Services
{
    // A Formból áthozott ViewModel
    public class RendelesViewModel
    {
        public string Rendelésszám { get; set; }
        public string Dátum { get; set; }
        public string Vevő_Neve { get; set; }
        public string Végösszeg { get; set; }
        public string Státuszkód { get; set; }
        public string Státusz { get; set; }

        [Browsable(false)]
        public OrderDTO EredetiRendeles { get; set; }
    }

    public interface IOrderService
    {
        int CalculateReleasedInventory(int currentReserved, int canceledQuantity);
        string GetCustomerFullName(AddressDTO billingAddress);
        void GetFirstItemSummary(List<LineItemDTO> items, out string productName, out string sku, out string quantity);
        RendelesViewModel MapToViewModel(OrderDTO order);
    }

    public class OrderService : IOrderService
    {
        // 1. Készlet visszaállításának matematikája (ne menjen mínuszba)
        public int CalculateReleasedInventory(int currentReserved, int canceledQuantity)
        {
            int newReserved = currentReserved - canceledQuantity;
            return newReserved < 0 ? 0 : newReserved;
        }

        // 2. Vevő nevének biztonságos összerakása
        public string GetCustomerFullName(AddressDTO billingAddress)
        {
            if (billingAddress == null || (string.IsNullOrWhiteSpace(billingAddress.FirstName) && string.IsNullOrWhiteSpace(billingAddress.LastName)))
            {
                return "Ismeretlen vevő";
            }
            return $"{billingAddress.FirstName} {billingAddress.LastName}".Trim();
        }

        // 3. Az első termék adatainak biztonságos kinyerése
        public void GetFirstItemSummary(List<LineItemDTO> items, out string productName, out string sku, out string quantity)
        {
            if (items != null && items.Count > 0)
            {
                var firstItem = items[0];
                productName = firstItem.ProductName ?? "Ismeretlen termék";
                sku = firstItem.ProductSku ?? "-";
                quantity = firstItem.Quantity.ToString();
            }
            else
            {
                productName = "Nincs termék a rendelésben";
                sku = "-";
                quantity = "0";
            }
        }

        // 4. A táblázat sorainak (ViewModel) generálása
        public RendelesViewModel MapToViewModel(OrderDTO order)
        {
            return new RendelesViewModel
            {
                Rendelésszám = order.OrderNumber,
                Dátum = order.TimeOfOrderUtc.ToLocalTime().ToString("yyyy. MM. dd. HH:mm"),
                Vevő_Neve = GetCustomerFullName(order.BillingAddress),
                Végösszeg = order.TotalGrand.ToString("C"),
                Státuszkód = order.StatusCode,
                Státusz = order.StatusName,
                EredetiRendeles = order
            };
        }
    }
}
