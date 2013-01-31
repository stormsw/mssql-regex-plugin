using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

public partial class UserDefinedFunctions
{
    public static readonly RegexOptions Options =
        RegexOptions.IgnorePatternWhitespace |
        RegexOptions.Singleline;

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlBoolean RegexMatch(SqlString pattern, SqlChars input)
    {
        Regex regex = new Regex(pattern.Value, Options);
        Match match = regex.Match(new string(input.Value));
        return regex.IsMatch(new string(input.Value));        
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlInt32 RegexMatchPos(SqlString pattern, SqlChars input)
    {
        Regex regex = new Regex(pattern.Value, Options);
        Match match = regex.Match(new string(input.Value));
        return match.Success?match.Index:-1;
        
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlChars RegexGroup(
        SqlString pattern,  SqlChars input, SqlString name)
    {
        Regex regex = new Regex(pattern.Value, Options);
        Match match = regex.Match(new string(input.Value));
        return match.Success ?
            new SqlChars(match.Groups[name.Value].Value) : SqlChars.Null;
    }

    internal class MatchNode
    {
        private int _index;
        public int Index { get { return _index; } }

        private string _value;
        public string Value { get { return _value; } }

        public MatchNode(int index, string value)
        {
            _index = index;
            _value = value;
        }
    }

    internal class MatchIterator : IEnumerable
    {
        private Regex _regex;
        private string _input;

        public MatchIterator(string input, string pattern)
        {
            _regex = new Regex(pattern, UserDefinedFunctions.Options);
            _input = input;
        }

        public IEnumerator GetEnumerator()
        {
            int index = 0;
            Match current = null;
            do
            {
                current = (current == null) ?
                    _regex.Match(_input) : current.NextMatch();
                if (current.Success)
                {
                    yield return new MatchNode(++index, current.Value);
                }
            }
            while (current.Success);
        }
    }

    [SqlFunction(FillRowMethodName = "FillMatchRow",
    TableDefinition = "[Index] int,[Text] nvarchar(max)")]
    public static IEnumerable RegexMatches(SqlString pattern, SqlChars input)
    {
        return new MatchIterator(new string(input.Value), pattern.Value);
    }

    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void FillMatchRow(object data,
        out SqlInt32 index, out SqlChars text)
    {
        MatchNode node = (MatchNode)data;
        index = new SqlInt32(node.Index);
        text = new SqlChars(node.Value.ToCharArray());
    }

    internal class GroupNode
    {
        private int _index;
        public int Index { get { return _index; } }

        private string _name;
        public string Name { get { return _name; } }

        private string _value;
        public string Value { get { return _value; } }

        public GroupNode(int index, string group, string value)
        {
            _index = index;
            _name = group;
            _value = value;
        }
    }

    internal class GroupIterator : IEnumerable
    {
        private Regex _regex;
        private string _input;

        public GroupIterator(string input, string pattern)
        {
            _regex = new Regex(pattern, UserDefinedFunctions.Options);
            _input = input;
        }

        public IEnumerator GetEnumerator()
        {
            int index = 0;
            Match current = null;
            string[] names = _regex.GetGroupNames();
            do
            {
                index++;
                current = (current == null) ?
                    _regex.Match(_input) : current.NextMatch();
                if (current.Success)
                {
                    foreach (string name in names)
                    {
                        Group group = current.Groups[name];
                        if (group.Success)
                        {
                            yield return new GroupNode(
                                index, name, group.Value);
                        }
                    }
                }
            }
            while (current.Success);
        }
    }

    [SqlFunction(FillRowMethodName = "FillGroupRow", TableDefinition =
    "[Index] int,[Group] nvarchar(max),[Text] nvarchar(max)")]
    public static IEnumerable
        RegexGroups(SqlString pattern, SqlChars input)
    {
        return new GroupIterator(new string(input.Value), pattern.Value);
    }

    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void FillGroupRow(object data,
        out SqlInt32 index, out SqlChars group, out SqlChars text)
    {
        GroupNode node = (GroupNode)data;
        index = new SqlInt32(node.Index);
        group = new SqlChars(node.Name.ToCharArray());
        text = new SqlChars(node.Value.ToCharArray());
    }
};

