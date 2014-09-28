using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking
{
    class BankService
    {
        private static Int32[] registeredCreditCards = new Int32[TravelAgency.maxTravelAgents];

        public Int32 getCreditCard(Int32 id)
        {
            Int32 cardNo = 0;

            for (int j = 0; j < 4; j++)
            {
                cardNo = cardNo * 10 + id;
            }
            lock (registeredCreditCards)
            {
                
                registeredCreditCards[id] = cardNo;
            }
            return cardNo;
        }

        public String validateCreditCard(String encryptedCardNo, Int32 customerId)
        {
            EncryptionService.ServiceClient encrytService = new EncryptionService.ServiceClient();
            Int32 cardNo = Convert.ToInt32(encrytService.Decrypt(encryptedCardNo));
            if (cardNo == registeredCreditCards[customerId])
            {
                return "valid";
            }
            return "not valid";
        }
    }
}
