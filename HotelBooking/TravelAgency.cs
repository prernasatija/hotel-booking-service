using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HotelBooking
{
    class TravelAgency
    {
        public static Int32 maxTravelAgents;
        private static MultiCellBuffer buffer;
        private static MultiCellBuffer confirmationBuffer;
        static Random rng = new Random();
        private DateTime startTime;

        public void travelAgencyFunc()
        {   //for starting thread
            try
            {
                Int32 p = rng.Next(1, 3);
                HotelSupplier hotelSupplier = HotelSupplier.getSupplier(p - 1);

                Console.WriteLine("TA:" + Thread.CurrentThread.Name + " subscribing to HS:" + p);
                hotelSupplier.subscribe(Thread.CurrentThread);
                while(hotelSupplier.validOrderCount <= hotelSupplier.maxOrder)
                {
                    Int32 p1 = hotelSupplier.getPrice();
                    //Console.WriteLine("TA:" + Thread.CurrentThread.Name + ": SUSPENDED");
                    Thread.CurrentThread.Suspend();
                    //Console.WriteLine("TA:" + Thread.CurrentThread.Name + ": RESUMED");
                    Int32 p2 = hotelSupplier.getPrice();
                    

                    // TODO: roomsCount depending on change in price.
                    Int32 roomsCount = rng.Next(1, 5);
                    Int32 senderId = Convert.ToInt32(Thread.CurrentThread.Name);
                    BankService bankService = new BankService();
                    Int32 cardNo = bankService.getCreditCard(senderId-1);

                    Order order = new Order(senderId, cardNo, p, roomsCount);
                    String strOrder = Order.encoder(order);
                    startTime = System.DateTime.Now;
                    Console.WriteLine("Travel Agent{0}:: Order sent at " +startTime.TimeOfDay+ " of {1} rooms to HS{2}", Thread.CurrentThread.Name, roomsCount, p);
                    buffer.setOneCell(strOrder);
                    Thread poller = new Thread(new ThreadStart(this.pollForConfirmation));
                    poller.Name = Thread.CurrentThread.Name;
                    poller.Start();
                }
            }
            catch (ThreadInterruptedException exception)
            {
                //Console.WriteLine("TA:" + Thread.CurrentThread.Name + "Thread Interrupted");
            }
        }

        private void pollForConfirmation()
        {
            Thread.Sleep(300);
            Boolean flag = true;
            while (flag)
            {
                String confirmation = confirmationBuffer.peekOneCell();
                if (confirmation != null)
                {
                    String[] split = confirmation.Split('|');
                    if (split[1] == Thread.CurrentThread.Name)
                    {
                        confirmation = confirmationBuffer.getOneCell();
                        if (split[0] == "confirmed")
                        {
                            TimeSpan confirmationTime = System.DateTime.Now - this.startTime;
                            Console.WriteLine("TA" + Thread.CurrentThread.Name + ":: Order " + split[0] + " and charged: $" + split[2] + " in " + confirmationTime.Milliseconds + " milliseconds");
                        }
                        flag = false;
                    }
                }
            }
        }

        public void roomsOnSale(Int32 p)
        {  // Event handler
            Console.WriteLine("HS:" + Thread.CurrentThread.Name + " -> roomsOnSale::");
            Int32 id = Convert.ToInt32(Thread.CurrentThread.Name);
            HotelSupplier supplier = HotelSupplier.getSupplier(id-1);
            supplier.resumeSubscribers();
        }
        public static void setBuffer(MultiCellBuffer buf)
        {
            buffer = buf;
        }
        public static void setConfirmationBuffer(MultiCellBuffer buf)
        {
            confirmationBuffer = buf;
        }
    }
}
