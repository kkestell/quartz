using System.Text;

namespace Zircon;

// Operations that the virtual machine can execute
public enum Opcode : byte
{
    // Stack operations
    PushConst = 0x01,
    Pop = 0x02,
    Dup = 0x03,
    Swap = 0x04,
    PushNil = 0x05, // New opcode for nil

    // Arithmetic operations
    Add = 0x10,
    Subtract = 0x11,
    Multiply = 0x12,
    Divide = 0x13,
    Modulo = 0x14,
    Negate = 0x15,

    // Logical operations
    And = 0x20,
    Or = 0x21,
    Not = 0x22,

    // Comparison operations
    Equal = 0x30,
    GreaterThan = 0x31,
    LessThan = 0x32,
    GreaterThanOrEqual = 0x33,
    LessThanOrEqual = 0x34,

    // Control flow
    Jump = 0x40,
    JumpIfTrue = 0x41,
    JumpIfFalse = 0x42,

    // Input/output
    Print = 0x60,

    // Variable access
    GetLocal = 0x70,
    SetLocal = 0x71,
    GetGlobal = 0x72,
    SetGlobal = 0x73,

    // Function calls
    Call = 0x80,
    Return = 0x81,

    // Memory management
    Alloc = 0x90,  // Allocate heap memory
    Store = 0x91,  // Store value at memory address + offset
    Load = 0x92,   // Load value from memory address + offset
    Free = 0x93,   // Free heap memory

    // VM control
    Halt = 0xFF,
}

public static class OpcodeExtensions
{
    // Convert byte value to opcode
    public static Opcode FromByte(byte value) => value switch
    {
        0x01 => Opcode.PushConst,
        0x02 => Opcode.Pop,
        0x03 => Opcode.Dup,
        0x04 => Opcode.Swap,
        0x05 => Opcode.PushNil, // Handle new opcode
        0x10 => Opcode.Add,
        0x11 => Opcode.Subtract,
        0x12 => Opcode.Multiply,
        0x13 => Opcode.Divide,
        0x14 => Opcode.Modulo,
        0x15 => Opcode.Negate,
        0x20 => Opcode.And,
        0x21 => Opcode.Or,
        0x22 => Opcode.Not,
        0x30 => Opcode.Equal,
        0x31 => Opcode.GreaterThan,
        0x32 => Opcode.LessThan,
        0x33 => Opcode.GreaterThanOrEqual,
        0x34 => Opcode.LessThanOrEqual,
        0x40 => Opcode.Jump,
        0x41 => Opcode.JumpIfTrue,
        0x42 => Opcode.JumpIfFalse,
        0x60 => Opcode.Print,
        0x70 => Opcode.GetLocal,
        0x71 => Opcode.SetLocal,
        0x72 => Opcode.GetGlobal,
        0x73 => Opcode.SetGlobal,
        0x80 => Opcode.Call,
        0x81 => Opcode.Return,
        0x90 => Opcode.Alloc,
        0x91 => Opcode.Store,
        0x92 => Opcode.Load,
        0x93 => Opcode.Free,
        0xFF => Opcode.Halt,
        _ => throw new IOException($"Unknown opcode: {value:X2}"),
    };

    // Check if opcode requires a ushort operand
    public static bool HasOperand(this Opcode opcode) => opcode switch
    {
        Opcode.PushConst or
        Opcode.Jump or
        Opcode.JumpIfTrue or
        Opcode.JumpIfFalse or
        Opcode.GetLocal or
        Opcode.SetLocal or
        Opcode.GetGlobal or
        Opcode.SetGlobal or
        Opcode.Call => true,
        _ => false,
    };
}

// Single VM instruction with optional operand
public class Instruction
{
    public Opcode Opcode { get; }
    private readonly ushort? _operand;

    public Instruction(Opcode opcode, ushort? operand)
    {
        Opcode = opcode;
        _operand = operand;
    }

    public ushort GetOperand() => 
        _operand ?? throw new InvalidOperationException("Instruction has no operand.");
}

// Value types supported by the VM
public enum ValueType
{
    Nil, // New Nil type
    Number,
    Boolean,
    String,
    HeapRef, // Reference to heap-allocated memory
}

// Runtime value that can be stored on stack or as constant
public class Value : IEquatable<Value>
{
    public ValueType Type { get; }
    private readonly object? _rawValue;

    private static readonly Value NilInstance = new(ValueType.Nil, null);

    private Value(ValueType type, object? rawValue)
    {
        Type = type;
        _rawValue = rawValue;
    }

    // Factory methods for creating values
    public static Value Nil() => NilInstance;
    public static Value Number(double value) => new(ValueType.Number, value);
    public static Value Boolean(bool value) => new(ValueType.Boolean, value);
    public static Value Str(string value) => new(ValueType.String, value);
    public static Value HeapRef(int address) => new(ValueType.HeapRef, address);

    // Type-safe accessors
    public double AsNumber() => Type == ValueType.Number 
        ? (double)_rawValue! 
        : throw new InvalidOperationException("Value is not a number.");

    public bool AsBoolean() => Type == ValueType.Boolean 
        ? (bool)_rawValue! 
        : throw new InvalidOperationException("Value is not a boolean.");

    public string AsString() => Type == ValueType.String 
        ? (string)_rawValue! 
        : throw new InvalidOperationException("Value is not a string.");

    public int AsHeapRef() => Type == ValueType.HeapRef 
        ? (int)_rawValue! 
        : throw new InvalidOperationException("Value is not a heap reference.");

    // Arithmetic operations
    public Value Add(Value other) => 
        (Type == ValueType.Number && other.Type == ValueType.Number) 
            ? Number(AsNumber() + other.AsNumber()) 
            : throw new InvalidOperationException("Invalid operand types for addition.");

    public Value Subtract(Value other) => 
        (Type == ValueType.Number && other.Type == ValueType.Number) 
            ? Number(AsNumber() - other.AsNumber()) 
            : throw new InvalidOperationException("Invalid operand types for subtraction.");

    public Value Multiply(Value other) => 
        (Type == ValueType.Number && other.Type == ValueType.Number) 
            ? Number(AsNumber() * other.AsNumber()) 
            : throw new InvalidOperationException("Invalid operand types for multiplication.");

    public Value Divide(Value other) => 
        (Type == ValueType.Number && other.Type == ValueType.Number) 
            ? Number(AsNumber() / other.AsNumber()) 
            : throw new InvalidOperationException("Invalid operand types for division.");

    public Value Modulo(Value other) => 
        (Type == ValueType.Number && other.Type == ValueType.Number) 
            ? Number(AsNumber() % other.AsNumber()) 
            : throw new InvalidOperationException("Invalid operand types for modulo.");

    public Value Negate() => Type == ValueType.Number 
        ? Number(-AsNumber()) 
        : throw new InvalidOperationException("Invalid operand type for negation.");

    // Logical operations
    public Value LogicalAnd(Value other) => 
        (Type == ValueType.Boolean && other.Type == ValueType.Boolean) 
            ? Boolean(AsBoolean() && other.AsBoolean()) 
            : throw new InvalidOperationException("Invalid operand types for logical AND.");

    public Value LogicalOr(Value other) => 
        (Type == ValueType.Boolean && other.Type == ValueType.Boolean) 
            ? Boolean(AsBoolean() || other.AsBoolean()) 
            : throw new InvalidOperationException("Invalid operand types for logical OR.");

    public Value LogicalNot() => Type == ValueType.Boolean 
        ? Boolean(!AsBoolean()) 
        : throw new InvalidOperationException("Invalid operand type for logical NOT.");

    // Comparison operations
    public Value GreaterThan(Value other) => 
        (Type == ValueType.Number && other.Type == ValueType.Number) 
            ? Boolean(AsNumber() > other.AsNumber()) 
            : throw new InvalidOperationException("Invalid operand types for comparison.");

    public Value LessThan(Value other) => 
        (Type == ValueType.Number && other.Type == ValueType.Number) 
            ? Boolean(AsNumber() < other.AsNumber()) 
            : throw new InvalidOperationException("Invalid operand types for comparison.");

    public Value GreaterThanOrEqual(Value other) => 
        (Type == ValueType.Number && other.Type == ValueType.Number) 
            ? Boolean(AsNumber() >= other.AsNumber()) 
            : throw new InvalidOperationException("Invalid operand types for comparison.");

    public Value LessThanOrEqual(Value other) => 
        (Type == ValueType.Number && other.Type == ValueType.Number) 
            ? Boolean(AsNumber() <= other.AsNumber()) 
            : throw new InvalidOperationException("Invalid operand types for comparison.");

    // Equality implementation
    public bool Equals(Value? other)
    {
        if (other is null) 
            return false;

        if (Type == ValueType.Nil) 
            return other.Type == ValueType.Nil;
            
        if (Type != other.Type)
            return false;

        if (ReferenceEquals(this, other)) 
            return true;

        return _rawValue!.Equals(other._rawValue);
    }

    public override bool Equals(object? obj) => Equals(obj as Value);
    public override int GetHashCode() => _rawValue?.GetHashCode() ?? 0;
    public override string ToString() => Type == ValueType.Nil ? "nil" : _rawValue?.ToString() ?? "";
}

// Function definition containing instructions and metadata
public class Function(List<Instruction> instructions, int numArgs)
{
    public IReadOnlyList<Instruction> Instructions { get; } = instructions;
    public int NumArgs { get; } = numArgs;
    
    public Instruction GetInstruction(int index) => Instructions[index];
}

// Complete bytecode program containing functions and constants
public class Bytecode
{
    private readonly IReadOnlyList<Function> _functions;
    private readonly IReadOnlyList<Value> _constants;

    private Bytecode(List<Function> functions, List<Value> constants)
    {
        _functions = functions;
        _constants = constants;
    }

    // Load bytecode from file
    public static Bytecode FromFile(string path)
    {
        using var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));

        // Verify magic number and version
        var magic = reader.ReadBytes(4);
        if (Encoding.ASCII.GetString(magic) != "ZRCN") 
            throw new IOException("Invalid magic number.");
        
        if (reader.ReadByte() != 1) 
            throw new IOException("Unsupported version.");

        var constants = ReadConstants(reader);
        var functions = ReadFunctions(reader);

        return new Bytecode(functions, constants);
    }

    // Read constants table from bytecode
    private static List<Value> ReadConstants(BinaryReader reader)
    {
        var numConstants = reader.ReadUInt32();
        var constants = new List<Value>((int)numConstants);
        
        for (var i = 0; i < numConstants; i++)
        {
            var typeId = reader.ReadByte();
            var constant = typeId switch
            {
                0x01 => Value.Number(reader.ReadDouble()),
                0x02 => Value.Boolean(reader.ReadBoolean()),
                0x03 => Value.Str(Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadUInt16()))),
                // Nil is not represented in the constant pool, it's pushed by its own opcode
                _ => throw new IOException($"Unknown constant type: {typeId:X2}")
            };
            constants.Add(constant);
        }
        
        return constants;
    }

    // Read functions table from bytecode
    private static List<Function> ReadFunctions(BinaryReader reader)
    {
        var numFunctions = reader.ReadUInt32();
        var functions = new List<Function>((int)numFunctions);
        
        for (var i = 0; i < numFunctions; i++)
        {
            var numArgs = reader.ReadInt32();
            var numInstructions = reader.ReadUInt32();
            var instructions = new List<Instruction>((int)numInstructions);
            
            for (var j = 0; j < numInstructions; j++)
            {
                var opcode = OpcodeExtensions.FromByte(reader.ReadByte());
                ushort? operand = opcode.HasOperand() ? reader.ReadUInt16() : null;
                instructions.Add(new Instruction(opcode, operand));
            }
            
            functions.Add(new Function(instructions, numArgs));
        }
        
        return functions;
    }

    // Public accessors
    public Function GetFunction(int index) => _functions[index];
    public Value GetConstant(int index) => _constants[index];
}
