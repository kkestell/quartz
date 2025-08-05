using System.Text;

namespace Zircon;

public class VirtualMachineException(
    string message,
    Exception innerException,
    IReadOnlyList<CallFrame> callStack,
    IReadOnlyDictionary<int, Value> globals)
    : Exception(message, innerException)
{
    public IReadOnlyList<CallFrame> CallStack { get; } = callStack;
    public IReadOnlyDictionary<int, Value> Globals { get; } = globals;

    public override string ToString()
    {
        var state = new StringBuilder();
        state.AppendLine($"{GetType().Name}: {Message}");
        
        if (InnerException != null)
        {
            state.AppendLine($"Inner Exception: {InnerException}");
        }

        state.AppendLine();
        state.AppendLine("--- VM STATE DUMP ---");

        // Call Stack
        state.AppendLine("--- Call Stack ---");
        if (CallStack.Count == 0)
        {
            state.AppendLine("  <empty>");
        }
        else
        {
            for (var i = CallStack.Count - 1; i >= 0; i--)
            {
                var frame = CallStack[i];
                state.AppendLine($"  - Frame {i}: Function {frame.FunctionIndex} at IP {frame.InstructionPointer}");
                
                // Operand Stack
                state.AppendLine("    Operand Stack:");
                if (frame.OperandStack.Count == 0) 
                    state.AppendLine("      <empty>");
                else
                {
                    for (var j = 0; j < frame.OperandStack.Count; j++)
                    {
                        state.AppendLine($"      [{j}] {frame.OperandStack[j]}");
                    }
                }

                // Locals
                state.AppendLine("    Locals:");
                if (frame.Locals.Count == 0) 
                    state.AppendLine("      <empty>");
                else
                {
                    foreach (var (key, value) in frame.Locals)
                    {
                        state.AppendLine($"      [{key}] = {value}");
                    }
                }
            }
        }
        
        // Globals
        state.AppendLine("--- Globals ---");
        if (Globals.Count == 0) 
            state.AppendLine("  <empty>");
        else
        {
            foreach (var (key, value) in Globals)
            {
                state.AppendLine($"  [{key}] = {value}");
            }
        }

        state.AppendLine("--- END OF DUMP ---");
        return state.ToString();
    }
}