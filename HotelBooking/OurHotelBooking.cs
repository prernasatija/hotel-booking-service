using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HotelBooking
{
    class OurHotelBooking
    {
        static void Main(string[] args)
        {
            //HotelSupplier hotelSupplier = new HotelSupplier(); //multiple objects
            //MultiCellBuffer buffer = new MultiCellBuffer();
            //hotelSupplier.setBuffer(buffer);
            //Thread priceModel = new Thread(new ThreadStart(hotelSupplier.pricingModel));
            //priceModel.Start();         // Start one farmer thread
            //TravelAgency travelAgency = new TravelAgency();
            //travelAgency.setBuffer(buffer);
            //HotelSupplier.priceCut += new priceCutEvent(travelAgency.roomsOnSale);
            //Thread[] agencies = new Thread[5];
            //for (int i = 0; i < 2; i++)
            //{   // Start N retailer threads
            //    agencies[i] = new Thread(new ThreadStart(travelAgency.travelAgencyFunc));
            //    agencies[i].Name = (i + 1).ToString();
            //    agencies[i].Start();
            //}
            MultiCellBuffer buffer = new MultiCellBuffer();
            Thread[] supplier = new Thread[3];
            HotelSupplier[] hotelSupplier = new HotelSupplier[3];
            for (int j = 0; j < 3; j++)
            {
                hotelSupplier[j] = new HotelSupplier();
                hotelSupplier[j].setBuffer(buffer);
                hotelSupplier[j].hotelSupplierID = j + 1;
                supplier[j] = new Thread(new ThreadStart(hotelSupplier[j].pricingModel));
                supplier[j].Name = (j + 1).ToString() ;
                supplier[j].Start();
            }

            TravelAgency travelAgency = new TravelAgency();
            travelAgency.setBuffer(buffer);
            
            Thread[] agencies = new Thread[5];
            for (int i = 0; i < 4; i++)
            {   // Start N retailer threads
                agencies[i] = new Thread(new ThreadStart(travelAgency.travelAgencyFunc));
                agencies[i].Name = "TA" + (i + 1).ToString();
                agencies[i].Start();
            }
        }
    }
}
