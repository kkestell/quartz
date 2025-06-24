using System.Linq;
using Xunit;

namespace Zircon.Tests;

public class StackTests : TestBase
{
    // Stack Operations (0x01 - 0x0F)
    [Fact]
    public void Test_Opcode_PushConst()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Number(123));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Number(123), vm.OperandStack.Last());
    }

    [Fact]
    public void Test_Opcode_Pop()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Number(123));
        var c2 = builder.AddConstant(Value.Number(456));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.Pop);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Number(123), vm.OperandStack.Last());
    }

    [Fact]
    public void Test_Opcode_Dup()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var val1 = Value.Number(789);
        var c1 = builder.AddConstant(val1);
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.Dup);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Equal(2, vm.OperandStack.Count);
        Assert.Equal(val1, vm.OperandStack[0]);
        Assert.Equal(val1, vm.OperandStack[1]);
    }

    [Fact]
    public void Test_Opcode_Swap()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var val1 = Value.Number(10);
        var val2 = Value.Number(5);
        var c1 = builder.AddConstant(val1);
        var c2 = builder.AddConstant(val2);
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1); // Stack: [10]
        builder.AddInstruction(Opcode.PushConst, c2); // Stack: [10, 5]
        builder.AddInstruction(Opcode.Swap);         // Stack: [5, 10]
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Equal(2, vm.OperandStack.Count);
        Assert.Equal(val2, vm.OperandStack[0]);
        Assert.Equal(val1, vm.OperandStack[1]);
    }
}
