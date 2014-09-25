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
            // TODO:::::APPLY SEMAPHORE
            Console.WriteLine("TA:" + travelAgent.Name + " subscribed as " + ta + "th to HS:" + this.hotelSupplierID);
            this.subscribers[ta] = travelAgent;
            ta++;
        }

        public void resumeSubscribers()
        {
            int i = 0;
            while (i < ta)
            {
                if (this.subscribers[i].ThreadState == ThreadState.Suspended)
                {
                    Console.WriteLine("TA:" + subscribers[i].Name + " is Resuming from HS:" + this.hotelSupplierID);
                    this.subscribers[i].Resume();
                }
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
            Thread.Sleep(1000);
            while(this.validOrder < 5 && ta > 0)
            {
                Thread.Sleep(200);
                // Take the order from the queue of the orders;
                // Decide the price based on the orders
                Int32 p = rng.Next(5, 10); // Generate a random price
                // Console.WriteLine("New Price is {0}", p);
                if (p < this.hotelPrice)
                { // a price cut 
                    if (this.priceCut != null)
                    { // there is at least a subscriber
                        Console.WriteLine("Price Reduced for HS:" +Thread.CurrentThread.Name);
                        this.priceCut(p); // emit event to subscribers
                    }
                }
                this.hotelPrice = p;
                
            }
            Console.WriteLine("Terminating HS" + Thread.CurrentThread.Name);
            terminate();
        }

        public void getAndProcessOrder()
        {
            Console.WriteLine("HS:" + this.hotelSupplierID + " processing starts....");
            String orderStr = buffer.getOneCell();
            Order order = Order.decoder(orderStr);
            // TODO: order processing logic
            // if valid
            mute.WaitOne();
            if(this.validOrder < 5){
                this.validOrder++;
                Console.WriteLine("HS:" + this.hotelSupplierID + " no. of order processed successfully: " + this.validOrder);
            }
            else{
                // discard the order
            }
            mute.Release();
        }

        private void terminate()
        {
            int i = 0;
            while (i < ta)
            {
                Thread agent = this.subscribers[i];
                if (agent.ThreadState == ThreadState.Suspended)
                {
                    agent.Resume();
                }
                Console.WriteLine("Terminating TA" + agent.Name);
                agent.Interrupt();
                i++;
            }
        }

        public void poll()
        {
            int i = 0;
            Boolean flag = true;
            while (flag)
            {
                Thread.Sleep(200);
                String orderStr = buffer.peekOneCell();
                if (orderStr != null)
                {
                    Order order = Order.decoder(orderStr);
                    if (order.getReceiverId() == this.hotelSupplierID) {
                        if (this.validOrder < 5)
                        {
                            i = 0;
                            Console.WriteLine("HS:" + this.hotelSupplierID + ":POLLED:");
                            Thread orderProcessing = new Thread(new ThreadStart(this.getAndProcessOrder));
                            orderProcessing.Start();
                        }
                        else
                        {
                            i = i + 3;
                            orderStr = buffer.getOneCell();
                        }
                    }
                    else { i = i + 1; }
                }
                if (i >= 9)
                {
                    // TODO:
                    Console.WriteLine("HS:" + this.hotelSupplierID + " poller aborted.");
                    //Thread.CurrentThread.Abort();
                    flag = false;
                }
            }
        }

    }
}
