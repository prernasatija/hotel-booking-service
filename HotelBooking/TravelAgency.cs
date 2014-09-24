using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HotelBooking
{
    class TravelAgency
    {
        private MultiCellBuffer buffer;
        static Random rng = new Random();
        public void travelAgencyFunc()
        {   //for starting thread
            Int32 p = rng.Next(1, 3);
            HotelSupplier hotelSupplier = HotelSupplier.getSupplier(p-1);//new HotelSupplier();
            hotelSupplier.priceCut += new priceCutEvent(this.roomsOnSale);

            Console.WriteLine(Thread.CurrentThread.Name + " subscribing to " + p);
            hotelSupplier.subscribe(Thread.CurrentThread);
            for (Int32 i = 0; i < 2; i++)
            {
                Int32 p1 = hotelSupplier.getPrice();
                Console.WriteLine(Thread.CurrentThread.Name + ": SUSPENDED");
                Thread.CurrentThread.Suspend();
                Console.WriteLine(Thread.CurrentThread.Name + ": RESUMED");
                Int32 p2 = hotelSupplier.getPrice();
                Console.WriteLine("Travel Agent:: {0} ordering from HS{1}", Thread.CurrentThread.Name, p);
            }
        }
        public void roomsOnSale(Int32 p)
        {  // Event handler
            Console.WriteLine(Thread.CurrentThread.Name + " -> roomsOnSale::");
            Int32 id = Convert.ToInt32(Thread.CurrentThread.Name);
            HotelSupplier supplier = HotelSupplier.getSupplier(id-1);
            supplier.resumeSubscribers();
            //Int32 roomsCount = rng.Next(1, 10);
            //Int32 amount = roomsCount * p;
            //Int32 senderId = 1;// Convert.ToInt32(Thread.CurrentThread.Name);
            //Int32 cardNo = 0;
            
            //for (int i = 0; i < 5; i++)
            //{
            //    cardNo = cardNo * 10 + senderId;
            //}
            //Order order = new Order(senderId, cardNo, 12, amount);
            //String strOrder = Order.encoder(order);
            //buffer.setOneCell(strOrder);
        }
        public void setBuffer(MultiCellBuffer buffer)
        {
            this.buffer = buffer;
        }
    }
}
