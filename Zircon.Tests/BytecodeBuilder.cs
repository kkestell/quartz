namespace Zircon.Tests;

using System.Reflection;

// A helper class to programmatically build Bytecode objects for testing.
// This avoids the need to create and read from physical .bcv files during tests.
public class BytecodeBuilder
{
    private readonly List<Value> _constants = new();
    private readonly List<Function> _functions = new();
    private List<Instruction>? _currentFunctionInstructions;
    private int _currentFunctionNumArgs;

    // Adds a constant value to the program's constant pool.
    // Returns the index of the newly added constant.
    public ushort AddConstant(Value value)
    {
        var existingIndex = _constants.IndexOf(value);
        if (existingIndex != -1)
        {
            return (ushort)existingIndex;
        }
        _constants.Add(value);
        return (ushort)(_constants.Count - 1);
    }

    // Begins the definition of a new function.
    public void StartFunction(int numArgs = 0)
    {
        if (_currentFunctionInstructions != null)
        {
            throw new InvalidOperationException("Cannot start a new function while another is being defined. Call EndFunction first.");
        }
        _currentFunctionInstructions = new List<Instruction>();
        _currentFunctionNumArgs = numArgs;
    }

    // Adds an instruction to the current function being built.
    public void AddInstruction(Opcode opcode, ushort? operand = null)
    {
        if (_currentFunctionInstructions == null)
        {
            throw new InvalidOperationException("Cannot add an instruction outside of a function definition. Call StartFunction first.");
        }

        if (opcode.HasOperand() && !operand.HasValue)
            throw new ArgumentException($"Opcode {opcode} requires an operand, but none was provided.");
        if (!opcode.HasOperand() && operand.HasValue)
            throw new ArgumentException($"Opcode {opcode} does not take an operand, but one was provided.");

        _currentFunctionInstructions.Add(new Instruction(opcode, operand));
    }

    // Finalizes the current function and adds it to the program.
    public void EndFunction()
    {
        if (_currentFunctionInstructions == null)
        {
            throw new InvalidOperationException("Cannot end a function that has not been started.");
        }

        var function = new Function(_currentFunctionInstructions, _currentFunctionNumArgs);
        _functions.Add(function);
        _currentFunctionInstructions = null; // Reset for the next function
    }

    // Constructs the final Bytecode object from the defined components.
    // This uses reflection to access the private constructor of the Bytecode class.
    public Bytecode Build()
    {
        if (_currentFunctionInstructions != null)
        {
            throw new InvalidOperationException("A function is still being defined. Call EndFunction before building.");
        }

        // The Bytecode constructor is private, so we must use reflection to create an instance for testing.
        var constructor = typeof(Bytecode).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(List<Function>), typeof(List<Value>) },
            null
        );

        if (constructor == null)
        {
            throw new InvalidOperationException("Could not find the private constructor for the Bytecode class.");
        }

        return (Bytecode)constructor.Invoke(new object[] { _functions, _constants });
    }
}
