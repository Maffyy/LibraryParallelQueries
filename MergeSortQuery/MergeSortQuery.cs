using System;
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.IO;
using System.Net.Sockets;
using LibraryModel;

namespace MergeSortQuery {

    /// <summary>
    /// Comparer interface for sorting books in the library by DueDate, LastName, FirstName, Shelf, Id
    /// </summary>
    class BookSorter : IComparer<Copy> {

        public int Compare(Copy a, Copy b) {

            int dueDate = a.OnLoan.DueDate.CompareTo(b.OnLoan.DueDate);
            int lastName = String.Compare(a.OnLoan.Client.LastName, b.OnLoan.Client.LastName,
                StringComparison.CurrentCulture);
            int firstName = String.Compare(a.OnLoan.Client.FirstName, b.OnLoan.Client.FirstName,
                StringComparison.CurrentCulture);
            int shelf = String.Compare(a.Book.Shelf, b.Book.Shelf,
                StringComparison.CurrentCulture);
            int id = String.Compare(a.Id, b.Id,
                StringComparison.CurrentCulture);

            if(dueDate == -1) {
                return -1;
            }
            if(dueDate == 0) {
                if(lastName == -1) {
                    return -1;
                }
                if(lastName == 0) {
                    if(firstName == -1) {
                        return -1;
                    }
                    if(firstName == 0) {
                        if(shelf == -1) {
                            return -1;
                        }
                        if(shelf == 0) {
                            if(id == -1) {
                                return -1;
                            }
                            if(id == 0) {
                                return 0;
                            }
                        }
                    }
                }
            }
            return 1;

        }
    }

    class MergeSortQuery {
        public Library Library { get; set; }
        public int ThreadCount { get; set; }

        /// <summary>
        /// Merges two lists of copies. Each time when the book from list "b" is inserted into the list "a",
        /// it is put into the right place by a comparer
        /// First we try to insert the book at the end of the list "a", then we try to insert it into other place.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public List<Copy> Merge(List<Copy> a, List<Copy> b) {
            BookSorter bookSorter = new BookSorter();
            int i = 0;
            int j = a.Count-1;
            foreach (Copy copy in b) {
                while (true) {
                    if (bookSorter.Compare(a[j],b[i]) == -1 && j != 0) {
                        j--;
                        continue;
                    }
                    a.Insert(j,b[i]);
                    j = a.Count - 1;
                    i++;
                    break;
                }
            }
            return a;
        }


        public List<Copy> MergeSort(List<Copy> filter, int count) {

            BookSorter bookSorter = new BookSorter();
            if (count == 1) {
                
                filter.Sort(bookSorter);
                return filter;
            }
            int temp = count / 2;
            count -= temp;
            List<Copy> first = filter.GetRange(0, filter.Count / 2);
            List<Copy> second = filter.GetRange(filter.Count / 2, filter.Count / 2);
            
            Thread thread = new Thread(() => second = MergeSort(second,count));
            thread.Start();
            first = MergeSort(first, count);
            thread.Join();

            return  Merge(first, second);
        }
        /// <summary>
        /// Executes query, gets books from shelves "A" to "Q", which are on a Loan.
        /// Finally books are sorted by MergeSort algorithm.
        /// </summary>
        /// <returns></returns>
        public List<Copy> ExecuteQuery() {
            if(ThreadCount == 0)
                throw new InvalidOperationException("Threads property not set and default value 0 is not valid.");

            List<Copy> copies = Library.Copies;
            List<Copy> filter = new List<Copy>();
            foreach (Copy copy in copies) {
                if (copy.Book.Shelf[2] <= 'Q' && copy.Book.Shelf[2] >= 'A' && copy.State == CopyState.OnLoan) {
                    filter.Add(copy);
                }               
            }

            return MergeSort(filter, ThreadCount);
        }
    }

    class ResultVisualizer {
        public static void PrintCopy(StreamWriter writer, Copy c) {
            writer.WriteLine("{0} {1}: {2} loaned to {3}, {4}.", c.OnLoan.DueDate.ToShortDateString(), c.Book.Shelf, c.Id, c.OnLoan.Client.LastName, System.Globalization.StringInfo.GetNextTextElement(c.OnLoan.Client.FirstName));
        }
    }
}
