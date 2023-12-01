using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxAdapterCLI {
    public class Parallel {
        public static void Main(string[] atgs) {

            Process process1 = new Process();
            Process process2 = new Process();
            Process process3 = new Process();
            process1.StartInfo.FileName = "notepad.exe";
            process1.StartInfo.Arguments = @"C:\Users\git\source\XploreAutomationIntegrations\AuxAdapterCLI\dotnetTestCommand.txt";
            process1.Start();
            Console.WriteLine($"Process1:{process1.Id}");
            process2.StartInfo.FileName = "notepad.exe";
            process2.StartInfo.Arguments = @"C:\Users\git\source\XploreAutomationIntegrations\AuxAdapterCLI\dotnetTestCommand.txt";
            process2.Start();
            Console.WriteLine($"Process2:{process2.Id}");
            process3.StartInfo.FileName = "notepad.exe";
            process3.StartInfo.Arguments = @"C:\Users\git\source\XploreAutomationIntegrations\AuxAdapterCLI\dotnetTestCommand.txt";
            process3.Start();
            Console.WriteLine($"Process3:{process3.Id}");
            process1.WaitForExit();
            process2.WaitForExit();
            process3.WaitForExit();
        }

    }
}