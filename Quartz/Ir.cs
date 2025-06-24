namespace Quartz;

public enum IrOp
{
    // Arithmetic & Logic
    Add, Sub, Mul, Div, Mod, Neg, Not,

    // Comparison
    Eq, Gt, Lt, Gte, Lte,

    // Data Movement & Memory
    LoadConst,   // result = constant
    Copy,        // dest = source

    // Control Flow
    Label,       // name:
    Jump,        // goto name
    JumpIfFalse, // if !condition goto name

    // Functions
    Call,        // result = callee(args...)
    Return,      // return value
    
    // I/O
    Print
}

public record IrInstruction(IrOp Op, object? Arg1 = null, object? Arg2 = null, object? Arg3 = null);

public class IrFunction(string name, List<string> parameters)
{
    public string Name { get; } = name;
    public List<string> Parameters { get; } = parameters;
    public List<IrInstruction> Body { get; } = new();
}

public class IrProgram
{
    public List<IrFunction> Functions { get; } = new();
    public IrFunction MainFunction { get; } = new ("__main", new List<string>());
}
