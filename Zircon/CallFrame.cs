namespace Zircon;

public class CallFrame(int functionIndex)
{
    public int InstructionPointer { get; set; }
    public int FunctionIndex { get; } = functionIndex;
    public List<Value> OperandStack { get; } = [];
    public Dictionary<int, Value> Locals { get; } = new();

    public void StackPush(Value value)
    {
        OperandStack.Add(value);
    }

    public Value StackPop()
    {
        if (OperandStack.Count == 0)
        {
            throw new InvalidOperationException("Operand stack underflow.");
        }

        var value = OperandStack[^1];
        OperandStack.RemoveAt(OperandStack.Count - 1);
        return value;
    }

    public Value StackPeek()
    {
        return OperandStack.Count > 0 ? OperandStack[^1] : throw new InvalidOperationException("Operand stack is empty.");
    }

    public bool IsStackEmpty()
    {
        return OperandStack.Count == 0;
    }
}