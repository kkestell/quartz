using Xunit;
using System.Linq;

namespace Zircon.Tests;

public class IoTests : TestBase
{
    // IO Operations (0x60 - 0x6F)
    [Fact]
    public void Test_Opcode_Print()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Str("Hello, Zircon!"));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.Print);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var output = RunAndCaptureOutput(bytecode);
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.Equal("Hello, Zircon!", output);
        Assert.NotNull(vm.OperandStack);
        Assert.Empty(vm.OperandStack);
    }
}