using Quartz;
using Zircon;

/// <summary>
/// A test harness for the Quartz compiler and Zircon VM.
/// This program demonstrates the end-to-end compilation and execution
/// of a program with an if-else statement, and includes debug printing.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        // 1. Define the program to compile using the AST.
        // This is equivalent to:
        // main() {
        //   var x = 100;
        //   if (x > 50) {
        //     return 1;
        //   } else {
        //     return 0;
        //   }
        // }
        var ast = new List<Statement>
        {
            new FunctionDeclStmt(
                name: "main",
                parameters: [],
                body: new BlockStmt(
                    [
                        new VarDeclStmt("x", new LiteralExpr(Value.Number(100))),
                        new IfStmt(
                            new BinaryExpr(new VariableExpr("x"), ">", new LiteralExpr(Value.Number(50))),
                            new BlockStmt([new ReturnStmt(new LiteralExpr(Value.Number(1)))]),
                            new BlockStmt([new ReturnStmt(new LiteralExpr(Value.Number(0)))])
                        )
                    ]
                )
            )
        };
        DebugHelpers.PrintAst(ast);

        // 2. Lower the AST to Intermediate Representation (IR).
        var irGenerator = new IrGenerator(ast);
        var irProgram = irGenerator.Generate();
        DebugHelpers.PrintIr(irProgram);

        // 3. Generate Zircon bytecode from the IR.
        var bytecodeGenerator = new BytecodeGenerator(irProgram);
        var bytecodeBytes = bytecodeGenerator.Generate();
        
        // 4. Execute the bytecode in the Zircon VM.
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, bytecodeBytes);
            var zirconBytecode = Bytecode.FromFile(tempFile);
            
            DebugHelpers.Disassemble(zirconBytecode);

            var vm = new VirtualMachine(zirconBytecode);
            Console.WriteLine("\n=== Step 4: Executing in Zircon VM ===\n");
            vm.Run();

            // 5. Print the result.
            if (vm.ReturnValue != null)
            {
                Console.WriteLine($"VM exited with return value: {vm.ReturnValue}");
            }
            else
            {
                Console.WriteLine("VM exited with no return value.");
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
