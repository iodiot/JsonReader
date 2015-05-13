using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Json
{
  #region Json wrappers
  public class JsonBase
  {
    public virtual int ToInt()
    {
      return 0;
    }

    public virtual List<JsonObject> ToListOfObjects()
    {
      return new List<JsonObject>();
    }
  }

  public class JsonObject : JsonBase
  {
    public readonly Dictionary<string, JsonBase> Fields;

    public bool Contains(string name)
    {
      return Fields.ContainsKey(name);
    }

    public JsonBase this[string name]
    {
      get
      {
        return Fields[name];
      }
    }

    public JsonObject()
    {
      Fields = new Dictionary<string, JsonBase>();
    }
  }

  public class JsonField : JsonBase
  {
    public string Name;
    public JsonBase Value;
  }

  public class JsonText : JsonBase
  {
    public readonly string Value;

    public JsonText(string value)
    {
      Value = value;
    }

    public override string ToString()
    {
      return Value;
    }

    public override int ToInt()
    {
      return Convert.ToInt32(Value);
    }
  }

  public class JsonNumber : JsonBase
  {
    public int Value;

    public JsonNumber(int value)
    {
      Value = value;
    }

    public override string ToString()
    {
      return Value.ToString();
    }

    public override int ToInt()
    {
      return Value;
    }
  }

  public class JsonArray : JsonBase
  {
    public readonly List<JsonObject> Objects;

    public JsonObject this[int n]
    {
      get
      {
        return Objects[n];
      }
    }

    public JsonArray()
    {
      Objects = new List<JsonObject>();
    }

    public override List<JsonObject> ToListOfObjects()
    {
      return Objects;
    }
  }

  public class JsonNull : JsonBase
  {
  }

  public class JsonBoolean : JsonBase
  {
    public readonly bool Value;

    public JsonBoolean(bool value)
    {
      Value = value;
    }

    public override string ToString()
    {
      return Value.ToString();
    }

    public override int ToInt()
    {
      return Value ? 1 : 0;
    }
  }
  #endregion

  public sealed class JsonReader
  {
    private readonly string text;
    private readonly JsonObject rootObject;

    private int currentPosition, currentLine, currentColumn;

    public JsonBase this[string name]
    {
      get
      {
        return rootObject.Fields[name];
      }
    }

    public JsonReader(string filePath)
    {
      using (var streamReader = new StreamReader(filePath))
      {
        text = streamReader.ReadToEnd();
      }

      rootObject = Block();
    }

    private JsonObject Block()
    {
      var block = new JsonObject();

      Read('{');

      while (LookAhead('\"'))
      {
        var field = Field();

        block.Fields.Add(field.Name, field.Value);

        if (LookAhead(','))
        {
          Read(',');
        }
        else
        {
          break;
        }
      }

      Read('}');

      return block;
    }

    private JsonArray Array()
    {
      var array = new JsonArray();

      Read('[');

      while (LookAhead('{'))
      {
        array.Objects.Add(Block());

        if (LookAhead(','))
        {
          Read(',');
        }
        else
        {
          break;
        }
      }

      Read(']');

      return array;
    }

    private JsonField Field()
    {
      var field = new JsonField();

      field.Name = Text().Value;

      Read(':');

      if (LookAhead('\"'))
      {
        field.Value = Text();
      }
      else if (LookAhead('{'))
      {
        field.Value = Block();
      }
      else if (LookAhead('['))
      {
        field.Value = Array();
      }
      else if (Char.IsDigit(LookAhead()) || LookAhead('-'))
      {
        field.Value = Number();
      }
      else if (LookAhead('n'))
      {
        field.Value = Null();
      }
      else if (LookAhead('t') || LookAhead('f'))
      {
        field.Value = Boolean();
      }

      return field;
    }

    private JsonBoolean Boolean()
    {
      if (LookAhead('t'))
      {
        Read("true");
        return new JsonBoolean(true);
      }
      else
      {
        Read("false");
        return new JsonBoolean(false);
      }
    }

    private JsonNull Null()
    {
      Read("null");

      return new JsonNull();
    }

    private JsonNumber Number()
    {
      var value = new StringBuilder();

      if (LookAhead('-'))
      {
        Read('-');
        value.Append('-');
      }

      while (Char.IsDigit(LookAhead()))
      {
        value.Append(Read());
      }

      return new JsonNumber(Convert.ToInt32(value.ToString()));
    }

    private JsonText Text()
    {
      var value = new StringBuilder();

      Read('\"');

      while (!LookAhead('\"'))
      {
        value.Append(Read());
      }

      Read('\"');

      return new JsonText(value.ToString());
    }

    #region Read and look ahead
    private void Read(string chars)
    {
      foreach (var ch in chars)
      {
        Read(ch);
      }
    }

    private char Read()
    {
      return LookAhead(true);      
    }

    private void Read(char ch)
    {
      var readCh = LookAhead(true);

      if (ch != readCh)
      {
        throw new Exception(String.Format("Unexpected symbol: line {0}, column {1}, expected {2}, got {3}", currentLine, currentColumn - 1, ch, readCh));
      }
    }

    private bool LookAhead(char ch)
    {
      return LookAhead() == ch;
    }

    private char LookAhead(bool move = false)
    {
      char result;

      var n = currentPosition;
      while (true)
      {
        if (Eof())
        {
          throw new Exception("Unexpected end of file");
        }

        result = text[n++];

        ++currentColumn;

        if (Char.IsControl(result) || Char.IsSeparator(result) || Char.IsWhiteSpace(result))
        {
          if (result == '\n')
          {
            ++currentLine;
            currentColumn = 0;
          }

          continue;
        }

        break;
      }

      if (move)
      {
        currentPosition = n;
      }

      return result;
    }

    private bool Eof()
    {
      return currentPosition == text.Length;
    }
    #endregion
  }
}
