using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodilityTask3Demo
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Test1 pass? {0}", Test1().ToString());
            Console.WriteLine("Test2 pass? {0}", Test2().ToString());
            Console.WriteLine("Test3 pass? {0}", Test3().ToString());
            Console.WriteLine("Test4 pass? {0}", Test4().ToString());
            Console.WriteLine("Test5 pass? {0}", Test5().ToString());

            Console.ReadLine();
        }

        public static int solution(int Y, string A, string B, string W)
        {
            DateTime startDate, endDate;

            DateTime m1 = DateTime.ParseExact(A + Y, "MMMMyyyy", CultureInfo.CurrentCulture);
            DateTime m2 = DateTime.ParseExact(B + Y, "MMMMyyyy", CultureInfo.CurrentCulture);

            startDate = m1 < m2 ? m1 : m2;
            endDate = m1 < m2 ? m2.AddDays(DateTime.DaysInMonth(m2.Year, m2.Month) - 1) : m1.AddDays(DateTime.DaysInMonth(m1.Year, m1.Month) - 1);

            while (startDate.DayOfWeek != DayOfWeek.Monday)
            {
                startDate = startDate.AddDays(1);
            }

            while (endDate.DayOfWeek != DayOfWeek.Sunday)
            {
                endDate = endDate.AddDays(-1);
            }

            return (endDate.Subtract(startDate).Days + 1) / 7;
        }

        public static int solution2(int Y, string A, string B, string W) 
        {
            if (A.Equals(B))
            {
                throw new InvalidOperationException("invalid arguments");
            }

            int febDayCount = Y%4 == 0? 29 : 28;
           
            int[] dayCount = new int[12] { 31, febDayCount, 31 ,30, 31, 30, 31, 31, 30 , 31 ,30 ,31};

            string[] days = new string[7] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"};

            string[] months = new string[12] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"};

            int end1 = Array.FindIndex<string>(months, p => p.Equals(A));
            int end2 = Array.FindIndex<string>(months, p => p.Equals(B));

            int startMonth = end1 < end2 ? end1 : end2;
            int endMonth = end1 < end2 ? end2 : end1;

            int dayOfWeek = Array.FindIndex<string>(days, p => p.Equals(W));

            int totalDays = 0;

            for(int i=0; i<=endMonth; i++)
            {
                if(i == startMonth)
                {
                    if(dayOfWeek == 0)
                    {
                        totalDays = totalDays + dayCount[i] ;
                    }
                    else
                    {
                        totalDays = totalDays + dayCount[i] - (days.Length - dayOfWeek);
                    }
                }
                else if(i > startMonth && i < endMonth)
                {
                    totalDays = totalDays + dayCount[i];
                }
                else if(i == endMonth)
                {
                    int dayOfWeekEndOfMonth = (dayOfWeek + dayCount[i] - 1) % 7;

                    if(dayOfWeekEndOfMonth == 6)
                    {
                        totalDays = totalDays + dayCount[i];
                    }
                    else
                    {
                        totalDays = totalDays + dayCount[i] - (dayOfWeekEndOfMonth + 1);
                    }
                }

                dayOfWeek = (dayOfWeek + dayCount[i]) % 7;
            }

            return totalDays / 7;
        }

        public static bool Test1()
        {
            return (7 == solution(2015, "January", "February", "Thursday"));
        }

        public static bool Test2()
        {
            return (8 == solution(2015, "February", "March", "Thursday"));
        }

        public static bool Test3()
        {
            return (8 == solution(2014, "November", "December", "Wednesday"));
        }

        public static bool Test4()
        {
            return (17 == solution(2015, "March", "June", "Thursday"));
        }

        public static bool Test5()
        {
            return (4 == solution(2015, "March", "March", "Thursday"));
        }

    }
}
