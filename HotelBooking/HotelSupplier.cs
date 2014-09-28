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
        private Int32 hotelSupplierID;
        public static Int32 maxHotelSupplier; // maximum hotel suppliers
        private static MultiCellBuffer buffer; // order multi-cell buffer
        private static MultiCellBuffer confirmationBuffer; // confirmation multi-cell buffer
        public event priceCutEvent priceCut;
        
        // to maintain a list of all the active hotelSuppliers
        private static HotelSupplier[] supplierList = new HotelSupplier[3];
        private static int supplierCount = 0;
        private static Random rng = new Random();

        // list of subscribers to this hotel supplier. Using the max array.
        private Thread[] subscribers = new Thread[TravelAgency.maxTravelAgents];
        public Int32 agentSubscribedCount = 0;

        // Initial values.
        public Int32 validOrderCount = 0;
        private Int32 hotelPrice = 10;
        public Int32 maxOrder = 10;
        private static Int32 maxRooms = 50;
        private Int32 availableRooms = 50;

        private Semaphore lockValidOrderCount = new Semaphore(1, 1);

        /** Constructors **/
        public HotelSupplier()
        {
            setSupplier(this);
        }

        public HotelSupplier(Int32 id)
        {
            this.hotelSupplierID = id;
            setSupplier(this);
        }

        /** setter and getter methods **/
        public static void setBuffer(MultiCellBuffer buf)
        {
            buffer = buf;
        }
        public static void setConfirmationBuffer(MultiCellBuffer buf)
        {
            confirmationBuffer = buf;
        }

        public Int32 getPrice()
        {
            return hotelPrice;
        }

        public Int32 getHotelSupplierID()
        {
            return hotelSupplierID;
        }

        // stores the supplied hotelSupplier object in the list.
        private static void setSupplier(HotelSupplier supplier)
        {
            supplierList[supplierCount] = supplier;
            supplierCount++;
        }

        // returns the hotelSupplier object corresponding to the i index.
        public static HotelSupplier getSupplier(int i)
        {
            return supplierList[i];
        }

        // returns a kind of random hotelSupplier object from the list.
        public static HotelSupplier getRandomSupplier()
        {
            for (int i = 0; i < maxHotelSupplier; i++)
            {
                HotelSupplier supplier = getSupplier(i);
                if (supplier.agentSubscribedCount == 0)
                    return supplier;
            }
            Int32 id = rng.Next(0, maxHotelSupplier-1);
            return getSupplier(id);
        }
        
        // subscribes/registers the travelAgent to this hotelSupplier.
        public void subscribe(Thread travelAgent)
        {
            this.subscribers[agentSubscribedCount] = travelAgent;
            agentSubscribedCount++;
        }

        // resumes all the subcribed travelAgent threads.
        public void resumeSubscribers()
        {
            int i = 0;
            while (i < agentSubscribedCount)
            {
                if (this.subscribers[i].ThreadState == ThreadState.Suspended)
                {
                    this.subscribers[i].Resume();
                }
                i++;
            }
        }

        // Sets the new price and fires the event if the price is reduced.
        public void changePrice(Int32 price)
        {
            if (price < this.hotelPrice)
            { // a price cut 
                if (this.priceCut != null)  // there is at least a subscriber
                    this.priceCut(price); // emit event to subscribers
            }
            this.hotelPrice = price;
        }

        // Pricing Model: decides a new price for this hotel supplier based on available rooms.
        public void pricingModel()
        {
            // sleep so that some travelAgents get subscribed.
            Thread.Sleep(1000);
            while (this.validOrderCount < maxOrder && agentSubscribedCount > 0)
            {
                Int32 newPrice = ((2*maxRooms)/availableRooms)*rng.Next(5, 10);
                changePrice(newPrice);
                Thread.Sleep(2500);
            }
            Console.WriteLine("Terminating Hotel Supplier " + Thread.CurrentThread.Name);
            terminate();
        }

        // Terminates (interrupts) all the travel agent thread subscribed to this hotel supplier.
        private void terminate()
        {
            int i = 0;
            while (i < agentSubscribedCount)
            {
                Thread agent = this.subscribers[i];
                if (agent.ThreadState == ThreadState.Suspended)
                {
                    agent.Resume();
                }
                Console.WriteLine("Terminating Travel Agent " + agent.Name);
                agent.Interrupt();
                i++;
            }
        }

        // Polling Thread: peeks the buffer to get it's order and then start a order processing thread to process them.
        public void poll()
        {
            Boolean flag = true;
            while (flag)
            {
                Thread.Sleep(100);
                String orderStr = buffer.peekOneCell();
                if (orderStr != null)
                {
                    Order order = Order.decoder(orderStr);
                    if (order.getReceiverId() == this.hotelSupplierID) {
                        if (this.validOrderCount < maxOrder)
                        {
                            Thread orderProcessing = new Thread(new ThreadStart(this.getAndProcessOrder));
                            orderProcessing.Start();
                        }
                        else
                        {
                            orderStr = buffer.getOneCell();
                        }
                    }
                }
            }
        }


        /* ORDER PROCESSING THREAD::
         * processes the order using encrptyion and banking service.
         * sets the comfirmation message in the confirmation buffer for travel agency.
         */
        public void getAndProcessOrder()
        {
            String orderStr = buffer.getOneCell();
            Order order = Order.decoder(orderStr);

            String validStatus = validateCreditCard(order);
            String confirmationStatus = "not confirmed|" + Convert.ToString(order.getSenderId()) + "|0";

            if (validStatus == "valid")
            {
                double amountCharged = getTotalCharge(order.getAmount());
                lockValidOrderCount.WaitOne();
                if (this.validOrderCount < maxOrder)
                {
                    confirmationStatus = "confirmed|" + Convert.ToString(order.getSenderId() + "|" + amountCharged);
                    this.validOrderCount++;

                    availableRooms = availableRooms - order.getAmount();
                    Console.WriteLine("Hotel Supplier " + this.hotelSupplierID + ":: has processed " + validOrderCount + " orders successfully");
                }
                lockValidOrderCount.Release();
            }
            confirmationBuffer.setOneCell(confirmationStatus);
        }

        // validates the creditCard for input order. uses encryption and banking service.
        private String validateCreditCard(Order order)
        {
            Int32 cardNo = order.getCardNo();
            EncryptionService.ServiceClient service = new EncryptionService.ServiceClient();
            String encryptedCardNo = service.Encrypt(Convert.ToString(cardNo));
            BankService bankService = new BankService();
            return bankService.validateCreditCard(encryptedCardNo, order.getSenderId() - 1);
        }

        // computes the total amount charged for a order.
        private double getTotalCharge(Int32 roomsOrdered)
        {
            Int32 amount = this.hotelPrice * roomsOrdered;
            double tax = 0.1;
            double locationCharge = 0.02;
            return (amount + tax * amount + locationCharge * amount);
        }
        /* ORDER PROCESSING THREAD:: end*/
    }
}
