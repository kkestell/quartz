using Zircon;

namespace Zircon.Tests;

// A base class for tests to share common setup and helper methods.
public abstract class TestBase
{
    // A helper method to run the VM with a logger and return the instance for inspection.
    protected VirtualMachine RunAndGetVM(Bytecode bytecode)
    {
        var vm = new VirtualMachine(bytecode);
        vm.Run();
        return vm;
    }
    
    protected string RunAndCaptureOutput(Bytecode bytecode)
    {
        var vm = new VirtualMachine(bytecode);
        var stringWriter = new StringWriter();
        
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);

        try
        {
            vm.Run();
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return stringWriter.ToString().Trim().Replace("\r\n", "\n");
    }
}