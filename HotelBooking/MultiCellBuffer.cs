using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HotelBooking
{
    class MultiCellBuffer
    {
        private static Int32 N = 3; // size of buffer
        private String[] buffer = new String[N];
        private Int32 inOffset = 0; // input index
        private Int32 outOffset = 0; // output index
        // Semaphores
        private Semaphore full = new Semaphore(0, N);
        private Semaphore empty = new Semaphore(N, N);
        private Semaphore mute = new Semaphore(1, 1);

        // Sets the input string in the buffer.
        public void setOneCell(String input)
        {
            empty.WaitOne();
            mute.WaitOne();
                buffer[inOffset] = input;
                inOffset = (inOffset + 1) % N;
            mute.Release();
            full.Release();
        }

        // Returns the string on the next available index.
        public String getOneCell()
        {
            full.WaitOne();
            mute.WaitOne();
                String ret = buffer[outOffset];
                outOffset = (outOffset + 1) % N;
            mute.Release();
            empty.Release();
            return ret;
        }

        // Returns the string but doesn't remove it. Used for polling.
        public String peekOneCell()
        {
            return buffer[outOffset];
        }
    }
}
