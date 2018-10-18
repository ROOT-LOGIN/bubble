/*
RELEASE TO PUBLIC DOMAIN.
NO ANY WARRANTY!
USE ON YOUR OWN RISK!
*/

/*
形成树形结构执行计算，想法应该可以实现，
最好的实现应该是调度场算法。
*/

enum ConditionOperator
{
    NONE, LPARAN, RPARAN, AND, OR, NOT
}

[DebuggerDisplay("{ToString()}")]
class ConditionNode
{
    public readonly List<ConditionNode> Children = new List<ConditionNode>();

    public static readonly ConditionNode Error = new ConditionNode();

    public string Symbol;

    public bool? Value;

    public ConditionOperator Operator = ConditionOperator.NONE;

    public void Merge(ConditionNode _)
    {
        Symbol = Symbol + _.Symbol;
    }

    public void And(ConditionNode _)
    {
        Symbol = Symbol + "&&" + _.Symbol;
        Value = (bool)Value && (bool)_.Value;
    }

    public void Or(ConditionNode _)
    {
        Symbol = Symbol + "||" + _.Symbol;
        Value = (bool)Value || (bool)_.Value;
    }

    public void Not()
    {
        Symbol = "!" + Symbol;
        Value = !(bool)Value;
    }

    public override string ToString()
    {
        if (Operator == ConditionOperator.AND) return "&&";
        else if (Operator == ConditionOperator.OR) return "||";
        else if (Operator == ConditionOperator.NOT) return "!";
        else if (Operator == ConditionOperator.LPARAN) return "(";
        else if (Operator == ConditionOperator.RPARAN) return ")";

        return $"{Symbol} -> {Value}";
    }

    public void Compute()
    {
        Compute(this);
    }

    static ConditionNode ComputeNot(ConditionNode root, ref int i)
    {
        int j = i;
        while (j < root.Children.Count)
        {
            if (root.Children[j].Operator == ConditionOperator.NOT)
                j++;
            else
                break;
        }
        var right = root.Children[j];
        int p = j;
        Compute(right);
        right = new ConditionNode()
        {
            Symbol = right.Symbol,
            Operator = right.Operator,
            Value = right.Value
        };
        while (j > i)
        {
            right.Not();
            j--;
        };
        i = p;
        return right;
    }

    static void Compute(ConditionNode root)
    {
        if (root.Children.Count == 0) return;

        ConditionNode left = null;
        for (int i = 0; i < root.Children.Count; i++)
        {
            var c = root.Children[i];
            if (c.Children.Count != 0)
            {
                Compute(c);
            }
            if (c.Operator == ConditionOperator.NONE)
            {
                if (left != null)
                {
                    int a = 0;
                }
                left = new ConditionNode()
                {
                    Symbol = c.Symbol,
                    Operator = c.Operator,
                    Value = c.Value
                };
            }
            else if (c.Operator == ConditionOperator.AND)
            {
                i++;
                var right = ComputeNot(root, ref i);
                left.And(right);
            }
            else if (c.Operator == ConditionOperator.OR)
            {
                i++;
                var right = ComputeNot(root, ref i);
                left.Or(right);
            }
            else if (c.Operator == ConditionOperator.NOT)
            {
                left = ComputeNot(root, ref i);
            }
        }
        root.Value = left.Value;
        root.Symbol = "(" + left.Symbol + ")";
    }
}

[DebuggerDisplay("{Count} -> {ToString()}")]
class ConditionStack : Stack<ConditionNode>
{
    public override string ToString()
    {
        if (Count == 0) return string.Empty;
        return Peek()?.Symbol;
    }
}

static bool IsLetter(char c)
{
    return c == '_' || char.IsLetter(c);
}
static bool IsTokenPart(char c)
{
    return c == '_' || char.IsLetterOrDigit(c);
}
int _ReadSymbol(ConditionNode node, string condition, int i)
{
    if (IsLetter(condition[i]))
    {
        int j = i;
        while (j < condition.Length && IsTokenPart(condition[j])) j++;
        var symbol = condition.Substring(i, j - i);
        node.Children.Add(new ConditionNode()
        {
            Symbol = symbol,
            Value = Symbols.Contains(symbol)
        });
        i = j;
    }
    return i;
}
int _ReadNumber(ConditionNode node, string condition, int i)
{
    if (char.IsNumber(condition[i]))
    {
        int j = i;
        while (j < condition.Length && char.IsNumber(condition[j])) j++;
        if (j < condition.Length)
        {
            if (!ExpectTokenStop(condition[j]))
            {
                OutputError($"Encount `{condition[j]}` while expecting token stop");
                return int.MaxValue;
            }
        }
        var num = condition.Substring(i, j - i);
        node.Children.Add(new ConditionNode()
        {
            Symbol = num,
            Value = ulong.Parse(num) != 0
        });
        i = j;
    }
    return i;
}
int _ReadNumberOrSymbol(ConditionNode node, string condition, int i)
{
    int xi = _ReadNumber(node, condition, i);
    if (xi == i)
    {
        xi = _ReadSymbol(node, condition, i);
    }
    i = xi;
    return xi;
}
int _SkipWhitespace(string condition, int i)
{
    while (i < condition.Length && char.IsWhiteSpace(condition[i])) i++;
    return i;
}
int _ReadExpression(ConditionNode node, string condition, int p, bool stop)
{
    ConditionStack stack = new ConditionStack();
    int i = p;
    while (i < condition.Length)
    {
        var c = condition[i];
        if (c == '(')
        {
            stack.Push(node);
            node = new ConditionNode();
            i++;
        }
        else if (c == ')')
        {
            stack.Peek().Children.Add(node);
            node = stack.Pop();
            i++;
        }
        else if (c == '!')
        {
            node.Children.Add(new ConditionNode() { Operator = ConditionOperator.NOT });
            i++;
        }
        else if (c == '&')
        {
            i++;
            if (condition[i] != '&')
            {
                OutputError($"Encount `{condition[i]}` while expecting `|` on position {i}");
                return int.MaxValue;
            }
            node.Children.Add(new ConditionNode() { Operator = ConditionOperator.AND });
            i++;
        }
        else if (c == '|')
        {
            i++;
            if (condition[i] != '|')
            {
                OutputError($"Encount `{condition[i]}` while expecting `|` on position {i}");
                return int.MaxValue;
            }
            node.Children.Add(new ConditionNode() { Operator = ConditionOperator.OR });
            i++;
        }
        else if (char.IsPunctuation(c))
        {
            return int.MaxValue;
        }
        else
        {
            int xi = _ReadSymbol(node, condition, i);
            if (xi == i)
            {
                xi = _ReadNumber(node, condition, i);
            }
            if (xi != i)
            {
                i = xi;
            }
        }
        i = _SkipWhitespace(condition, i);
    }
    return i;
}
