using System.Linq;
using Xunit;

namespace Zircon.Tests;

public class ControlFlowTests : TestBase
{
    // Control Flow Operations (0x40 - 0x4F)
    [Fact]
    public void Test_Opcode_Jump()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var c1 = builder.AddConstant(Value.Number(111));
        var c2 = builder.AddConstant(Value.Number(222));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, c2);
        builder.AddInstruction(Opcode.Jump, 3);
        builder.AddInstruction(Opcode.Pop);
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);
        
        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Equal(2, vm.OperandStack.Count);
        Assert.Equal(Value.Number(222), vm.OperandStack[0]);
        Assert.Equal(Value.Number(111), vm.OperandStack[1]);
    }

    [Theory]
    [InlineData(true, 100)]
    [InlineData(false, 200)]
    public void Test_Opcode_JumpIfTrue(bool condition, double expected)
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var cCond = builder.AddConstant(Value.Boolean(condition));
        var cVal1 = builder.AddConstant(Value.Number(100));
        var cVal2 = builder.AddConstant(Value.Number(200));
        
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, cCond);
        builder.AddInstruction(Opcode.JumpIfTrue, 4);
        builder.AddInstruction(Opcode.PushConst, cVal2);
        builder.AddInstruction(Opcode.Jump, 5);
        builder.AddInstruction(Opcode.PushConst, cVal1);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();
        
        // Act
        var vm = RunAndGetVM(bytecode);
        
        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Number(expected), vm.OperandStack.Last());
    }

    [Theory]
    [InlineData(false, 100)]
    [InlineData(true, 200)]
    public void Test_Opcode_JumpIfFalse(bool condition, double expected)
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var cCond = builder.AddConstant(Value.Boolean(condition));
        var cVal1 = builder.AddConstant(Value.Number(100));
        var cVal2 = builder.AddConstant(Value.Number(200));
        
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, cCond);
        builder.AddInstruction(Opcode.JumpIfFalse, 4);
        builder.AddInstruction(Opcode.PushConst, cVal2);
        builder.AddInstruction(Opcode.Jump, 5);
        builder.AddInstruction(Opcode.PushConst, cVal1);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();
        
        // Act
        var vm = RunAndGetVM(bytecode);
        
        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Number(expected), vm.OperandStack.Last());
    }
}
