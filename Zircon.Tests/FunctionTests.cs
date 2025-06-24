using System.Linq;
using Xunit;

namespace Zircon.Tests;

public class FunctionTests : TestBase
{
    // Function Operations (0x80 - 0x8F)
    [Fact]
    public void Test_Opcode_CallAndReturn()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Number(10));
        var c2 = builder.AddConstant(Value.Number(20));
        var func1Index = (ushort)1;

        // Main function (index 0)
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.Call, func1Index);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();

        // Add function (index 1)
        builder.StartFunction(numArgs: 2);
        builder.AddInstruction(Opcode.GetLocal, 0);
        builder.AddInstruction(Opcode.GetLocal, 1);
        builder.AddInstruction(Opcode.Add);
        builder.AddInstruction(Opcode.Return);
        builder.EndFunction();

        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Number(30), vm.OperandStack.Last());
    }

    [Fact]
    public void Test_Implicit_Nil_Return()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var func1Index = (ushort)1;

        // Main function (index 0)
        builder.StartFunction();
        builder.AddInstruction(Opcode.Call, func1Index);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();

        // Empty function (index 1)
        builder.StartFunction(numArgs: 0);
        builder.EndFunction();

        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Nil(), vm.OperandStack.Last());
    }

    [Fact]
    public void Test_Explicit_Nil_Return()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var func1Index = (ushort)1;

        // Main function (index 0)
        builder.StartFunction();
        builder.AddInstruction(Opcode.Call, func1Index);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();

        // Function that returns nil (index 1)
        builder.StartFunction(numArgs: 0);
        builder.AddInstruction(Opcode.PushNil);
        builder.AddInstruction(Opcode.Return);
        builder.EndFunction();

        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Nil(), vm.OperandStack.Last());
    }
}
