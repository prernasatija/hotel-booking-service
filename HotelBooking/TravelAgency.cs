using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HotelBooking
{
    class TravelAgency
    {
        public static Int32 maxTravelAgents; // maximum travel agents
        private static MultiCellBuffer buffer; // order multi-cell buffer
        private static MultiCellBuffer confirmationBuffer; // confirmation multi-cell buffer
        private static Random rng = new Random();
        private DateTime startTime; // to record the start time of a order.

        // setter methods
        public static void setBuffer(MultiCellBuffer buf)
        {
            buffer = buf;
        }

        public static void setConfirmationBuffer(MultiCellBuffer buf)
        {
            confirmationBuffer = buf;
        }

        // travel agency thread.
        public void travelAgencyFunc()
        {
            try
            {
                // get a random hotelSupplier to subscribe.
                HotelSupplier hotelSupplier = HotelSupplier.getRandomSupplier();

                Console.WriteLine("Travel Agent " + Thread.CurrentThread.Name + " subscribing to Hotel Supplier " + hotelSupplier.getHotelSupplierID());
                hotelSupplier.subscribe(Thread.CurrentThread);
                while(hotelSupplier.validOrderCount <= hotelSupplier.maxOrder)
                {
                    Int32 oldPrice = hotelSupplier.getPrice();
                    // Suspend the thread till next price cut event.
                    Thread.CurrentThread.Suspend();
                    Int32 newPrice = hotelSupplier.getPrice();
                    
                    // create an order
                    Order order = createOrder(hotelSupplier, newPrice - oldPrice);
                    String strOrder = Order.encoder(order); // encode the order
                    startTime = System.DateTime.Now; // store the order time
                    Console.WriteLine("Travel Agent {0}:: Order sent at " + startTime.TimeOfDay + " of {1} rooms to HS{2}", Thread.CurrentThread.Name, order.getAmount(), hotelSupplier.getHotelSupplierID());
                    buffer.setOneCell(strOrder); // place the order in the buffer

                    // start a polling thread to check for confirmation.
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

        // Create an order for the hotelSupplier.
        private Order createOrder(HotelSupplier hotelSupplier, Int32 discount)
        {
            Int32 roomsCount = rng.Next(1, 5);
            Int32 senderId = Convert.ToInt32(Thread.CurrentThread.Name);
            BankService bankService = new BankService();
            Int32 cardNo = bankService.getCreditCard(senderId - 1);

            return new Order(senderId, cardNo, hotelSupplier.getHotelSupplierID(), roomsCount);
        }

        // The polling thread to check if the order was confirmed.
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
                            Console.WriteLine("Travel Agent " + Thread.CurrentThread.Name + ":: Order " + split[0] + " and charged: $" + split[2] + " in " + confirmationTime.Milliseconds + " milliseconds");
                        }
                        flag = false;
                    }
                }
            }
        }

        // Event handler: mapped to the priceCut of hotelSupplier
        public void roomsOnSale(Int32 p)
        {
            Console.WriteLine("Hotel Supplier " + Thread.CurrentThread.Name + ": **DISCOUNT** Rooms On Sale!!");
            Int32 id = Convert.ToInt32(Thread.CurrentThread.Name);
            HotelSupplier supplier = HotelSupplier.getSupplier(id-1);
            supplier.resumeSubscribers();
        }
    }
}
