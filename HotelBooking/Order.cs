using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking
{
    class Order
    {
        private Int32 senderId;
        private Int32 cardNo;
        private Int32 receiverId;
        private Int32 amount;

        public Int32 getSenderId()
        {
            return this.senderId;
        }
        public Int32 getCardNo()
        {
            return this.cardNo;
        }
        public Int32 getReceiverId()
        {
            return this.receiverId;
        }
        public Int32 getAmount()
        {
            return this.amount;
        }

        public Order(Int32 senderId, Int32 cardNo, Int32 receiverId, Int32 amount)
        {
            this.senderId = senderId;
            this.cardNo = cardNo;
            this.receiverId = receiverId;
            this.amount = amount;
        }

        public static String encoder(Order order)
        {
            String ret = Convert.ToString(order.senderId) + "|";
            ret = ret + Convert.ToString(order.cardNo) + "|";
            ret = ret + Convert.ToString(order.receiverId) + "|";
            ret = ret + Convert.ToString(order.amount);
            return ret;
        }

        public static Order decoder( String stringOrder)
        {
            String[] split = stringOrder.Split('|');
            Order order = new Order(Convert.ToInt32(split[0]),
                                    Convert.ToInt32(split[1]),
                                    Convert.ToInt32(split[2]),
                                    Convert.ToInt32(split[3]));
            return order;
        }
    }
}
