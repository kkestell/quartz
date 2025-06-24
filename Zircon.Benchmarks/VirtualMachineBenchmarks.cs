using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Zircon.Tests;

namespace Zircon.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class VirtualMachineBenchmarks
{
    private Bytecode _fibonacciBytecode;

    [GlobalSetup]
    public void Setup()
    {
        SetupFibonacci();
    }

    private void SetupFibonacci()
    {
        var builder = new BytecodeBuilder();

        // Constants needed for the program
        var cFibArg = builder.AddConstant(Value.Number(15));
        var cNum2 = builder.AddConstant(Value.Number(2));
        var cNum1 = builder.AddConstant(Value.Number(1));
        var funcFibIndex = (ushort)1;

        // === Function 0: main ===
        builder.StartFunction();
        builder.AddInstruction(Opcode.PushConst, cFibArg);
        builder.AddInstruction(Opcode.Call, funcFibIndex);
        builder.AddInstruction(Opcode.Halt);
        builder.EndFunction();

        // === Function 1: fib(n) ===
        builder.StartFunction(numArgs: 1);
        // Base case check: if (n < 2)
        builder.AddInstruction(Opcode.GetLocal, 0);
        builder.AddInstruction(Opcode.PushConst, cNum2);
        builder.AddInstruction(Opcode.LessThan);
        builder.AddInstruction(Opcode.JumpIfFalse, 6);

        // Base case body: return n
        builder.AddInstruction(Opcode.GetLocal, 0);
        builder.AddInstruction(Opcode.Return);

        // Recursive step: return fib(n-1) + fib(n-2)
        builder.AddInstruction(Opcode.GetLocal, 0);
        builder.AddInstruction(Opcode.PushConst, cNum1);
        builder.AddInstruction(Opcode.Subtract);
        builder.AddInstruction(Opcode.Call, funcFibIndex);

        builder.AddInstruction(Opcode.GetLocal, 0);
        builder.AddInstruction(Opcode.PushConst, cNum2);
        builder.AddInstruction(Opcode.Subtract);
        builder.AddInstruction(Opcode.Call, funcFibIndex);

        builder.AddInstruction(Opcode.Add);
        builder.AddInstruction(Opcode.Return);
        builder.EndFunction();

        _fibonacciBytecode = builder.Build();
    }

    [Benchmark(Description = "Recursive Fibonacci(15)")]
    public void RecursiveFibonacci()
    {
        var vm = new VirtualMachine(_fibonacciBytecode);
        vm.Run();
    }
}
