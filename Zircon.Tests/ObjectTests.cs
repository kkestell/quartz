namespace Zircon.Tests;

public class ObjectTests : TestBase
{
    // Object Operations

    [Fact]
    public void Test_Opcode_NewObject()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        builder.StartFunction();
        builder.AddInstruction(Opcode.NewObject);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        var result = Assert.Single(vm.OperandStack);
        Assert.Equal(ValueType.Object, result.Type);
        Assert.Empty(result.AsObject());
    }

    [Fact]
    public void Test_Opcode_SetAndGetProperty()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var propNameConst = builder.AddConstant(Value.Str("name"));
        var propValueConst = builder.AddConstant(Value.Str("Zircon"));

        builder.StartFunction();
        builder.AddInstruction(Opcode.NewObject); // stack: [obj]
        builder.AddInstruction(Opcode.Dup);       // stack: [obj, obj]
        builder.AddInstruction(Opcode.PushConst, propValueConst); // stack: [obj, obj, "Zircon"]
        builder.AddInstruction(Opcode.SetProperty, propNameConst); // consumes top 2
        // stack is now [obj]
        builder.AddInstruction(Opcode.GetProperty, propNameConst);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);
        
        // Assert
        Assert.NotNull(vm.OperandStack);
        var result = Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Str("Zircon"), result);
    }

    [Fact]
    public void Test_Opcode_GetProperty_NotFound()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var propNameConst = builder.AddConstant(Value.Str("non_existent"));

        builder.StartFunction();
        builder.AddInstruction(Opcode.NewObject);
        builder.AddInstruction(Opcode.GetProperty, propNameConst);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);
        
        // Assert
        Assert.NotNull(vm.OperandStack);
        var result = Assert.Single(vm.OperandStack);
        Assert.Equal(Value.Nil(), result);
    }
}
