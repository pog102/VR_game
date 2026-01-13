using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MiniJSON
{
    public static class Json
    {
        public static object Deserialize(string json)
        {
            if (json == null)
            {
                return null;
            }
            return Parser.Parse(json);
        }

        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }

        private sealed class Parser : IDisposable
        {
            private const string WordBreak = "{}[],:\"";
            private StringReader _reader;

            private Parser(string json)
            {
                _reader = new StringReader(json);
            }

            public static object Parse(string json)
            {
                using (Parser parser = new Parser(json))
                {
                    return parser.ParseValue();
                }
            }

            public void Dispose()
            {
                _reader.Dispose();
                _reader = null;
            }

            private Dictionary<string, object> ParseObject()
            {
                Dictionary<string, object> table = new Dictionary<string, object>();
                _reader.Read();

                while (true)
                {
                    Token nextToken = NextToken;
                    if (nextToken == Token.None)
                    {
                        return null;
                    }
                    if (nextToken == Token.CurlyClose)
                    {
                        return table;
                    }

                    string name = ParseString();
                    if (name == null)
                    {
                        return null;
                    }

                    if (NextToken != Token.Colon)
                    {
                        return null;
                    }
                    _reader.Read();

                    table[name] = ParseValue();
                }
            }

            private List<object> ParseArray()
            {
                List<object> array = new List<object>();
                _reader.Read();

                bool parsing = true;
                while (parsing)
                {
                    Token nextToken = NextToken;
                    if (nextToken == Token.None)
                    {
                        return null;
                    }
                    if (nextToken == Token.SquareClose)
                    {
                        _reader.Read();
                        break;
                    }
                    if (nextToken == Token.Comma)
                    {
                        _reader.Read();
                        continue;
                    }

                    object value = ParseValue();
                    array.Add(value);
                }
                return array;
            }

            private object ParseValue()
            {
                switch (NextToken)
                {
                    case Token.String:
                        return ParseString();
                    case Token.Number:
                        return ParseNumber();
                    case Token.CurlyOpen:
                        return ParseObject();
                    case Token.SquareOpen:
                        return ParseArray();
                    case Token.True:
                        _reader.Read();
                        _reader.Read();
                        _reader.Read();
                        _reader.Read();
                        return true;
                    case Token.False:
                        _reader.Read();
                        _reader.Read();
                        _reader.Read();
                        _reader.Read();
                        _reader.Read();
                        return false;
                    case Token.Null:
                        _reader.Read();
                        _reader.Read();
                        _reader.Read();
                        _reader.Read();
                        return null;
                    case Token.None:
                        return null;
                    default:
                        return null;
                }
            }

            private string ParseString()
            {
                StringBuilder s = new StringBuilder();
                char c;

                _reader.Read();
                bool parsing = true;
                while (parsing)
                {
                    if (_reader.Peek() == -1)
                    {
                        break;
                    }

                    c = NextChar;
                    if (c == '"')
                    {
                        parsing = false;
                        break;
                    }
                    if (c == '\\')
                    {
                        if (_reader.Peek() == -1)
                        {
                            parsing = false;
                            break;
                        }

                        c = NextChar;
                        if (c == '"')
                        {
                            s.Append('"');
                        }
                        else if (c == '\\')
                        {
                            s.Append('\\');
                        }
                        else if (c == '/')
                        {
                            s.Append('/');
                        }
                        else if (c == 'b')
                        {
                            s.Append('\b');
                        }
                        else if (c == 'f')
                        {
                            s.Append('\f');
                        }
                        else if (c == 'n')
                        {
                            s.Append('\n');
                        }
                        else if (c == 'r')
                        {
                            s.Append('\r');
                        }
                        else if (c == 't')
                        {
                            s.Append('\t');
                        }
                        else if (c == 'u')
                        {
                            char[] hex = new char[4];
                            for (int i = 0; i < 4; i++)
                            {
                                hex[i] = NextChar;
                            }
                            s.Append((char)Convert.ToInt32(new string(hex), 16));
                        }
                    }
                    else
                    {
                        s.Append(c);
                    }
                }

                return s.ToString();
            }

            private object ParseNumber()
            {
                string number = NextWord;
                if (number.IndexOf('.') == -1 && number.IndexOf('e') == -1 && number.IndexOf('E') == -1)
                {
                    if (long.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsedInt))
                    {
                        return parsedInt;
                    }
                }

                if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedDouble))
                {
                    return parsedDouble;
                }

                return 0;
            }

            private void EatWhitespace()
            {
                while (char.IsWhiteSpace(PeekChar))
                {
                    _reader.Read();
                    if (_reader.Peek() == -1)
                    {
                        break;
                    }
                }
            }

            private char PeekChar
            {
                get { return Convert.ToChar(_reader.Peek()); }
            }

            private char NextChar
            {
                get { return Convert.ToChar(_reader.Read()); }
            }

            private string NextWord
            {
                get
                {
                    StringBuilder word = new StringBuilder();
                    while (_reader.Peek() != -1 && !IsWordBreak(PeekChar))
                    {
                        word.Append(NextChar);
                    }
                    return word.ToString();
                }
            }

            private Token NextToken
            {
                get
                {
                    EatWhitespace();
                    if (_reader.Peek() == -1)
                    {
                        return Token.None;
                    }

                    char c = PeekChar;
                    if (c == '"')
                    {
                        return Token.String;
                    }
                    if (c == '{')
                    {
                        return Token.CurlyOpen;
                    }
                    if (c == '}')
                    {
                        _reader.Read();
                        return Token.CurlyClose;
                    }
                    if (c == '[')
                    {
                        return Token.SquareOpen;
                    }
                    if (c == ']')
                    {
                        _reader.Read();
                        return Token.SquareClose;
                    }
                    if (c == ',')
                    {
                        _reader.Read();
                        return Token.Comma;
                    }
                    if (c == ':')
                    {
                        return Token.Colon;
                    }
                    if (char.IsDigit(c) || c == '-')
                    {
                        return Token.Number;
                    }

                    string word = NextWord;
                    if (word == "false")
                    {
                        return Token.False;
                    }
                    if (word == "true")
                    {
                        return Token.True;
                    }
                    if (word == "null")
                    {
                        return Token.Null;
                    }

                    return Token.None;
                }
            }

            private static bool IsWordBreak(char c)
            {
                return char.IsWhiteSpace(c) || WordBreak.IndexOf(c) != -1;
            }

            private enum Token
            {
                None,
                CurlyOpen,
                CurlyClose,
                SquareOpen,
                SquareClose,
                Colon,
                Comma,
                String,
                Number,
                True,
                False,
                Null
            }
        }

        private sealed class Serializer
        {
            private StringBuilder _builder;

            private Serializer()
            {
                _builder = new StringBuilder();
            }

            public static string Serialize(object obj)
            {
                Serializer serializer = new Serializer();
                serializer.SerializeValue(obj);
                return serializer._builder.ToString();
            }

            private void SerializeValue(object value)
            {
                if (value == null)
                {
                    _builder.Append("null");
                    return;
                }

                if (value is string str)
                {
                    SerializeString(str);
                }
                else if (value is bool boolean)
                {
                    _builder.Append(boolean ? "true" : "false");
                }
                else if (value is IList list)
                {
                    SerializeArray(list);
                }
                else if (value is IDictionary dict)
                {
                    SerializeObject(dict);
                }
                else if (IsNumeric(value))
                {
                    SerializeNumber(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                }
                else
                {
                    SerializeString(value.ToString());
                }
            }

            private void SerializeObject(IDictionary obj)
            {
                bool first = true;
                _builder.Append('{');
                foreach (object e in obj.Keys)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                    }
                    SerializeString(e.ToString());
                    _builder.Append(':');
                    SerializeValue(obj[e]);
                    first = false;
                }
                _builder.Append('}');
            }

            private void SerializeArray(IList array)
            {
                _builder.Append('[');
                bool first = true;
                foreach (object obj in array)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                    }
                    SerializeValue(obj);
                    first = false;
                }
                _builder.Append(']');
            }

            private void SerializeString(string str)
            {
                _builder.Append('"');
                foreach (char c in str)
                {
                    if (c == '"')
                    {
                        _builder.Append("\\\"");
                    }
                    else if (c == '\\')
                    {
                        _builder.Append("\\\\");
                    }
                    else if (c == '\b')
                    {
                        _builder.Append("\\b");
                    }
                    else if (c == '\f')
                    {
                        _builder.Append("\\f");
                    }
                    else if (c == '\n')
                    {
                        _builder.Append("\\n");
                    }
                    else if (c == '\r')
                    {
                        _builder.Append("\\r");
                    }
                    else if (c == '\t')
                    {
                        _builder.Append("\\t");
                    }
                    else
                    {
                        _builder.Append(c);
                    }
                }
                _builder.Append('"');
            }

            private void SerializeNumber(double number)
            {
                _builder.Append(number.ToString(CultureInfo.InvariantCulture));
            }

            private static bool IsNumeric(object value)
            {
                return value is sbyte || value is byte ||
                       value is short || value is ushort ||
                       value is int || value is uint ||
                       value is long || value is ulong ||
                       value is float || value is double ||
                       value is decimal;
            }
        }
    }
}
