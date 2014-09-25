using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HotelBooking
{
    class MultiCellBuffer
    {
        private static Int32 N = 3;
        private String[] buffer = new String[N];
        private Int32 inOffset = 0;
        private Int32 outOffset = 0;
        private Semaphore full = new Semaphore(0, N);
        private Semaphore empty = new Semaphore(N, N);
        private Semaphore mute = new Semaphore(1, 1);

        public void setOneCell(String input)
        {
            empty.WaitOne();
            // TODO: semaphore
            mute.WaitOne();
                Console.WriteLine("Set to Buffer: " + inOffset);
                buffer[inOffset] = input;
                inOffset = (inOffset + 1) % N;
                //Int32 count = full.Release() + 1;
            mute.Release();
            full.Release();
        }

        public String getOneCell()
        {
            full.WaitOne();
            mute.WaitOne();
                Console.WriteLine("Get from Buffer: " + outOffset);
                String ret = buffer[outOffset];
                outOffset = (outOffset + 1) % N;
                //Int32 count = empty.Release() + 1;
            mute.Release();
            empty.Release();
            return ret;
        }

        public String peekOneCell()
        {
            return buffer[outOffset];
        }
    }
}
