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
        public event priceCutEvent priceCut;
        private Semaphore mute = new Semaphore(1, 1);
        private Int32 validOrder = 0;
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

        public void getAndProcessOrder()
        {
            Console.WriteLine(this.hotelSupplierID + " processing starts ");
            String orderStr = buffer.getOneCell();
            Order order = Order.decoder(orderStr);
            // TODO: order processing logic
            // if valid
            mute.WaitOne();
                this.validOrder++;
                if (this.validOrder == 10)
                    terminate();
            mute.Release(1);
            Console.WriteLine(this.hotelSupplierID + " order processing success: " + this.validOrder);
            Thread.CurrentThread.Abort();
        }

        private void terminate()
        {
            int i = 0;
            while (i < ta)
            {
                Thread agent = this.subscribers[i];
                if (agent.ThreadState == ThreadState.Suspended)
                    Console.WriteLine("suspended thread");
                    //agent.Resume();
                agent.Abort();
                Console.WriteLine(subscribers[i].Name + " is terminated by " + this.hotelSupplierID);
                i++;
            }
            mute.Release(1);
            Thread.CurrentThread.Abort();
        }

        public void poll()
        {
            while (true)
            {
                Thread.Sleep(500);
                String orderStr = buffer.peekOneCell();
                if (orderStr != null)
                {
                    Order order = Order.decoder(orderStr);
                    if (order.getReceiverId() == this.hotelSupplierID) {
                        Console.WriteLine(this.hotelSupplierID + ":POLLED:");
                        Thread orderProcessing = new Thread(new ThreadStart(this.getAndProcessOrder));
                        orderProcessing.Start();
                    }
                }
            }
        }

    }
}
