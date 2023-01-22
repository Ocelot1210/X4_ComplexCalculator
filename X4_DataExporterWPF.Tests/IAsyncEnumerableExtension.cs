// The MIT License (MIT)
// 
// Copyright(c).NET Foundation and Contributors
// 
// All rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// https://github.com/dotnet/reactive

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace X4_DataExporterWPF.Tests
{
    internal static class IAsyncEnumerableExtension
    {
        // REVIEW: This type of blocking is an anti-pattern. We may want to move it to System.Interactive.Async
        //         and remove it from System.Linq.Async API surface.

        /// <summary>
        /// Converts an async-enumerable sequence to an enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to convert to an enumerable sequence.</param>
        /// <returns>The enumerable sequence containing the elements in the async-enumerable sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        internal static IEnumerable<TSource> ToEnumerable<TSource>(this IAsyncEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return Core(source);

            static IEnumerable<TSource> Core(IAsyncEnumerable<TSource> source)
            {
                var e = source.GetAsyncEnumerator(default);

                try
                {
                    while (true)
                    {
                        if (!Wait(e.MoveNextAsync()))
                            break;

                        yield return e.Current;
                    }
                }
                finally
                {
                    Wait(e.DisposeAsync());
                }
            }
        }

        // NB: ValueTask and ValueTask<T> do not have to support blocking on a call to GetResult when backed by
        //     an IValueTaskSource or IValueTaskSource<T> implementation. Convert to a Task or Task<T> to do so
        //     in case the task hasn't completed yet.

        private static void Wait(ValueTask task)
        {
            var awaiter = task.GetAwaiter();

            if (!awaiter.IsCompleted)
            {
                task.AsTask().GetAwaiter().GetResult();
                return;
            }

            awaiter.GetResult();
        }

        private static T Wait<T>(ValueTask<T> task)
        {
            var awaiter = task.GetAwaiter();

            if (!awaiter.IsCompleted)
            {
                return task.AsTask().GetAwaiter().GetResult();
            }

            return awaiter.GetResult();
        }
    }
}
