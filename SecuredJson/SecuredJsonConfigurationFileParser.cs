using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBGang.Configuration.SecuredJson
{
    internal class SecuredJsonConfigurationFileParser : IDisposable
    {
        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _context = new Stack<string>();
        private readonly string _passPhrase;

        private bool _disposed = false;
        private string _currentPath;
        private JsonTextReader _reader;
        private bool _needWriteBack = false;

        private SecuredJsonConfigurationFileParser(string passPhrase)
        {
            _passPhrase = passPhrase;
        }

        public static IDictionary<string, string> Parse(Stream input, string passPhrase, ref bool needWriteBack, ref JObject jObject)
            => new SecuredJsonConfigurationFileParser(passPhrase).ParseStream(input, ref needWriteBack, ref jObject);

        private IDictionary<string, string> ParseStream(Stream input, ref bool needWriteBack, ref JObject jObject)
        {
            _data.Clear();
            _reader = new JsonTextReader(new StreamReader(input))
            {
                DateParseHandling = DateParseHandling.None
            };

            var jsonConfig = (JObject)JToken.ReadFrom(_reader);
            VisitJObject(jsonConfig);

            needWriteBack = _needWriteBack;
            jObject = jsonConfig;

            return _data;
        }

        private void VisitJObject(JObject jObject)
        {
            var properties = jObject.Properties().ToList();

            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var propertyName = property.Name;
                var propertyValue = property.Value;
                var previous = property.Previous;
                var parent = property.Parent;

                if (propertyName.StartsWith(Constants.NeedEncryption, false, CultureInfo.InvariantCulture))
                {
                    propertyName = propertyName.Substring(Constants.NeedEncryption.Length);

                    if (parent != null && !string.IsNullOrEmpty(parent.Path) &&
                        (parent.Path.Contains(Constants.NeedDecryption) || parent.Path.Contains(Constants.NeedEncryption)))
                    {
                        throw new FormatException($"You cannot encrypt/decrypt this node [{propertyName}] because it's been encrypted by one of its parent node.");
                    }

                    var newProperty = new JProperty(
                        $"{Constants.NeedDecryption}{propertyName}",
                        UtilHelper.Encrypt($"{propertyValue.Type}{Constants.TypeValueSeparator}{propertyValue}", _passPhrase)
                        );
                    
                    property.Remove();
                    if (previous != null)
                    {
                        previous.AddAfterSelf(newProperty);
                    }
                    else
                    {
                        parent.AddFirst(newProperty);
                    }

                    if (!_needWriteBack)
                    {
                        _needWriteBack = true;
                    }
                }
                else if (propertyName.StartsWith(Constants.NeedDecryption, false, CultureInfo.InvariantCulture))
                {
                    propertyName = propertyName.Substring(Constants.NeedDecryption.Length);

                    if (parent != null && !string.IsNullOrEmpty(parent.Path) &&
                        (parent.Path.Contains(Constants.NeedDecryption) || parent.Path.Contains(Constants.NeedEncryption)))
                    {
                        throw new FormatException($"You cannot encrypt/decrypt this node [{propertyName}] because it's been encrypted by one of its parent node.");
                    }

                    var plainText = UtilHelper.Decrypt(propertyValue.ToString(), _passPhrase).Split(Constants.TypeValueSeparator);    
                    if (plainText.Length != 2)
                    {
                        throw new FormatException($"Format error in encrypted value for key {propertyName}.");
                    }

                    var type = Enum.Parse<JTokenType>(plainText[0]);
                    var value = plainText[1];
                    if (type != JTokenType.Object && type != JTokenType.Array && type != JTokenType.Integer 
                        && type != JTokenType.Float && type != JTokenType.Boolean && type != JTokenType.Bytes
                        && type != JTokenType.None && type != JTokenType.Null && type != JTokenType.Undefined)
                    {
                        value = $"\"{value.Replace("\\", "\\\\")}\"";
                    }

                    if (type == JTokenType.Boolean)
                    {
                        value = value.ToLower();
                    }

                    propertyValue = JToken.Parse(value);
                }

                EnterContext(propertyName);
                VisitToken(propertyValue);
                ExitContext();
            }
        }

        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                    VisitPrimitive(token.Value<JValue>());
                    break;

                default:
                    throw new FormatException($"Incorrect format: {_reader.TokenType}, {_reader.Path}, {_reader.LineNumber}, {_reader.LinePosition}.");
            }
        }

        private void VisitArray(JArray array)
        {
            for (int index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        private void VisitPrimitive(JValue value)
        {
            var key = _currentPath;

            if (_data.ContainsKey(key))
            {
                throw new FormatException($"Duplicate key: [{key}].");
            }

            _data[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        private void EnterContext(string name)
        {
            _context.Push(name);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _ = _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                if (_reader != null)
                {
                    _reader.Close();
                }
            }

            _disposed = true;
        }
    }
}
