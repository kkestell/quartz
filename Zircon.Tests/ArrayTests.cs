namespace Zircon.Tests;

public class ArrayTests : TestBase
{
    // Array Operations

    [Fact]
    public void Test_Opcode_NewArray()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var sizeConst = builder.AddConstant(Value.Number(10));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, sizeConst);
        builder.AddInstruction(Opcode.NewArray);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        var result = Assert.Single(vm.OperandStack);
        Assert.Equal(ValueType.Array, result.Type);
        Assert.Equal(10, result.AsArray().Length);
        Assert.All(result.AsArray(), item => Assert.Equal(Value.Nil(), item));
    }
    
    [Fact]
    public void Test_Opcode_SetElement()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var sizeConst = builder.AddConstant(Value.Number(5));
        var indexConst = builder.AddConstant(Value.Number(2));
        var valueConst = builder.AddConstant(Value.Str("test"));
        
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, sizeConst);
        builder.AddInstruction(Opcode.NewArray); // stack: [array]
        builder.AddInstruction(Opcode.Dup);       // stack: [array, array]
        builder.AddInstruction(Opcode.PushConst, indexConst); // stack: [array, array, 2]
        builder.AddInstruction(Opcode.PushConst, valueConst); // stack: [array, array, 2, "test"]
        builder.AddInstruction(Opcode.SetElement); // consumes top 3, leaves array
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        var result = Assert.Single(vm.OperandStack);
        Assert.Equal(ValueType.Array, result.Type);
        var array = result.AsArray();
        Assert.Equal(5, array.Length);
        Assert.Equal(Value.Str("test"), array[2]);
    }

    [Fact]
    public void Test_Opcode_GetElement()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var sizeConst = builder.AddConstant(Value.Number(3));
        var indexConst = builder.AddConstant(Value.Number(1));
        var valueConst = builder.AddConstant(Value.Number(99));
        
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, sizeConst);
        builder.AddInstruction(Opcode.NewArray);
        builder.AddInstruction(Opcode.Dup);
        builder.AddInstruction(Opcode.PushConst, indexConst);
        builder.AddInstruction(Opcode.PushConst, valueConst);
        builder.AddInstruction(Opcode.SetElement); // setup complete
        
        builder.AddInstruction(Opcode.PushConst, indexConst); // push index again
        builder.AddInstruction(Opcode.GetElement);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();
        
        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        var result = Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Number(99), result);
    }
    
    [Fact]
    public void Test_Opcode_ArrayLength()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var sizeConst = builder.AddConstant(Value.Number(42));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, sizeConst);
        builder.AddInstruction(Opcode.NewArray);
        builder.AddInstruction(Opcode.ArrayLength);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();
        
        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        var result = Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Number(42), result);
    }
}
