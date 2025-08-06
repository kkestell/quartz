using Quartz;
using Zircon;

public static class Program
{
    public static void Main(string[] args)
    {
        // AST

        /*
        main() {
          var x = 100;
          if (x > 50) {
            return 1;
          } else {
            return 0;
          }
        }
        */
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

        // IR

        var irGenerator = new IrGenerator(ast);
        var irProgram = irGenerator.Generate();
        DebugHelpers.PrintIr(irProgram);

        // Bytecode

        var bytecodeGenerator = new BytecodeGenerator(irProgram);
        var bytecodeBytes = bytecodeGenerator.Generate();
        
        // Execute

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, bytecodeBytes);
            var zirconBytecode = Bytecode.FromFile(tempFile);
            
            DebugHelpers.Disassemble(zirconBytecode);

            var vm = new VirtualMachine(zirconBytecode);
            vm.Run();

            if (vm.ReturnValue != null)
            {
                Console.WriteLine(vm.ReturnValue);
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
