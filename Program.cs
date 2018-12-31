using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpanAndFriends
{
    class Program
    {
        private static int SumSpan(ReadOnlySpan<int> span)
        {
            var res = 0;
            for (int i = 0; i < span.Length; i++)
            {
                res += span[i];
            }
            return res;
        }

        private static int SumSpan(ReadOnlySpan<byte> span)
        {
            var res = 0;
            for (int i = 0; i < span.Length; i++)
            {
                res += span[i];
            }
            return res;
        }

        private static double SumSpan(ReadOnlySpan<double> span)
        {
            double res = 0;
            for (int i = 0; i < span.Length; i++)
            {
                res += span[i];
            }
            return res;
        }

        private static void StackAllocTest(int a, int b, int c, int d)
        {
            // C# 7.3
            Span<int> span = stackalloc int[] { a, b, c, d };

            // C# 7.2
            //Span<int> span = stackalloc int[4];
            //span[0] = a;
            //span[1] = b;
            //span[2] = c;
            //span[3] = d;

            Console.WriteLine(SumSpan(span));
        }

        private static void ArrayPoolTest(int len = 10000)
        {
            var buf = ArrayPool<int>.Shared.Rent(len);
            try
            {
                var span = buf.AsSpan(0, len);
                int pre = SumSpan(span);
                for (int i = 0; i < 10; ++i)
                    span[i] = i;
                int post = SumSpan(span);
                Console.WriteLine($"Req {len} -> ArrayPool array size {buf.Length}; span {span.ToString()} pre {pre} post {post}");
            }
            finally
            {
                ArrayPool<int>.Shared.Return(buf);
            }
        }

        private static void MemoryPoolTest(int len = 10000)
        {
            using (var memOwner = MemoryPool<int>.Shared.Rent(len))
            {
                var mem = memOwner.Memory;
                var span = mem.Span.Slice(0, len);
                int pre = SumSpan(span);
                for (int i = 0; i < 10; ++i)
                    span[i] = i;
                int post = SumSpan(span);
                Console.WriteLine($"Req {len} -> MemoryPool mem size {mem.Length}; span {span.ToString()} pre {pre} post {post}");
            }
        }

        private static void CastTest()
        {
            Span<int> span = stackalloc int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            Console.WriteLine($"As ints {SumSpan(span)}");

            ReadOnlySpan<byte> asBytes = MemoryMarshal.AsBytes(span);
            Console.WriteLine($"As bytes {SumSpan(asBytes)}");
            span[0] = 100;
            Console.WriteLine($"As bytes {SumSpan(asBytes)} (after modification of original)");


            ReadOnlySpan<double> asDouble = MemoryMarshal.Cast<int, double>(span);
            Console.WriteLine($"As double {SumSpan(asDouble)}");
        }


        static void Main(string[] args)
        {
            StackAllocTest(1, 2, 3, 4);
            Console.WriteLine("\tNew array pool of 10000 -- note how it's the original memory is shared with memorypool");
            ArrayPoolTest();
            ArrayPoolTest();
            MemoryPoolTest();
            Console.WriteLine("\tNow create a larger one of 32000");
            MemoryPoolTest(32000);
            MemoryPoolTest(32000);
            ArrayPoolTest(32010);

            Console.WriteLine("\tCasting tests");
            CastTest();



        }
    }
}
