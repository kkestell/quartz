namespace Zircon.Tests;

public class MemoryTests : TestBase
{
    // Memory Operations (0x90 - 0x9F)
    [Fact]
    public void Test_Opcode_Alloc_And_Free()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var size = builder.AddConstant(Value.Number(10));
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, size);
        builder.AddInstruction(Opcode.Alloc); // Allocates, pushes address
        builder.AddInstruction(Opcode.Dup);      // Duplicate address for later free
        builder.AddInstruction(Opcode.Free);     // Free the allocation
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack); // Address should be left on stack after free
        var addressVal = vm.OperandStack.Last();
        Assert.Equal(ValueType.HeapRef, addressVal.Type);
        
        var address = addressVal.AsHeapRef();
        Assert.NotNull(vm.Heap);
        Assert.True(address < vm.Heap.Count);
        Assert.Null(vm.Heap[address]); // Should be nulled out after free
        Assert.Contains(address, vm.FreeList);
    }

    [Fact]
    public void Test_Opcode_Store_And_Load()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var size = builder.AddConstant(Value.Number(5));
        var index = builder.AddConstant(Value.Number(3));
        var value = builder.AddConstant(Value.Number(99));

        builder.StartFunction();
        // Allocate memory
        builder.AddInstruction(Opcode.PushConst, size);
        builder.AddInstruction(Opcode.Alloc); // Stack: [address]
        
        // Store value
        builder.AddInstruction(Opcode.Dup); // Stack: [address, address]
        builder.AddInstruction(Opcode.PushConst, index); // Stack: [address, address, index]
        builder.AddInstruction(Opcode.PushConst, value); // Stack: [address, address, index, value]
        builder.AddInstruction(Opcode.Store); // Stack: [address]

        // Load value
        builder.AddInstruction(Opcode.PushConst, index); // Stack: [address, index]
        builder.AddInstruction(Opcode.Load); // Stack: [value]
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);

        // Assert
        Assert.NotNull(vm.OperandStack);
        Assert.Single(vm.OperandStack);

        var result = vm.OperandStack.Last();
        Assert.Equal(Value.Number(99), result);
    }
    
    [Fact]
    public void Test_Opcode_Load_OutOfBounds_ThrowsException()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var size = builder.AddConstant(Value.Number(5));
        var index = builder.AddConstant(Value.Number(10)); // Out of bounds

        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, size);
        builder.AddInstruction(Opcode.Alloc);
        builder.AddInstruction(Opcode.PushConst, index);
        builder.AddInstruction(Opcode.Load);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();
        
        var vm = new VirtualMachine(bytecode);

        // Act & Assert
        Assert.Throws<VirtualMachineException>(() => vm.Run());
    }

    [Fact]
    public void Test_Opcode_Store_OutOfBounds_ThrowsException()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        var size = builder.AddConstant(Value.Number(2));
        var index = builder.AddConstant(Value.Number(2)); // Out of bounds
        var value = builder.AddConstant(Value.Number(1));

        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, size);
        builder.AddInstruction(Opcode.Alloc);
        builder.AddInstruction(Opcode.PushConst, index);
        builder.AddInstruction(Opcode.PushConst, value);
        builder.AddInstruction(Opcode.Store);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();
        var bytecode = builder.Build();

        var vm = new VirtualMachine(bytecode);

        // Act & Assert
        Assert.Throws<VirtualMachineException>(() => vm.Run());
    }

    [Fact]
    public void Test_Opcode_Load_InvalidAddress_ThrowsException()
    {
        // Arrange
        var builder = new BytecodeBuilder();
        // NOTE: We can't just create a HeapRef constant, we must allocate and then free.
        var size = builder.AddConstant(Value.Number(1));
        var index = builder.AddConstant(Value.Number(0));

        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, size);
        builder.AddInstruction(Opcode.Alloc); // address 0
        builder.AddInstruction(Opcode.Free);  // address 0 is now invalid
        
        // now, try to load from it
        builder.AddInstruction(Opcode.PushConst, builder.AddConstant(Value.HeapRef(0)));
        builder.AddInstruction(Opcode.PushConst, index);
        builder.AddInstruction(Opcode.Load);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();

        var bytecode = builder.Build();

        var vm = new VirtualMachine(bytecode);

        // Act & Assert
        Assert.Throws<VirtualMachineException>(() => vm.Run());
    }
    
    [Fact]
    public void Test_Scenario_SumMemory()
    {
        // Arrange
        var builder = new BytecodeBuilder();

        // Constants
        var cSize = builder.AddConstant(Value.Number(10));
        var c0 = builder.AddConstant(Value.Number(0));
        var c1 = builder.AddConstant(Value.Number(1));
        
        // Locals: 0=address, 1=i, 2=sum
        builder.StartFunction();
        
        // Allocate Memory
        builder.AddInstruction(Opcode.PushConst, cSize);
        builder.AddInstruction(Opcode.Alloc);
        builder.AddInstruction(Opcode.SetLocal, 0);

        // Fill Loop (i = 0; i < 10; i++)
        builder.AddInstruction(Opcode.PushConst, c0);
        builder.AddInstruction(Opcode.SetLocal, 1);
        
        // loop_fill_start: (IP 5)
        builder.AddInstruction(Opcode.GetLocal, 1);
        builder.AddInstruction(Opcode.PushConst, cSize);
        builder.AddInstruction(Opcode.LessThan);
        builder.AddInstruction(Opcode.JumpIfFalse, 20);

        // Body: heap[address][i] = i + 1
        builder.AddInstruction(Opcode.GetLocal, 0);
        builder.AddInstruction(Opcode.GetLocal, 1);
        builder.AddInstruction(Opcode.GetLocal, 1);
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.Add);
        builder.AddInstruction(Opcode.Store);

        // i++
        builder.AddInstruction(Opcode.GetLocal, 1);
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.Add);
        builder.AddInstruction(Opcode.SetLocal, 1);
        builder.AddInstruction(Opcode.Jump, 5);

        // Sum Loop (sum = 0; i = 0; i < 10; i++)
        // (IP 20)
        builder.AddInstruction(Opcode.PushConst, c0);
        builder.AddInstruction(Opcode.SetLocal, 2);
        builder.AddInstruction(Opcode.PushConst, c0);
        builder.AddInstruction(Opcode.SetLocal, 1);

        // loop_sum_start: (IP 24)
        builder.AddInstruction(Opcode.GetLocal, 1);
        builder.AddInstruction(Opcode.PushConst, cSize);
        builder.AddInstruction(Opcode.LessThan);
        builder.AddInstruction(Opcode.JumpIfFalse, 39); // Jump past the loop to the return logic

        // Body: sum = sum + heap[address][i]
        builder.AddInstruction(Opcode.GetLocal, 2);
        builder.AddInstruction(Opcode.GetLocal, 0);
        builder.AddInstruction(Opcode.GetLocal, 1);
        builder.AddInstruction(Opcode.Load);
        builder.AddInstruction(Opcode.Add);
        builder.AddInstruction(Opcode.SetLocal, 2);

        // i++
        builder.AddInstruction(Opcode.GetLocal, 1);
        builder.AddInstruction(Opcode.PushConst, c1);
        builder.AddInstruction(Opcode.Add);
        builder.AddInstruction(Opcode.SetLocal, 1);
        builder.AddInstruction(Opcode.Jump, 24);

        // End: Return sum
        // (IP 39)
        builder.AddInstruction(Opcode.GetLocal, 2);
        builder.AddInstruction(Opcode.Return); // Use Return to set VM's ReturnValue
        builder.EndFunction();

        var bytecode = builder.Build();

        // Act
        var vm = RunAndGetVM(bytecode);
        
        // Assert
        Assert.NotNull(vm.ReturnValue);
        Assert.Equal(Value.Number(55), vm.ReturnValue);
    }
}
