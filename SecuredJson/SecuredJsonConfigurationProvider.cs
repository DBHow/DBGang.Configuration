using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBGang.Configuration.SecuredJson
{
    public class SecuredJsonConfigurationProvider : FileConfigurationProvider
    {
        public SecuredJsonConfigurationProvider(SecuredJsonConfigurationSource source) : base(source) { }

        public override void Load(Stream stream)
        {
            bool needWriteBack = false;
            JObject jObject = null;

            try
            {
                Data = SecuredJsonConfigurationFileParser.Parse(stream, ((SecuredJsonConfigurationSource)Source).PassPhrase, ref needWriteBack, ref jObject);
            }
            catch (JsonReaderException ex)
            {
                string errorLine = string.Empty;
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    using var sr = new StreamReader(stream);
                    IEnumerable<string> fileContent = ReadLines(sr);
                    errorLine = RetrieveErrorContext(ex, fileContent);
                }

                throw new FormatException($"The format for the provided Json file is not correct at {ex.LineNumber}: [{errorLine}].", ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (needWriteBack && jObject != null)
            {
                var file = Source.FileProvider.GetFileInfo(Source.Path);

                try
                {
                    using var streamWriter = new StreamWriter(file.CreateWriteStream());
                    using var jsonTextWriter = new JsonTextWriter(streamWriter)
                    {
                        Formatting = Formatting.Indented
                    };

                    jObject.WriteTo(jsonTextWriter);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Failed to save the file [{file.Name}] with encrypted information.", ex);
                }
            }
        }

        private static string RetrieveErrorContext(JsonReaderException e, IEnumerable<string> fileContent)
        {
            string errorLine = null;
            if (e.LineNumber >= 2)
            {
                var errorContext = fileContent.Skip(e.LineNumber - 2).Take(2).ToList();
                if (errorContext.Count() >= 2)
                {
                    errorLine = errorContext[0].Trim() + Environment.NewLine + errorContext[1].Trim();
                }
            }

            if (string.IsNullOrEmpty(errorLine))
            {
                var possibleLineContent = fileContent.Skip(e.LineNumber - 1).FirstOrDefault();
                errorLine = possibleLineContent ?? string.Empty;
            }

            return errorLine;
        }

        private static IEnumerable<string> ReadLines(StreamReader sr)
        {
            string line;
            do
            {
                line = sr.ReadLine();
                yield return line;
            } while (line != null);
        }
    }
}
