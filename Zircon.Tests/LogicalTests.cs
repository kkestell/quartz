using System.Linq;
using Xunit;

namespace Zircon.Tests;

public class LogicalTests : TestBase
{
    // Logical Operations (0x20 - 0x2F)
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public void Test_Opcode_And(bool v1, bool v2, bool expected)
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Boolean(v1));
        var c2 = builder.AddConstant(Value.Boolean(v2));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.And);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();
    
        // Act
        var vm = RunAndGetVM(bytecode);
    
        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Boolean(expected), vm.OperandStack.Last());
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    public void Test_Opcode_Or(bool v1, bool v2, bool expected)
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Boolean(v1));
        var c2 = builder.AddConstant(Value.Boolean(v2));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.Or);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Boolean(expected), vm.OperandStack.Last());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Test_Opcode_Not(bool val, bool expected)
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Boolean(val));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.Not);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Boolean(expected), vm.OperandStack.Last());
    }
}
