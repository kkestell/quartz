using System;
using System.IO;

namespace Zircon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} <bytecode_file>");
                return;
            }

            string bytecodeFilename = args[0];
            try
            {
                Bytecode bytecode = Bytecode.FromFile(bytecodeFilename);

                VirtualMachine vm = new(bytecode);
                vm.Run();
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Failed to load bytecode from '{bytecodeFilename}': {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred during VM execution: {ex.Message}");
            }
        }
    }
}