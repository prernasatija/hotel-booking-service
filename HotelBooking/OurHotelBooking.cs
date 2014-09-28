//=============================================
// Implementation of a hotel booking senario
//=============================================

/* Project 2:
 *
 * Learning: Event-driven programming, Multi-Threading, Synchronization, producer consumer problem.
 * 
 * This program is written as a project of course CSE 598: Distributed Software Development
 * Professor: Yinong Chen
 * Authors: 1. Prerna Satija (33.33%)
 *          2. Nitesh Kedia (33.33%)
 *          3. Nishant Bansal (33.34%)
 * Start Date: 09/15/2014
 * Submission Date: 09/28/2014
 */

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
            init(); // to initialize some parameters.
            TravelAgency travelAgency = new TravelAgency();
            for (int j = 0; j < HotelSupplier.maxHotelSupplier; j++)
            {
                HotelSupplier hotelSupplier = new HotelSupplier(j + 1);
                hotelSupplier.priceCut += new priceCutEvent(travelAgency.roomsOnSale);

                // start a pricing model thread for this hotel supplier
                Thread supplier = new Thread(new ThreadStart(hotelSupplier.pricingModel));
                supplier.Name = (j + 1).ToString();
                supplier.Start();

                // start a polling thread for this hotel supplier
                Thread polling = new Thread(new ThreadStart(hotelSupplier.poll));
                polling.Name = (j + 1).ToString();
                polling.Start();
            }

            for (int i = 0; i < TravelAgency.maxTravelAgents; i++)
            {
                // start a travel agency thread.
                Thread agency = new Thread(new ThreadStart(travelAgency.travelAgencyFunc));
                agency.Name = (i + 1).ToString();
                agency.Start();
            }
        }

        // Initialze some parameters.
        public static void init()
        {
            HotelSupplier.maxHotelSupplier = 3; // hotel supplier count.
            TravelAgency.maxTravelAgents = 5; // travel agent count

            MultiCellBuffer buffer = new MultiCellBuffer();
            HotelSupplier.setBuffer(buffer); // set order buffer
            TravelAgency.setBuffer(buffer); // set order buffer

            MultiCellBuffer confirmationBuffer = new MultiCellBuffer();
            HotelSupplier.setConfirmationBuffer(confirmationBuffer); // set confirmation buffer
            TravelAgency.setConfirmationBuffer(confirmationBuffer); // set confirmation buffer
        }
    }
}
