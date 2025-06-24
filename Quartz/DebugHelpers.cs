using System.Text;
using Quartz;
using Zircon;

namespace Quartz;

/// <summary>
/// Provides static methods for printing human-readable representations
/// of compiler artifacts for debugging purposes.
/// </summary>
public static class DebugHelpers
{
    /// <summary>
    /// Prints the entire Abstract Syntax Tree.
    /// </summary>
    public static void PrintAst(List<Statement> program)
    {
        Console.WriteLine("\n=== Step 1: AST ===\n");
        foreach (var stmt in program)
        {
            PrintStatement(stmt, "");
        }
    }

    private static void PrintStatement(Statement stmt, string indent)
    {
        var newIndent = indent + "  ";

        switch (stmt)
        {
            case FunctionDeclStmt s:
                Console.WriteLine($"{indent}FunctionDeclStmt ({s.Name}, params: {string.Join(", ", s.Parameters)})");
                PrintStatement(s.Body, newIndent);
                break;
            case BlockStmt s:
                Console.WriteLine($"{indent}BlockStmt");
                for (var i = 0; i < s.Statements.Count; i++)
                {
                    PrintStatement(s.Statements[i], newIndent);
                }
                break;
            case VarDeclStmt s:
                Console.WriteLine($"{indent}VarDeclStmt ({s.Name})");
                if (s.Initializer != null) PrintExpression(s.Initializer, newIndent, "Initializer");
                break;
            case IfStmt s:
                Console.WriteLine($"{indent}IfStmt");
                PrintExpression(s.Condition, newIndent, "Condition");
                PrintStatement(s.ThenBranch, newIndent);
                if (s.ElseBranch != null) PrintStatement(s.ElseBranch, newIndent);
                break;
            case ReturnStmt s:
                Console.WriteLine($"{indent}ReturnStmt");
                if (s.Value != null) PrintExpression(s.Value, newIndent, "Value");
                break;
            case ExpressionStmt s:
                Console.WriteLine($"{indent}ExpressionStmt");
                PrintExpression(s.Expression, newIndent, "Expression");
                break;
            default:
                Console.WriteLine($"{indent}{stmt.GetType().Name}");
                break;
        }
    }

    private static void PrintExpression(Expression expr, string indent, string label)
    {
        var newIndent = indent + "  ";
        Console.WriteLine($"{indent}{label}: {expr.GetType().Name}");

        switch(expr)
        {
            case BinaryExpr e:
                Console.WriteLine($"{newIndent}Op: {e.Operator}");
                PrintExpression(e.Left, newIndent, "Left");
                PrintExpression(e.Right, newIndent, "Right");
                break;
            case VariableExpr e:
                Console.WriteLine($"{newIndent}Name: {e.Name}");
                break;
            case LiteralExpr e:
                Console.WriteLine($"{newIndent}Value: {e.Value}");
                break;
            case UnaryExpr e:
                 Console.WriteLine($"{newIndent}Op: {e.Operator}");
                 PrintExpression(e.Right, newIndent, "Right");
                 break;
        }
    }

    /// <summary>
    /// Prints the entire Intermediate Representation program.
    /// </summary>
    public static void PrintIr(IrProgram program)
    {
        Console.WriteLine("\n=== Step 2: IR ===\n");
        Console.WriteLine("Main Function (__main):");
        PrintFunction(program.MainFunction);

        foreach (var func in program.Functions)
        {
            Console.WriteLine($"\nFunction ({func.Name}):");
            PrintFunction(func);
        }
    }

    private static void PrintFunction(IrFunction function)
    {
        foreach (var instruction in function.Body)
        {
            if (instruction.Op == IrOp.Label)
            {
                Console.WriteLine($"{instruction.Arg1}:");
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append($"    {instruction.Op,-12}");
                if (instruction.Arg1 != null) sb.Append($" {instruction.Arg1}");
                if (instruction.Arg2 != null) sb.Append($", {instruction.Arg2}");
                if (instruction.Arg3 != null) sb.Append($", {instruction.Arg3}");
                Console.WriteLine(sb.ToString());
            }
        }
    }

    /// <summary>
    /// Disassembles and prints the bytecode for an entire program.
    /// </summary>
    public static void Disassemble(Bytecode bytecode)
    {
        Console.WriteLine("\n=== Step 3: Bytecode ===\n");
        var constants = new List<Value>();
        for (int i = 0; ; i++)
        {
            try { constants.Add(bytecode.GetConstant(i)); }
            catch (ArgumentOutOfRangeException) { break; }
        }
        
        var mainFunc = bytecode.GetFunction(0);
        Console.WriteLine("Function __main:");
        DisassembleFunction(mainFunc, constants);

        // In the future, loop through other functions
    }
    
    /// <summary>
    /// Disassembles a single function's bytecode instructions.
    /// </summary>
    public static void DisassembleFunction(Function function, IReadOnlyList<Value> constants)
    {
        for (int i = 0; i < function.Instructions.Count; i++)
        {
            var instruction = function.GetInstruction(i);
            var opcode = instruction.Opcode;
            
            var sb = new StringBuilder();
            sb.Append($"{i:D4}  {opcode,-20}");

            if (opcode.HasOperand())
            {
                var operand = instruction.GetOperand();
                sb.Append($" {operand}");
                if (opcode == Opcode.PushConst)
                {
                    sb.Append($" ({constants[operand]})");
                }
            }
            
            Console.WriteLine(sb.ToString());
        }
    }
}
