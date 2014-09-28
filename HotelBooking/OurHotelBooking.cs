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
            MultiCellBuffer buffer = new MultiCellBuffer();
            MultiCellBuffer confirmationBuffer = new MultiCellBuffer();
            HotelSupplier.setBuffer(buffer);
            HotelSupplier.setConfirmationBuffer(confirmationBuffer);
            TravelAgency.maxTravelAgents = 5;
            TravelAgency travelAgency = new TravelAgency();
            TravelAgency.setBuffer(buffer);
            TravelAgency.setConfirmationBuffer(confirmationBuffer);
            Thread[] supplier = new Thread[3];
            Thread[] polling = new Thread[3];
            HotelSupplier[] hotelSupplier = new HotelSupplier[3];
            for (int j = 0; j < 3; j++)
            {
                hotelSupplier[j] = new HotelSupplier();
                
                hotelSupplier[j].priceCut += new priceCutEvent(travelAgency.roomsOnSale);
                polling[j] = new Thread(new ThreadStart(hotelSupplier[j].poll));
                polling[j].Start();
                hotelSupplier[j].hotelSupplierID = j + 1;
                supplier[j] = new Thread(new ThreadStart(hotelSupplier[j].pricingModel));
                supplier[j].Name = (j + 1).ToString() ;
                supplier[j].Start();
            }

            Thread[] agencies = new Thread[TravelAgency.maxTravelAgents];
            for (int i = 0; i < TravelAgency.maxTravelAgents; i++)
            {   // Start N retailer threads
                agencies[i] = new Thread(new ThreadStart(travelAgency.travelAgencyFunc));
                agencies[i].Name = (i + 1).ToString();
                agencies[i].Start();
            }
        }
    }
}
