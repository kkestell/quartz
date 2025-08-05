using System.Text;
using Zircon;
using ValueType = Zircon.ValueType;

namespace Quartz;

public class BytecodeGenerator
{
    private readonly IrProgram _program;
    private readonly List<Value> _constants = new();
    private readonly Dictionary<Value, ushort> _constantMap = new();

    public BytecodeGenerator(IrProgram program)
    {
        _program = program;
    }

    public byte[] Generate()
    {
        var mainFunction = CompileFunction(_program.MainFunction);
        var functions = new List<Function> { mainFunction };
        
        foreach (var irFunc in _program.Functions)
        {
            functions.Add(CompileFunction(irFunc));
        }

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        WriteHeader(writer);
        WriteConstants(writer);
        WriteFunctions(writer, functions);

        return stream.ToArray();
    }

    private Function CompileFunction(IrFunction irFunc)
    {
        var instructions = new List<Instruction>();
        var locals = new Dictionary<string, ushort>();
        ushort localCounter = 0;
        
        var labelAddresses = new Dictionary<string, ushort>();

        ushort GetLocalSlot(string name)
        {
            if (!locals.TryGetValue(name, out var slot))
            {
                slot = localCounter++;
                locals[name] = slot;
            }
            return slot;
        }

        ushort instructionCount = 0;
        foreach (var ir in irFunc.Body)
        {
            if (ir.Op == IrOp.Label)
            {
                labelAddresses[(string)ir.Arg1!] = instructionCount;
                continue; // Labels don't generate instructions themselves.
            }

            instructionCount += ir.Op switch
            {
                IrOp.Copy => 2, // GetLocal, SetLocal
                IrOp.LoadConst => 2, // PushConst, SetLocal
                IrOp.Add or IrOp.Sub or IrOp.Mul or IrOp.Div or IrOp.Mod or IrOp.Eq or IrOp.Gt or IrOp.Lt or IrOp.Gte or IrOp.Lte => 4, // GetLocal, GetLocal, Op, SetLocal
                IrOp.Jump => 1, // Jump
                IrOp.JumpIfFalse => 2, // GetLocal, JumpIfFalse
                IrOp.Return => (ushort)(ir.Arg1 != null ? 2 : 1), // [GetLocal], Return
                _ => throw new NotImplementedException($"Address calculation for {ir.Op} not implemented.")
            };
        }

        foreach (var ir in irFunc.Body)
        {
            if (ir.Op == IrOp.Label) continue;

            switch (ir.Op)
            {
                case IrOp.Copy:
                {
                    var destSlot = GetLocalSlot((string)ir.Arg1!);
                    var sourceSlot = GetLocalSlot((string)ir.Arg2!);
                    instructions.Add(new Instruction(Opcode.GetLocal, sourceSlot));
                    instructions.Add(new Instruction(Opcode.SetLocal, destSlot));
                    break;
                }
                case IrOp.LoadConst:
                {
                    var tempName = (string)ir.Arg1!;
                    var value = (Value)ir.Arg2!;
                    var constIndex = AddConstant(value);
                    var localSlot = GetLocalSlot(tempName);
                    instructions.Add(new Instruction(Opcode.PushConst, constIndex));
                    instructions.Add(new Instruction(Opcode.SetLocal, localSlot));
                    break;
                }
                case IrOp.Add or IrOp.Sub or IrOp.Mul or IrOp.Div or IrOp.Mod or IrOp.Eq or IrOp.Gt or IrOp.Lt or IrOp.Gte or IrOp.Lte:
                {
                    var op = ir.Op switch {
                        IrOp.Add => Opcode.Add, IrOp.Sub => Opcode.Subtract, IrOp.Mul => Opcode.Multiply,
                        IrOp.Div => Opcode.Divide, IrOp.Mod => Opcode.Modulo, IrOp.Eq => Opcode.Equal,
                        IrOp.Gt => Opcode.GreaterThan, IrOp.Lt => Opcode.LessThan,
                        IrOp.Gte => Opcode.GreaterThanOrEqual, IrOp.Lte => Opcode.LessThanOrEqual,
                        _ => throw new InvalidOperationException()
                    };
                    
                    var destSlot = GetLocalSlot((string)ir.Arg1!);
                    var leftSlot = GetLocalSlot((string)ir.Arg2!);
                    var rightSlot = GetLocalSlot((string)ir.Arg3!);
                    
                    instructions.Add(new Instruction(Opcode.GetLocal, leftSlot));
                    instructions.Add(new Instruction(Opcode.GetLocal, rightSlot));
                    instructions.Add(new Instruction(op, null));
                    instructions.Add(new Instruction(Opcode.SetLocal, destSlot));
                    break;
                }
                case IrOp.Jump:
                {
                    var targetAddress = labelAddresses[(string)ir.Arg1!];
                    instructions.Add(new Instruction(Opcode.Jump, targetAddress));
                    break;
                }
                case IrOp.JumpIfFalse:
                {
                    var conditionSlot = GetLocalSlot((string)ir.Arg1!);
                    var targetAddress = labelAddresses[(string)ir.Arg2!];
                    instructions.Add(new Instruction(Opcode.GetLocal, conditionSlot));
                    instructions.Add(new Instruction(Opcode.JumpIfFalse, targetAddress));
                    break;
                }
                case IrOp.Return:
                {
                    if (ir.Arg1 != null)
                    {
                        var localSlot = GetLocalSlot((string)ir.Arg1);
                        instructions.Add(new Instruction(Opcode.GetLocal, localSlot));
                    }
                    instructions.Add(new Instruction(Opcode.Return, null));
                    break;
                }
                default:
                     throw new NotImplementedException($"IR operation {ir.Op} not supported in BytecodeGenerator.");
            }
        }
        
        var lastOp = instructions.LastOrDefault()?.Opcode;
        if (irFunc.Name == "__main" && lastOp != Opcode.Return && lastOp != Opcode.Halt)
        {
             instructions.Add(new Instruction(Opcode.Halt, null));
        }

        return new Function(instructions, irFunc.Parameters.Count);
    }

    private void WriteHeader(BinaryWriter writer)
    {
        writer.Write(Encoding.ASCII.GetBytes("ZRCN")); // Magic number
        writer.Write((byte)1); // Version
    }

    private void WriteConstants(BinaryWriter writer)
    {
        writer.Write((uint)_constants.Count);
        foreach (var constant in _constants)
        {
            switch (constant.Type)
            {
                case ValueType.Number:
                    writer.Write((byte)0x01);
                    writer.Write(constant.AsNumber());
                    break;
                case ValueType.Boolean:
                    writer.Write((byte)0x02);
                    writer.Write(constant.AsBoolean());
                    break;
                case ValueType.String:
                    writer.Write((byte)0x03);
                    var bytes = Encoding.UTF8.GetBytes(constant.AsString());
                    writer.Write((ushort)bytes.Length);
                    writer.Write(bytes);
                    break;
                default:
                    throw new IOException($"Unsupported constant type: {constant.Type}");
            }
        }
    }

    private void WriteFunctions(BinaryWriter writer, List<Function> functions)
    {
        writer.Write((uint)functions.Count);
        foreach (var function in functions)
        {
            writer.Write(function.NumArgs);
            writer.Write((uint)function.Instructions.Count);
            foreach (var instruction in function.Instructions)
            {
                writer.Write((byte)instruction.Opcode);
                if (instruction.Opcode.HasOperand())
                {
                    writer.Write(instruction.GetOperand());
                }
            }
        }
    }

    private ushort AddConstant(Value value)
    {
        if (!_constantMap.TryGetValue(value, out var index))
        {
            index = (ushort)_constants.Count;
            _constants.Add(value);
            _constantMap[value] = index;
        }
        return index;
    }
}
