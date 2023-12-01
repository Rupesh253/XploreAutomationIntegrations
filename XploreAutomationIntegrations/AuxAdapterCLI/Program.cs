using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;

namespace AuxAdapterCLI {
    public class Program {
        public static string NL = Environment.NewLine; // shortcut
        public static string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
        public static string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
        public static string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";
        public static string YELLOW = Console.IsOutputRedirected ? "" : "\x1b[93m";
        public static string BLUE = Console.IsOutputRedirected ? "" : "\x1b[94m";
        public static string MAGENTA = Console.IsOutputRedirected ? "" : "\x1b[95m";
        public static string CYAN = Console.IsOutputRedirected ? "" : "\x1b[96m";
        public static string GREY = Console.IsOutputRedirected ? "" : "\x1b[97m";
        public static string BOLD = Console.IsOutputRedirected ? "" : "\x1b[1m";
        public static string NOBOLD = Console.IsOutputRedirected ? "" : "\x1b[22m";
        public static string UNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[4m";
        public static string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
        public static string REVERSE = Console.IsOutputRedirected ? "" : "\x1b[7m";
        public static string NOREVERSE = Console.IsOutputRedirected ? "" : "\x1b[27m";
        public static string type;
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisConsole = GetConsoleWindow();
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;
        public static void TypingConsole(string message, string color, string style) {
            //SoundPlayer typewriter = new SoundPlayer();
            //typewriter.SoundLocation = @"C:\Users\git\source\XploreAutomationIntegrations\XploreAutomationIntegrations\typewriter-key-2.wav";
            //typewriter.Load();
            foreach (char x in message.ToCharArray()) {
                //typewriter.Play();
                Console.Write($"{color}{x}{style}");
                Thread.Sleep(1);
                //typewriter.Stop();
            }
            Console.WriteLine();
        }
        public static void NoTypingConsole(string message, string color, string style) {
            Console.WriteLine($"{color}{message}{style}");
        }
        public class ProgressBar : IDisposable, IProgress<double> {
            private const int blockCount = 10;
            private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
            private const string animation = @"|/-\";

            private readonly Timer timer;

            private double currentProgress = 0;
            private string currentText = string.Empty;
            private bool disposed = false;
            private int animationIndex = 0;

            public ProgressBar() {
                timer = new Timer(TimerHandler);

                // A progress bar is only for temporary display in a console window.
                // If the console output is redirected to a file, draw nothing.
                // Otherwise, we'll end up with a lot of garbage in the target file.
                if (!Console.IsOutputRedirected) {
                    ResetTimer();
                }
            }

            public void Report(double value) {
                // Make sure value is in [0..1] range
                value = Math.Max(0, Math.Min(1, value));
                Interlocked.Exchange(ref currentProgress, value);
            }

            private void TimerHandler(object state) {
                lock (timer) {
                    if (disposed) return;

                    int progressBlockCount = (int)(currentProgress * blockCount);
                    int percent = (int)(currentProgress * 100);
                    string text = string.Format("[{0}{1}] {2,3}% {3}",
                        new string('#', progressBlockCount), new string('-', blockCount - progressBlockCount),
                        percent,
                        animation[animationIndex++ % animation.Length]);
                    UpdateText(text);

                    ResetTimer();
                }
            }

            private void UpdateText(string text) {
                // Get length of common portion
                int commonPrefixLength = 0;
                int commonLength = Math.Min(currentText.Length, text.Length);
                while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength]) {
                    commonPrefixLength++;
                }

                // Backtrack to the first differing character
                StringBuilder outputBuilder = new StringBuilder();
                outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

                // Output new suffix
                outputBuilder.Append(text.Substring(commonPrefixLength));

                // If the new text is shorter than the old one: delete overlapping characters
                int overlapCount = currentText.Length - text.Length;
                if (overlapCount > 0) {
                    outputBuilder.Append(' ', overlapCount);
                    outputBuilder.Append('\b', overlapCount);
                }

                Console.Write(outputBuilder);
                currentText = text;
            }

            private void ResetTimer() {
                timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
            }

            public void Dispose() {
                lock (timer) {
                    disposed = true;
                    UpdateText(string.Empty);
                }
            }

        }
        public void Main(string[] args) {

            Console.Write("Preparing your automation infrastructure...This will take a while.");
            using (var progress = new ProgressBar()) {
                for (int i = 0; i <= 100; i++) {
                    progress.Report((double)i / 10);
                    Thread.Sleep(1);
                }
            }
            Console.WriteLine("\nVoila! All done.");
            //Play();
            Console.Title = "CLI for executing nunit tests via nunit3 console runner or dotnet test runner";
            //Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
            //ShowWindow(ThisConsole, MAXIMIZE);
            TypingConsole($"Helper process id: {Process.GetCurrentProcess().Id}", GREEN, BOLD);
            TypingConsole("Please enter your testing type:", GREEN, BOLD);
            TypingConsole(" [1]. Smoke [2].Sanity [3]. Regression", MAGENTA, NORMAL);
            type = Console.ReadLine();
            string testingType = (type == "1") ? "Smoke" : (type == "2") ? "Sanity" : "Regression";
            TypingConsole($"Yay! You have selected [{type}].{testingType}", GREEN, NORMAL);

            NoTypingConsole("**************************************************************************************************\n", BLUE, NORMAL);
            string dotnetTestCommandFileLoc = new DirectoryInfo("../../../../").FullName + @"XploreAutomationIntegrations\dotnetTestCommand.txt";
            string dotnetTestCommand = File.ReadAllText(dotnetTestCommandFileLoc);
            TypingConsole($"Fetching format command from {dotnetTestCommandFileLoc} as\n \t{dotnetTestCommand}", YELLOW, BOLD);
            dotnetTestCommand = dotnetTestCommand.Replace("TestCategory=0", $"TestCategory={testingType}");
            File.WriteAllText(dotnetTestCommandFileLoc, dotnetTestCommand);
            TypingConsole($"Applied command from {dotnetTestCommandFileLoc} as\n \t{dotnetTestCommand}", CYAN, NORMAL);
            dotnetTestCommand = File.ReadAllText(dotnetTestCommandFileLoc);
            TypingConsole($"Executing the dotnet command: \n\t{dotnetTestCommand}", GREEN, NORMAL);
            //System.Diagnostics.Process.Start("CMD.exe", command);

            //ProcessStartInfo pi = new ProcessStartInfo("cmd.exe");
            //pi.CreateNoWindow = false;
            //pi.Arguments = "\\c " + dotnetTestCommand;
            //pi.WorkingDirectory = new DirectoryInfo("../../../../").FullName;
            //Process p = new Process();
            //p.StartInfo = pi;
            //p.Start();

            var startInfo = new ProcessStartInfo {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            var process = new Process { StartInfo = startInfo };
            process.Start();
            process.EnableRaisingEvents = true;

            //TypingConsole($"Automation Process Id: {process.Id}", MAGENTA, BOLD);
            process.StandardInput.WriteLine(dotnetTestCommand);

            process.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Exited += (sender, e) => {
                Debug.WriteLine("Process exited with exit code " + process.ExitCode.ToString());
            };
            process.Close();


            TypingConsole("Executed the dotnet command", MAGENTA, BOLD);
            NoTypingConsole("**************************************************************************************************\n", BLUE, NORMAL);
            string nunit3CommandFileLoc = new DirectoryInfo("../../../../").FullName + @"XploreAutomationIntegrations\nunit3Command.txt";
            string nunit3Command = File.ReadAllText(nunit3CommandFileLoc);
            TypingConsole($"Fetching format command from {nunit3CommandFileLoc} as\n \t{nunit3Command}", YELLOW, BOLD);
            nunit3Command = nunit3Command.Replace("cat==0", $"cat=={testingType}");
            File.WriteAllText(nunit3CommandFileLoc, nunit3Command);
            TypingConsole($"Applied command from {nunit3CommandFileLoc} as\n \t{nunit3Command}", CYAN, BOLD);
            nunit3Command = File.ReadAllText(nunit3CommandFileLoc);
            TypingConsole($"Executing the nunit command: \n\t{nunit3Command}", GREEN, NORMAL);
            //System.Diagnostics.Process.Start("CMD.exe", command);
            TypingConsole("Executed the nunit command", MAGENTA, BOLD);
            NoTypingConsole("**************************************************************************************************\n", BLUE, NORMAL);

            string resetdotnetTestCommand = dotnetTestCommand.Replace($"TestCategory={testingType}", "TestCategory=0");
            string resetnunit3Command = nunit3Command.Replace($"cat=={testingType}", "cat==0");
            TypingConsole($"Reset the nunit  command: \n\t{resetnunit3Command}", GREEN, NORMAL);
            TypingConsole($"Reset the dotnet  command: \n\t{resetdotnetTestCommand}", GREEN, NORMAL);
            File.WriteAllText(dotnetTestCommandFileLoc, resetdotnetTestCommand);
            File.WriteAllText(nunit3CommandFileLoc, resetnunit3Command);
            NoTypingConsole("**************************************************************************************************\n\n", BLUE, NORMAL);
            TypingConsole($"Done.", GREEN, BOLD);
            //Play();
        }


        public static void Play() {
            Note[] Mary =
            {
        new Note(Tone.B, Duration.QUARTER),
        new Note(Tone.A, Duration.QUARTER),
        new Note(Tone.GbelowC, Duration.QUARTER),
        new Note(Tone.A, Duration.QUARTER),
        new Note(Tone.B, Duration.QUARTER),
        new Note(Tone.B, Duration.QUARTER),
        new Note(Tone.B, Duration.HALF),
        new Note(Tone.A, Duration.QUARTER),
        new Note(Tone.A, Duration.QUARTER),
        new Note(Tone.A, Duration.HALF),
        new Note(Tone.B, Duration.QUARTER),
        new Note(Tone.D, Duration.QUARTER),
        new Note(Tone.D, Duration.HALF)
        };
            // Play the song
            Play(Mary);
        }

        // Play the notes in a song.
        protected static void Play(Note[] tune) {
            foreach (Note n in tune) {
                if (n.NoteTone == Tone.REST)
                    Thread.Sleep((int)n.NoteDuration);
                else
                    Console.Beep((int)n.NoteTone, (int)n.NoteDuration);
            }
        }

        // Define the frequencies of notes in an octave, as well as
        // silence (rest).
        protected enum Tone {
            REST = 0,
            GbelowC = 196,
            A = 220,
            Asharp = 233,
            B = 247,
            C = 262,
            Csharp = 277,
            D = 294,
            Dsharp = 311,
            E = 330,
            F = 349,
            Fsharp = 370,
            G = 392,
            Gsharp = 415,
        }

        // Define the duration of a note in units of milliseconds.
        protected enum Duration {
            WHOLE = 1600,
            HALF = WHOLE / 2,
            QUARTER = HALF / 2,
            EIGHTH = QUARTER / 2,
            SIXTEENTH = EIGHTH / 2,
        }

        // Define a note as a frequency (tone) and the amount of
        // time (duration) the note plays.
        protected struct Note {
            Tone toneVal;
            Duration durVal;

            // Define a constructor to create a specific note.
            public Note(Tone frequency, Duration time) {
                toneVal = frequency;
                durVal = time;
            }

            // Define properties to return the note's tone and duration.
            public Tone NoteTone { get { return toneVal; } }
            public Duration NoteDuration { get { return durVal; } }
        }
    }
}
