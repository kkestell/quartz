using System;
using System.IO;

namespace Zircon
{
    // The entry point for the Zircon VM application.
    public class Program
    {
        public static void Main(string[] args)
        {
            // Check if a bytecode file argument is provided.
            if (args.Length < 1)
            {
                Console.Error.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} <bytecode_file>");
                return;
            }

            string bytecodeFilename = args[0];
            try
            {
                // Load the bytecode from the specified file.
                Bytecode bytecode = Bytecode.FromFile(bytecodeFilename);

                // Initialize and run the virtual machine.
                VirtualMachine vm = new(bytecode);
                vm.Run();
            }
            catch (IOException ex)
            {
                // Handle I/O errors during file loading (e.g., file not found, invalid format).
                Console.Error.WriteLine($"Failed to load bytecode from '{bytecodeFilename}': {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions during VM execution.
                Console.Error.WriteLine($"An error occurred during VM execution: {ex.Message}");
                // Optionally print stack trace for debugging
                // Console.Error.WriteLine(ex.StackTrace);
            }
        }
    }
}