using System.Linq;
using Xunit;

namespace Zircon.Tests;

public class VariableTests : TestBase
{
    // Variable Operations (0x70 - 0x7F)
    [Fact]
    public void Test_Opcode_SetAndGetLocal()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var val = Value.Number(999);
        var c1 = builder.AddConstant(val);
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.SetLocal, 0);
        builder.AddInstruction(Opcode.GetLocal, 0);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(val, vm.OperandStack.Last());
        Assert.NotNull(vm.Locals);
        Assert.Single(vm.Locals);
        Assert.Equal(val, vm.Locals[0]);
    }

    [Fact]
    public void Test_Opcode_SetAndGetGlobal()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var val = Value.Number(888);
        var c1 = builder.AddConstant(val);
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.SetGlobal, 0);
        builder.AddInstruction(Opcode.GetGlobal, 0);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();
        
        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(val, vm.OperandStack.Last());
        Assert.Single(vm.Globals);
        Assert.Equal(val, vm.Globals[0]);
    }
}