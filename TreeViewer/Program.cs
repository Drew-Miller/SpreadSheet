using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CptS322;

namespace TreeViewer
{
    class Program
    {
        static void Main(string[] args)
        {
            //the debug bool is to alter the expression
            //for you, the ta's, standards.
            //I have a menu that I was using which is implemented slightly
            //different, so I created a second menu which uses the same functionanilty
            //minus some of the fancier console descriptions.
            bool debug = true;
            //original expression
            string expression = "A1+B1+C1";
            ExpTree ET = new ExpTree(expression, new Dictionary<string, double>());

            //runs the menu2 if debug is true. Will be true by default
            if (debug)
            {
                ET.Menu2();
            }

            //menu1 for a more luxurious console menu.
            else
            {
               ET.Menu1();
            }
        }
    }
}
