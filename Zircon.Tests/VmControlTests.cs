using System.Linq;
using Xunit;

namespace Zircon.Tests;

public class VmControlTests : TestBase
{
    // VM Control (0xF0 - 0xFF)
    [Fact]
    public void Test_Opcode_Halt()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Number(111));
        var c2 = builder.AddConstant(Value.Number(222));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.Halt);
        builder.AddInstruction(Opcode.PushConst, c2); // This should not be executed
        builder.EndFunction();
        var bytecode = builder.Build();
        
        // Act
        var vm = RunAndGetVM(bytecode);
        
        // Assert
        Assert.False(vm.IsRunning);
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Number(111), vm.OperandStack.Last());
    }
}