using Quartz;
using Zircon;

namespace Quartz;

public class IrGenerator
{
    private readonly List<Statement> _ast;
    private readonly IrProgram _program = new();
    
    private IrFunction _currentFunction = null!;
    private int _tempCounter;
    private int _labelCounter;

    private readonly Dictionary<string, string> _symbols = new();
    private readonly Dictionary<string, IrFunction> _functions = new();

    public IrGenerator(List<Statement> ast)
    {
        _ast = ast;
    }

    public IrProgram Generate()
    {
        PreScan();

        // Process top-level statements and the main function body
        _currentFunction = _program.MainFunction;
        var mainFuncDecl = _ast.OfType<FunctionDeclStmt>().FirstOrDefault(f => f.Name == "main");
        if (mainFuncDecl != null)
        {
             GenerateFunctionBody(mainFuncDecl);
        }
        else
        {
            // If no explicit main, treat top-level statements as main
            foreach (var statement in _ast)
            {
                if (statement is not FunctionDeclStmt)
                {
                    Visit(statement);
                }
            }
        }
        
        // Process all other functions
        foreach (var funcDecl in _ast.OfType<FunctionDeclStmt>().Where(f => f.Name != "main"))
        {
            GenerateFunctionBody(funcDecl);
        }
        
        return _program;
    }
    
    private void PreScan()
    {
        // Find all top-level function declarations
        foreach (var statement in _ast)
        {
            if (statement is FunctionDeclStmt funcDecl)
            {
                // The main function is handled specially
                if (funcDecl.Name == "main") 
                    continue;
                
                var irFunc = new IrFunction(funcDecl.Name, funcDecl.Parameters);
                _program.Functions.Add(irFunc);
                _functions[funcDecl.Name] = irFunc;
            }
        }
    }
    
    private void GenerateFunctionBody(FunctionDeclStmt funcDecl)
    {
        _currentFunction = funcDecl.Name == "main" ? _program.MainFunction : _functions[funcDecl.Name];
        _tempCounter = 0;
        _labelCounter = 0;
        _symbols.Clear();

        // Register parameters as local symbols
        foreach (var param in funcDecl.Parameters)
        {
           _symbols[param] = param;
        }

        // Visit the function body
        Visit(funcDecl.Body);
    }

    private void Visit(Statement stmt)
    {
        switch (stmt)
        {
            case ExpressionStmt s: 
                VisitExpressionStmt(s); 
                break;
            case VarDeclStmt s: 
                VisitVarDeclStmt(s); 
                break;
            case PrintStmt s: 
                VisitPrintStmt(s); 
                break;
            case BlockStmt s: 
                VisitBlockStmt(s); 
                break;
            case IfStmt s: 
                VisitIfStmt(s); 
                break;
            case WhileStmt s: 
                VisitWhileStmt(s); 
                break;
            case ReturnStmt s: 
                VisitReturnStmt(s); 
                break;
            case FunctionDeclStmt: 
                break; 
            default: 
                throw new NotImplementedException($"Statement {stmt.GetType().Name} not supported.");
        }
    }

    // Returns the name of the temporary variable holding the result
    private string Visit(Expression expr)
    {
        return expr switch
        {
            LiteralExpr e => VisitLiteralExpr(e),
            BinaryExpr { Operator: "&&" or "||" } e => VisitLogicalBinaryExpr(e),
            BinaryExpr e => VisitBinaryExpr(e),
            UnaryExpr e => VisitUnaryExpr(e),
            VariableExpr e => VisitVariableExpr(e),
            AssignmentExpr e => VisitAssignmentExpr(e),
            CallExpr e => VisitCallExpr(e),
            _ => throw new NotImplementedException($"Expression {expr.GetType().Name} not supported.")
        };
    }

    private void VisitExpressionStmt(ExpressionStmt stmt)
    {
        Visit(stmt.Expression); // Visit for side effects, discard result
    }

    private void VisitVarDeclStmt(VarDeclStmt stmt)
    {
        var varName = stmt.Name;
        var tempName = NewTemp();
        _symbols[varName] = tempName;

        if (stmt.Initializer != null)
        {
            var initializerTemp = Visit(stmt.Initializer);
            Emit(IrOp.Copy, tempName, initializerTemp);
        }
        else
        {
            Emit(IrOp.LoadConst, tempName, Value.Boolean(false));
        }
    }
    
    private void VisitPrintStmt(PrintStmt stmt)
    {
        var temp = Visit(stmt.Expression);
        Emit(IrOp.Print, temp);
    }
    
    private void VisitBlockStmt(BlockStmt stmt)
    {
        // TODO: Block scope
        foreach (var statement in stmt.Statements)
        {
            Visit(statement);
        }
    }

    private void VisitIfStmt(IfStmt stmt)
    {
        var elseLabel = NewLabel("else");
        var endIfLabel = NewLabel("endif");
        
        var conditionTemp = Visit(stmt.Condition);
        Emit(IrOp.JumpIfFalse, conditionTemp, elseLabel);
        
        Visit(stmt.ThenBranch);
        Emit(IrOp.Jump, endIfLabel);
        
        Emit(IrOp.Label, elseLabel);
        if (stmt.ElseBranch != null)
        {
            Visit(stmt.ElseBranch);
        }
        
        Emit(IrOp.Label, endIfLabel);
    }
    
    private void VisitWhileStmt(WhileStmt stmt)
    {
        var startLabel = NewLabel("whilestart");
        var endLabel = NewLabel("whileend");
        
        Emit(IrOp.Label, startLabel);
        var conditionTemp = Visit(stmt.Condition);
        Emit(IrOp.JumpIfFalse, conditionTemp, endLabel);
        
        Visit(stmt.Body);
        Emit(IrOp.Jump, startLabel);
        
        Emit(IrOp.Label, endLabel);
    }

    private void VisitReturnStmt(ReturnStmt stmt)
    {
        if (stmt.Value != null)
        {
            var returnValueTemp = Visit(stmt.Value);
            Emit(IrOp.Return, returnValueTemp);
        }
        else
        {
            Emit(IrOp.Return);
        }
    }

    private string VisitLiteralExpr(LiteralExpr expr)
    {
        var temp = NewTemp();
        Emit(IrOp.LoadConst, temp, expr.Value);
        return temp;
    }

    private string VisitVariableExpr(VariableExpr expr)
    {
        if (_symbols.TryGetValue(expr.Name, out var tempName))
        {
            // The "value" of a variable expression is the temporary holding its content.
            return tempName;
        }
        // It could also be a function name, which doesn't have a temporary.
        if (_functions.ContainsKey(expr.Name) || expr.Name == "main")
        {
            return expr.Name;
        }
        throw new Exception($"Undefined variable or function: {expr.Name}");
    }
    
    private string VisitAssignmentExpr(AssignmentExpr expr)
    {
        if (!_symbols.ContainsKey(expr.Name))
        {
             throw new Exception($"Cannot assign to undeclared variable: {expr.Name}");
        }
        
        var valueTemp = Visit(expr.Value);
        var destTemp = _symbols[expr.Name];
        Emit(IrOp.Copy, destTemp, valueTemp);
        return destTemp;
    }

    private string VisitUnaryExpr(UnaryExpr expr)
    {
        var rightTemp = Visit(expr.Right);
        var resultTemp = NewTemp();
        var op = expr.Operator switch
        {
            "-" => IrOp.Neg,
            "!" => IrOp.Not,
            _ => throw new NotImplementedException($"Unary operator '{expr.Operator}' not supported.")
        };
        Emit(op, resultTemp, rightTemp);
        return resultTemp;
    }

    private string VisitLogicalBinaryExpr(BinaryExpr expr)
    {
        var resultTemp = NewTemp();
        var endLabel = NewLabel("logical_end");
        var leftTemp = Visit(expr.Left);

        if (expr.Operator == "&&")
        {
            var isFalseLabel = NewLabel("and_false");
            // If left is false, short-circuit: result is false.
            Emit(IrOp.Copy, resultTemp, leftTemp); // Assume result is left
            Emit(IrOp.JumpIfFalse, leftTemp, endLabel); // If it's false, we're done

            // Otherwise, result is the value of the right side.
            var rightTemp = Visit(expr.Right);
            Emit(IrOp.Copy, resultTemp, rightTemp);
        }
        else // Operator is "||"
        {
            var isTrueLabel = NewLabel("or_true");
            // If left is true, short-circuit: result is true.
            Emit(IrOp.Copy, resultTemp, leftTemp); // Assume result is left
            Emit(IrOp.JumpIfFalse, leftTemp, isTrueLabel); // If it's false, continue
            Emit(IrOp.Jump, endLabel); // Otherwise, we are done
            
            // Evaluate the right side.
            Emit(IrOp.Label, isTrueLabel);
            var rightTemp = Visit(expr.Right);
            Emit(IrOp.Copy, resultTemp, rightTemp);
        }
        
        Emit(IrOp.Label, endLabel);
        return resultTemp;
    }


    private string VisitBinaryExpr(BinaryExpr expr)
    {
        var leftTemp = Visit(expr.Left);
        var rightTemp = Visit(expr.Right);
        var resultTemp = NewTemp();

        var op = expr.Operator switch
        {
            "+" => IrOp.Add,
            "-" => IrOp.Sub,
            "*" => IrOp.Mul,
            "/" => IrOp.Div,
            "%" => IrOp.Mod,
            "==" => IrOp.Eq,
            "!=" => IrOp.Eq, // Will be followed by a Not
            ">" => IrOp.Gt,
            "<" => IrOp.Lt,
            ">=" => IrOp.Gte,
            "<=" => IrOp.Lte,
            _ => throw new NotImplementedException($"Binary operator {expr.Operator} not supported.")
        };
        
        Emit(op, resultTemp, leftTemp, rightTemp);

        if (expr.Operator == "!=")
        {
            var finalResultTemp = NewTemp();
            Emit(IrOp.Not, finalResultTemp, resultTemp);
            return finalResultTemp;
        }

        return resultTemp;
    }
    
    private string VisitCallExpr(CallExpr expr)
    {
        var calleeName = Visit(expr.Callee);
        var argTemps = expr.Arguments.Select(Visit).ToList();
        var resultTemp = NewTemp();
        
        Emit(IrOp.Call, resultTemp, calleeName, argTemps);
        return resultTemp;
    }

    private string NewTemp() => $"t{_tempCounter++}";
    private string NewLabel(string hint = "L") => $".{hint}{_labelCounter++}";
    private void Emit(IrOp op, object? arg1 = null, object? arg2 = null, object? arg3 = null)
    {
        _currentFunction.Body.Add(new IrInstruction(op, arg1, arg2, arg3));
    }
}
