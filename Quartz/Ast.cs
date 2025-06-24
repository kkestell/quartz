namespace Quartz;

using Zircon;

public abstract class AstNode { }

public abstract class Statement : AstNode { }

public abstract class Expression : AstNode { }

public class LiteralExpr(Value value) : Expression
{
    public Value Value { get; } = value;
}

public class BinaryExpr(Expression left, string op, Expression right) : Expression
{
    public Expression Left { get; } = left;
    public string Operator { get; } = op;
    public Expression Right { get; } = right;
}

public class UnaryExpr(string op, Expression right) : Expression
{
    public string Operator { get; } = op;
    public Expression Right { get; } = right;
}

public class VariableExpr(string name) : Expression
{
    public string Name { get; } = name;
}

public class AssignmentExpr(string name, Expression value) : Expression
{
    public string Name { get; } = name;
    public Expression Value { get; } = value;
}

public class CallExpr(Expression callee, List<Expression> arguments) : Expression
{
    public Expression Callee { get; } = callee;
    public List<Expression> Arguments { get; } = arguments;
}

public class ExpressionStmt(Expression expression) : Statement
{
    public Expression Expression { get; } = expression;
}

public class PrintStmt(Expression expression) : Statement
{
    public Expression Expression { get; } = expression;
}

public class VarDeclStmt(string name, Expression? initializer) : Statement
{
    public string Name { get; } = name;
    public Expression? Initializer { get; } = initializer;
}

public class BlockStmt(List<Statement> statements) : Statement
{
    public List<Statement> Statements { get; } = statements;
}

public class IfStmt(Expression condition, Statement thenBranch, Statement? elseBranch) : Statement
{
    public Expression Condition { get; } = condition;
    public Statement ThenBranch { get; } = thenBranch;
    public Statement? ElseBranch { get; } = elseBranch;
}

public class WhileStmt(Expression condition, Statement body) : Statement
{
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;
}

public class FunctionDeclStmt(string name, List<string> parameters, BlockStmt body) : Statement
{
    public string Name { get; } = name;
    public List<string> Parameters { get; } = parameters;
    public BlockStmt Body { get; } = body;
}

public class ReturnStmt(Expression? value) : Statement
{
    public Expression? Value { get; } = value;
}
