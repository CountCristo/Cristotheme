using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace AuroraPatch
{
    internal static class Program
    {
        internal static readonly Logger Logger = new Logger();

        /// <summary>
        /// The main entry point for the application.
        /// Will calculate Aurora.exe checksum, load the assembly, find the TacticalMap, and load up 3rd party patches.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            var exe = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Aurora.exe");
            if (args.Length > 0)
            {
                Logger.LogInfo("User provided Aurora.exe path: " + args[0]);
                exe = args[0];
            }
            
            if (!File.Exists(exe))
            {
                Logger.LogCritical($"File {exe} is missing or is not readable.");
                Application.Exit();

                return;
            }

            Logger.LogInfo($"Found Aurora at {exe}");

            var checksum = GetChecksum(File.ReadAllBytes(exe));
            var loader = new Loader(exe, checksum);

            AppDomain.CurrentDomain.AssemblyResolve += (sender, a) =>
            {
                foreach (var assembly in ((AppDomain)sender).GetAssemblies())
                {
                    if (assembly.FullName == a.Name)
                    {
                        return assembly;
                    }
                }

                return null;
            };

            var form = new AuroraPatchForm(loader);
            form.Show();

            Application.Run();
        }

        /// <summary>
        /// Helper method to calculate a byte array checksum.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string GetChecksum(byte[] bytes)
        {
            Logger.LogDebug("Calculating checksum");
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(bytes);
                var str = Convert.ToBase64String(hash);

                string checksum = str.Replace("/", "").Replace("+", "").Replace("=", "").Substring(0, 6);
                Logger.LogDebug("Checksum: " + checksum);

                return checksum;
            }
        }
    }
}
