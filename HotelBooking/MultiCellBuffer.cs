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
                buffer[inOffset] = input;
                inOffset = inOffset % N;
            mute.Release(1);
            Int32 count = full.Release();
            full.Release(count + 1);
        }

        public String getOneCell()
        {
            full.WaitOne();
            mute.WaitOne();
                String ret = buffer[outOffset];
                outOffset = outOffset % N;
            mute.Release(1);
            Int32 count = empty.Release();
            empty.Release(count + 1);
            return ret;
        }
    }
}
