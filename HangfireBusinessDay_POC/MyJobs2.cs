using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireBusinessDay_POC
{
    public static class MyJobs2
    {
        [BusinessDay("HangfireBusinessDay_POC", nameof(MyJobs2), "PerformImportantTask", "HangfireBusinessDay_POC")]
        public static void PerformImportantTask()
        {
            Console.WriteLine($"Performing an important task of jobs2 class..... {nameof(MyJobs2)}!");
        }
    }
}
