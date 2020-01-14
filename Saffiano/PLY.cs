using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;

namespace Saffiano
{
    internal class PLY
    {
        public class TokenReader
        {
            Queue<string> queue = null;
            StreamReader streamReader = null;

            public TokenReader(FileStream fileStream)
            {
                this.streamReader = new StreamReader(fileStream);
            }

            public void Close()
            {
                this.streamReader.Close();
                GC.SuppressFinalize(streamReader);
                this.streamReader = null;
            }

            private bool FillQueue()
            {
                while (queue == null || queue.Count == 0)
                {
                    string line = streamReader.ReadLine();
                    if (line is null)
                    {
                        return false;
                    }
                    queue = new Queue<string>(line.Trim().Split(blankCharacters));
                }
                return true;
            }

            public string ReadToken()
            {
                if (!FillQueue())
                {
                    return null;
                }
                return queue.Dequeue();
            }

            public string PeekToken()
            {
                if (!FillQueue())
                {
                    return null;
                }
                return queue.Peek();
            }

            public Queue<string> ReadRemainingTokensFromCurrentLine()
            {
                var result = queue;
                queue = null;
                return result;
            }
        }

        public class Parser
        {
            internal static Dictionary<string, Type> types = new Dictionary<string, Type>
            {
                { "int",     typeof(int)    },
                { "int16",   typeof(Int16)  },
                { "int32",   typeof(Int32)  },
                { "int64",   typeof(Int64)  },
                { "uchar",   typeof(Byte)   },
                { "uint8",   typeof(Byte)   },
                { "float",   typeof(float)  },
                { "float32", typeof(float)  },
                { "double",  typeof(double) },
                { "float64", typeof(double) },
            };

            private object[] parameters = new object[] { null, null };
            private Type type = null;
            private MethodInfo tryParseMethodInfo = null;

            public Parser(Type type)
            {
                this.type = type;
                this.tryParseMethodInfo = this.type.GetMethod("TryParse", new Type[] { typeof(string), this.type.MakeByRefType() });
            }

            public dynamic Parse(string token)
            {
                this.parameters[0] = token;
                this.tryParseMethodInfo.Invoke(null, this.parameters);
                return this.parameters[1];
            }

            public static Parser Create(Type type)
            {
                return new Parser(type);
            }

            public static Parser Create(string typeName)
            {
                return Parser.Create(Parser.types[typeName]);
            }
        }

        public abstract class Property
        {
            protected TokenReader tokenReader;
            
            public string name
            {
                get;
                protected set;
            }

            public abstract dynamic Read();

            public Property(TokenReader tokenReader)
            {
                this.tokenReader = tokenReader;
            }
        }

        public class Value : Property
        {
            private Parser parser;

            public Value(TokenReader tokenReader) : base(tokenReader)
            {
                parser = Parser.Create(tokenReader.ReadToken());
                name = tokenReader.ReadToken();
            }

            public override dynamic Read()
            {
                return this.parser.Parse(tokenReader.ReadToken());
            }
        }

        public class List : Property
        {
            private Parser lengthParser;
            private Parser itemParser;

            public List(TokenReader tokenReader) : base(tokenReader)
            {
                tokenReader.ReadToken();
                lengthParser = Parser.Create(tokenReader.ReadToken());
                itemParser = Parser.Create(tokenReader.ReadToken());
                name = tokenReader.ReadToken();
            }

            public override dynamic Read()
            {
                int length = this.lengthParser.Parse(tokenReader.ReadToken());
                object[] list = new object[length];
                for (uint i = 0; i < length; i++)
                {
                    list[i] = this.itemParser.Parse(tokenReader.ReadToken());
                }
                return list;
            }
        }

        public class Element
        {
            public string name
            {
                get;
                private set;
            }

            public uint count
            {
                get;
                private set;
            }

            private List<Property> properties = new List<Property>();

            public void AddProperty(Property property)
            {
                properties.Add(property);
            }

            public Element(TokenReader tokenReader)
            {
                name = tokenReader.ReadToken();
                uint count;
                if (!uint.TryParse(tokenReader.ReadToken(), out count))
                {
                    throw new FileFormatException();
                }
                this.count = count;
            }

            public Property GetPropertyByIndex(int index)
            {
                return properties[index];
            }

            public dynamic Read()
            {
                dynamic value = new ExpandoObject();
                foreach (Property property in this.properties)
                {
                    ((IDictionary<string, object>)value)[property.name] = property.Read();
                }
                return value;
            }

        }

        private static char[] blankCharacters = { '\x20', '\n', '\r', '\t' };
        private StreamReader streamReader = null;
        private TokenReader tokenReader = null;
        private List<Element> elements = new List<Element>();

        public Element FindElementByName(string name)
        {
            foreach (var element in elements)
            {
                if (element.name == name)
                {
                    return element;
                }
            }
            return null;
        }

        public dynamic data
        {
            get;
            private set;
        }

        private string ReadToken()
        {
            return this.tokenReader.ReadToken();
        }

        private Queue<string> ReadRemainingTokensFromCurrentLine()
        {
            return this.tokenReader.ReadRemainingTokensFromCurrentLine();
        }

        private string PeekToken()
        {
            return this.tokenReader.PeekToken();
        }

        private void ReadHeader()
        {
            if (ReadToken() != "ply")
            {
                throw new FileFormatException();
            }
            if (ReadToken() != "format" || ReadToken() != "ascii" || ReadToken() != "1.0")
            {
                throw new FileFormatException();
            }
            Element currentElement = null;
            while (true)
            {
                string keyword = ReadToken();
                switch (keyword)
                {
                    case "comment":
                        ReadRemainingTokensFromCurrentLine();
                        break;
                    case "element":
                        currentElement = new Element(tokenReader);
                        elements.Add(currentElement);
                        break;
                    case "property":
                        string type = this.PeekToken();
                        switch (type)
                        {
                            case "list":
                                currentElement.AddProperty(new List(tokenReader));
                                break;
                            default:
                                currentElement.AddProperty(new Value(tokenReader));
                                break;
                        }
                        break;
                    case "end_header":
                        break;
                    default:
                        throw new FileFormatException();
                }
                if (keyword == "end_header")
                {
                    if (ReadRemainingTokensFromCurrentLine().Count > 0)
                    {
                        throw new FileFormatException();
                    }
                    break;
                }
            }
        }

        private void ReadData()
        {
            this.data = new ExpandoObject();
            var setter = this.data as IDictionary<string, object>;
            foreach (Element element in this.elements)
            {
                ExpandoObject[] values = new ExpandoObject[element.count];
                for (int i = 0; i < element.count; i++)
                {
                    values[i] = element.Read();
                }
                setter[element.name] = values;
            }
        }

        public PLY(FileStream fileStream)
        {
            tokenReader = new TokenReader(fileStream);
            ReadHeader();
            ReadData();
            tokenReader.Close();
        }
    }
}
