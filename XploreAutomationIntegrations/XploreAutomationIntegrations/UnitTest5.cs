using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XploreAutomationIntegrations {
    public class UnitTest5 {

        [Test]
        [Category("Smoke")]
        [Category("Regression")]
        public void Test() {
            Console.WriteLine("UnitTest5.Test");
            Console.Beep();
        }
    }
}
