using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace STR_Addon_PeruRamo_V1
{
    public class Program
    {
        static void Main()
        {
            Task.Factory.StartNew(() =>
            {
                new Main();
            });
            Application.Run();

        }
    }
}
