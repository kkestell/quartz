namespace Zircon;

// Exception thrown when the VM encounters a fatal error during execution

public class VirtualMachine
{
    private readonly Bytecode _bytecode;
    private readonly List<CallFrame> _frames = [];
    private readonly Dictionary<int, Value> _globals = new();
    private readonly List<Value[]?> _heap = [];
    private readonly Stack<int> _freeList = new();
    private bool _isRunning = true;
    
    public Value? ReturnValue { get; private set; }
    
    // Instrumentation properties for debugging and introspection
    public IReadOnlyList<CallFrame> Frames => _frames;
    public IReadOnlyList<Value>? OperandStack => !IsCallStackEmpty() ? CurrentFrame().OperandStack : null;
    public IReadOnlyDictionary<int, Value>? Locals => !IsCallStackEmpty() ? CurrentFrame().Locals : null;
    public IReadOnlyDictionary<int, Value> Globals => _globals;
    public bool IsRunning => _isRunning;
    public IReadOnlyList<Value[]?> Heap => _heap;
    public IReadOnlyCollection<int> FreeList => _freeList;
    
    public VirtualMachine(Bytecode bytecode)
    {
        _bytecode = bytecode;
    }

    // Push a new call frame onto the call stack
    private void PushFrame(CallFrame frame)
    {
        _frames.Add(frame);
    }

    // Pop the current call frame from the call stack
    private void PopFrame()
    {
        if (_frames.Count > 0)
        {
            _frames.RemoveAt(_frames.Count - 1);
        }
        else
        {
            _isRunning = false;
        }
    }

    // Get the current call frame (top of the call stack)
    private CallFrame CurrentFrame()
    {
        return _frames.Count > 0 ? _frames[^1] : throw new InvalidOperationException("Call stack is empty.");
    }

    // Check if the call stack is empty
    private bool IsCallStackEmpty()
    {
        return _frames.Count == 0;
    }

    // Push a value onto the current frame's operand stack
    private void PushOperand(Value value)
    {
        CurrentFrame().StackPush(value);
    }

    // Pop a value from the current frame's operand stack
    private Value PopOperand()
    {
        return CurrentFrame().StackPop();
    }

    // Main execution loop of the Virtual Machine
    public void Run()
    {
        PushFrame(new CallFrame(0));

        try
        {
            while (!IsCallStackEmpty() && _isRunning)
            {
                var frame = CurrentFrame();
                var function = _bytecode.GetFunction(frame.FunctionIndex);
                
                // Check if we've reached the end of the function
                if (frame.InstructionPointer >= function.Instructions.Count)
                {
                    // Implicit return when falling off the end of a function
                    HandleReturn();
                    continue;
                }

                var instruction = function.GetInstruction(frame.InstructionPointer++);
                var opcode = instruction.Opcode;

                switch (opcode)
                {
                    case Opcode.PushConst: 
                        PushOperand(_bytecode.GetConstant(instruction.GetOperand())); 
                        break;
                        
                    case Opcode.PushNil: 
                        PushOperand(Value.Nil()); 
                        break;
                        
                    case Opcode.Pop: 
                        PopOperand(); 
                        break;
                        
                    case Opcode.Dup: 
                        PushOperand(CurrentFrame().StackPeek()); 
                        break;
                        
                    case Opcode.Swap:
                    {
                        var a = PopOperand();
                        var b = PopOperand();
                        PushOperand(a);
                        PushOperand(b);
                        break;
                    }
                    
                    // Binary operations
                    case Opcode.Add:
                    case Opcode.Subtract:
                    case Opcode.Multiply:
                    case Opcode.Divide:
                    case Opcode.Modulo:
                    case Opcode.And:
                    case Opcode.Or:
                    case Opcode.GreaterThan:
                    case Opcode.LessThan:
                    case Opcode.GreaterThanOrEqual:
                    case Opcode.LessThanOrEqual: 
                        BinaryOp(opcode); 
                        break;
                        
                    // Unary operations
                    case Opcode.Not:
                    case Opcode.Negate: 
                        UnaryOp(opcode); 
                        break;
                        
                    case Opcode.Equal:
                    {
                        var b = PopOperand();
                        var a = PopOperand();
                        PushOperand(Value.Boolean(a.Equals(b)));
                        break;
                    }
                    
                    // Control flow
                    case Opcode.Jump: 
                        frame.InstructionPointer = instruction.GetOperand();
                        break;
                        
                    case Opcode.JumpIfTrue:
                        if (PopOperand().AsBoolean())
                        {
                            frame.InstructionPointer = instruction.GetOperand();
                        }
                        break;
                        
                    case Opcode.JumpIfFalse:
                        if (!PopOperand().AsBoolean())
                        {
                            frame.InstructionPointer = instruction.GetOperand();
                        }
                        break;
                        
                    case Opcode.Print: 
                        var printedValue = PopOperand();
                        Console.WriteLine(printedValue); 
                        break;
                        
                    // Variable access
                    case Opcode.GetLocal: 
                        PushOperand(frame.Locals[instruction.GetOperand()]); 
                        break;
                        
                    case Opcode.SetLocal: 
                        frame.Locals[instruction.GetOperand()] = PopOperand(); 
                        break;
                        
                    case Opcode.GetGlobal: 
                        PushOperand(_globals[instruction.GetOperand()]); 
                        break;
                        
                    case Opcode.SetGlobal: 
                        _globals[instruction.GetOperand()] = PopOperand(); 
                        break;
                        
                    // Function calls and returns
                    case Opcode.Call: 
                        HandleFunctionCall(instruction.GetOperand()); 
                        break;
                        
                    case Opcode.Return: 
                        HandleReturn(); 
                        break;
                        
                    // Memory management
                    case Opcode.Alloc:
                    {
                        var sizeValue = PopOperand();
                        if (sizeValue.Type != ValueType.Number) 
                            throw new InvalidOperationException("Alloc size must be a number.");
                        
                        var size = (int)sizeValue.AsNumber();
                        if (size < 0) 
                            throw new InvalidOperationException("Alloc size cannot be negative.");

                        var address = Allocate(size);
                        PushOperand(Value.HeapRef(address));
                        break;
                    }
                    
                    case Opcode.Store:
                    {
                        var value = PopOperand();
                        var indexValue = PopOperand();
                        var addressValue = PopOperand();
                        
                        if (indexValue.Type != ValueType.Number || addressValue.Type != ValueType.HeapRef)
                            throw new InvalidOperationException("Invalid types for Store operation.");
                            
                        var index = (int)indexValue.AsNumber();
                        var address = addressValue.AsHeapRef();
                        
                        HeapStore(address, index, value);
                        break;
                    }
                    
                    case Opcode.Load:
                    {
                        var indexValue = PopOperand();
                        var addressValue = PopOperand();
                        
                        if (indexValue.Type != ValueType.Number || addressValue.Type != ValueType.HeapRef)
                            throw new InvalidOperationException("Invalid types for Load operation.");

                        var index = (int)indexValue.AsNumber();
                        var address = addressValue.AsHeapRef();
                        
                        var value = HeapLoad(address, index);
                        PushOperand(value);
                        break;
                    }
                    
                    case Opcode.Free:
                    {
                        var addressValue = PopOperand();
                        if (addressValue.Type != ValueType.HeapRef)
                            throw new InvalidOperationException("Can only free a heap reference.");
                        
                        Free(addressValue.AsHeapRef());
                        break;
                    }
                    
                    case Opcode.Halt:
                        _isRunning = false; 
                        break;
                        
                    default: 
                        throw new InvalidOperationException($"Unhandled opcode: {opcode}");
                }
            }
        }
        catch (Exception ex)
        {
            // Create a snapshot of the VM state and throw a custom exception
            var framesCopy = _frames.ToList();
            var globalsCopy = new Dictionary<int, Value>(_globals);
            var heapCopy = _heap.ToList();
            var freeListCopy = _freeList.ToList();
            
            throw new VirtualMachineException(
                "A fatal error occurred in the VM during execution.", 
                ex,
                framesCopy,
                globalsCopy,
                heapCopy,
                freeListCopy);
        }
    }

    // Execute a unary operation
    private void UnaryOp(Opcode opcode)
    {
        var val = PopOperand();
        var result = opcode switch
        {
            Opcode.Not => val.LogicalNot(),
            Opcode.Negate => val.Negate(),
            _ => throw new InvalidOperationException($"Invalid opcode for unary op: {opcode}")
        };
        PushOperand(result);
    }

    // Execute a binary operation
    private void BinaryOp(Opcode opcode)
    {
        var b = PopOperand();
        var a = PopOperand();
        var result = opcode switch
        {
            Opcode.Add => a.Add(b),
            Opcode.Subtract => a.Subtract(b),
            Opcode.Multiply => a.Multiply(b),
            Opcode.Divide => a.Divide(b),
            Opcode.Modulo => a.Modulo(b),
            Opcode.And => a.LogicalAnd(b),
            Opcode.Or => a.LogicalOr(b),
            Opcode.GreaterThan => a.GreaterThan(b),
            Opcode.LessThan => a.LessThan(b),
            Opcode.GreaterThanOrEqual => a.GreaterThanOrEqual(b),
            Opcode.LessThanOrEqual => a.LessThanOrEqual(b),
            _ => throw new InvalidOperationException($"Invalid opcode for binary op: {opcode}")
        };
        PushOperand(result);
    }

    // Handle function call by setting up a new call frame
    private void HandleFunctionCall(int funcIndex)
    {
        var function = _bytecode.GetFunction(funcIndex);
        var newFrame = new CallFrame(funcIndex);
        
        // Pop arguments from the current frame and set them as locals in the new frame
        // Arguments are popped in reverse order (last argument first)
        for (var i = function.NumArgs - 1; i >= 0; i--)
        {
            newFrame.Locals[i] = PopOperand();
        }

        PushFrame(newFrame);
    }

    // Handle function return
    private void HandleReturn()
    {
        // Get return value from current frame's stack, or default to nil
        var returnValue = CurrentFrame().IsStackEmpty() ? Value.Nil() : PopOperand();
        
        PopFrame();
        
        if (!IsCallStackEmpty())
        {
            // Push return value onto the caller's stack
            PushOperand(returnValue);
        }
        else
        {
            // This was the main function, store the final return value
            ReturnValue = returnValue;
        }
    }

    // Allocate a block of memory on the heap
    private int Allocate(int size)
    {
        var block = new Value[size];
        
        // Initialize all elements to nil
        for (var i = 0; i < size; i++)
        {
            block[i] = Value.Nil();
        }
        
        // Reuse a freed address if available
        if (_freeList.Count > 0)
        {
            var address = _freeList.Pop();
            _heap[address] = block;
            return address;
        }
        
        // Otherwise allocate a new address
        _heap.Add(block);
        return _heap.Count - 1;
    }
    
    // Free a block of memory on the heap
    private void Free(int address)
    {
        if (address < 0 || address >= _heap.Count || _heap[address] == null)
            throw new InvalidOperationException("Invalid address to free.");
        
        _heap[address] = null;
        _freeList.Push(address);
    }
    
    // Store a value at a specific index in a heap-allocated block
    private void HeapStore(int address, int index, Value value)
    {
        if (address < 0 || address >= _heap.Count || _heap[address] is not { } block)
            throw new InvalidOperationException($"Invalid heap address: {address}");
            
        if (index < 0 || index >= block.Length)
            throw new IndexOutOfRangeException($"Index {index} out of bounds for allocation at address {address} with size {block.Length}.");
        
        block[index] = value;
    }
    
    // Load a value from a specific index in a heap-allocated block
    private Value HeapLoad(int address, int index)
    {
        if (address < 0 || address >= _heap.Count || _heap[address] is not { } block)
            throw new InvalidOperationException($"Invalid heap address: {address}");
        
        if (index < 0 || index >= block.Length)
            throw new IndexOutOfRangeException($"Index {index} out of bounds for allocation at address {address} with size {block.Length}.");
        
        return block[index];
    }
}
