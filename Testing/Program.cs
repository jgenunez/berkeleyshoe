using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BerkeleyEntities;
using System.Xml.Serialization;
using System.IO;
using System.Net.NetworkInformation;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {

            Ping ping = new Ping();

            PingReply pingReply = ping.Send("192.168.1.22");
         
        }

        static void CreateOrder(string id)
        {
            Order order = new Order();
            order.id = id;

            OrderAddressInfo address1 = new OrderAddressInfo();
            address1.Address1 = "testing ship";
            address1.Address2 = "testing";
            address1.City = "Narnia";
            address1.Country = "Atlantis";
            address1.Email = "boho@gmail.com";
            address1.Name = new OrderAddressInfoName() { First = "Romulo Bentancourt", Full = "testing", Last = "testing" };
            address1.Phone = "555*666-beast";
            address1.State = "MA";
            address1.type = "ship";
            address1.Zip = 01844;

            OrderAddressInfo address2 = new OrderAddressInfo();
            address2.Address1 = "testing bill";
            address2.Address2 = "testing";
            address2.City = "Narnia";
            address2.Country = "Atlantis";
            address2.Email = "boho@gmail.com";
            address2.Name = new OrderAddressInfoName() { First = "Romulo Bentancourt", Full = "testing", Last = "testing" };
            address2.Phone = "555*666-beast";
            address2.State = "MA";
            address2.type = "bill";
            address2.Zip = 01844;

            order.AddressInfo = new OrderAddressInfo[] { address1, address2 };

            OrderItem item = new OrderItem();
            item.Code = "10061-10-M";
            item.Description = "CASH MONEY HOE";
            item.Id = 6666666;
            item.Quantity = 10;
            item.UnitPrice = 100;

            order.Item = new OrderItem[] { item };

            OrderLine orderLine = new OrderLine();
            orderLine.name = "testing";
            orderLine.type = "type";
            orderLine.Value = 1000;


            order.Total = new OrderLine[] { orderLine };


            StringWriter stringWriter = new StringWriter();

            XmlSerializer serializer = new XmlSerializer(typeof(Order));
            serializer.Serialize(stringWriter, order);


            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                Exchange exchange = new Exchange();
                exchange.Data = stringWriter.ToString();
                exchange.Comment = "testing";
                exchange.DateCreated = DateTime.UtcNow;
                exchange.ProcessorCode = "YahooStore";
                exchange.Status = 0;
                exchange.LastUpdated = DateTime.UtcNow;

                dataContext.Exchanges.AddObject(exchange);

                dataContext.SaveChanges();
            }
        }

        static void TestOrder()
        {
            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
 
            }
        }
    }
}
