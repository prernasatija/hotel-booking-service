using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace HotelBooking
{
    public delegate void priceCutEvent(Int32 pr);
    class HotelSupplier
    {
        private static HotelSupplier[] list = new HotelSupplier[3];
        private static int count = 0; 
        private MultiCellBuffer buffer;
        public Int32 hotelSupplierID;
        private Thread[] subscribers = new Thread[5];
        private Int32 ta = 0;
        static Random rng = new Random();
        public event priceCutEvent priceCut; // Define event
        private Int32 hotelPrice = 10;

        public static HotelSupplier getSupplier(int i)
        {
            return list[i];
        }
        private static void setSupplier(HotelSupplier supplier)
        {
            list[count] = supplier;
            count++;
        }

        public void subscribe(Thread travelAgent)
        {
            Console.WriteLine(travelAgent.Name + " subscribed as " + ta+ "th to " + this.hotelSupplierID);
            this.subscribers[ta] = travelAgent;
            ta++;
        }

        public void resumeSubscribers()
        {
            int i = 0;
            while (i < ta)
            {
                Console.WriteLine(subscribers[i].Name + " is Resuming from " +this.hotelSupplierID);
                if (this.subscribers[i].ThreadState == ThreadState.Suspended) 
                    this.subscribers[i].Resume();
                i++;
            }
        }

        public HotelSupplier() {
            setSupplier(this);
        }

        public HotelSupplier(Int32 id)
        {
            this.hotelSupplierID = id;
            setSupplier(this);
        }

        public void setBuffer(MultiCellBuffer buffer)
        {
            this.buffer = buffer;
        }
        public Int32 getPrice()
        {
            return hotelPrice;
        }
        public void changePrice(Int32 price)
        {
            if (price < hotelPrice)
            { // a price cut 
                if (this.priceCut != null)  // there is at least a subscriber
                    this.priceCut(price); // emit event to subscribers
            }
            hotelPrice = price;
        }
        public void pricingModel()
        {
            for (Int32 i = 0; i < 50; i++)
            {
                Thread.Sleep(500);
                // Take the order from the queue of the orders;
                // Decide the price based on the orders
                Int32 p = rng.Next(5, 10); // Generate a random price
                // Console.WriteLine("New Price is {0}", p);
                if (p < this.hotelPrice)
                { // a price cut 
                    if (this.priceCut != null)
                    { // there is at least a subscriber
                        Console.WriteLine("Price Reduced " +Thread.CurrentThread.Name);
                        this.priceCut(p); // emit event to subscribers
                    }
                }
                this.hotelPrice = p;
                
            }
        }

    }
}
