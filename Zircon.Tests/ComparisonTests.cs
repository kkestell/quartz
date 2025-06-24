using System.Linq;
using Xunit;

namespace Zircon.Tests;

public class ComparisonTests : TestBase
{
    // Comparison Operations (0x30 - 0x3F)
    [Theory]
    [InlineData(5, 5, true)]
    [InlineData(5, 10, false)]
    [InlineData(10, 5, false)]
    public void Test_Opcode_Equal_Numbers(double v1, double v2, bool expected)
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Number(v1));
        var c2 = builder.AddConstant(Value.Number(v2));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.Equal);
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

    [Fact]
    public void Test_Opcode_Equal_Strings()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Str("hello"));
        var c2 = builder.AddConstant(Value.Str("hello"));
        var c3 = builder.AddConstant(Value.Str("world"));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.Equal); // True
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c3);
        builder.AddInstruction(Opcode.Equal); // False
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Equal(2, vm.OperandStack.Count);
        Assert.Equal(Value.Boolean(true), vm.OperandStack[0]);
        Assert.Equal(Value.Boolean(false), vm.OperandStack[1]);
    }


    [Fact]
    public void Test_Opcode_GreaterThan()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Number(10));
        var c2 = builder.AddConstant(Value.Number(5));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.GreaterThan);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Boolean(true), vm.OperandStack.Last());
    }

    [Fact]
    public void Test_Opcode_LessThan()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Number(2));
        var c2 = builder.AddConstant(Value.Number(8));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.LessThan);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Boolean(true), vm.OperandStack.Last());
    }

    [Theory]
    [InlineData(10, 5, true)]
    [InlineData(5, 5, true)]
    [InlineData(5, 10, false)]
    public void Test_Opcode_GreaterThanOrEqual(double v1, double v2, bool expected)
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Number(v1));
        var c2 = builder.AddConstant(Value.Number(v2));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.GreaterThanOrEqual);
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
    [InlineData(5, 10, true)]
    [InlineData(5, 5, true)]
    [InlineData(10, 5, false)]
    public void Test_Opcode_LessThanOrEqual(double v1, double v2, bool expected)
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Number(v1));
        var c2 = builder.AddConstant(Value.Number(v2));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.LessThanOrEqual);
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
