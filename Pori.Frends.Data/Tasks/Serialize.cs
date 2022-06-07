using System.IO;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.CSharp; // You can remove this if you don't need dynamic type in .NET Standard frends Tasks
using Newtonsoft.Json.Linq;

#pragma warning disable 1591

namespace Pori.Frends.Data
{
    /// <summary>
    /// Parameters for serializing a Table.
    /// </summary>
    public class SerializeParameters
    {
        /// <summary>
        /// The table to serialize.
        /// </summary>
        [DisplayFormat(DataFormatString = "Expression")]
        public Table Data { get; set; }

        /// <summary>
        /// Whether to serialize the table 
        /// </summary>
        [DefaultValue(SerializationType.File)]
        public SerializationType Target { get; set; }

        /// <summary>
        /// File path to serialize the table to.
        /// </summary>
        [UIHint(nameof(Target), "", SerializationType.File)]
        public string Path { get; set; }
    }


    public static partial class TableTasks
    {
        /// <summary>
        /// Serialize a table into a file or as a string value for easy loading
        /// in another process.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// For file serialization, the path the file was written to 
        /// (resolved version of the input path). For string serialization,
        /// the serialized table data.
        /// </returns>
        public static string Serialize([PropertyTab] SerializeParameters input, CancellationToken cancellationToken)
        {
            string data = JToken.FromObject(input.Data).ToString(Newtonsoft.Json.Formatting.None);

            if(input.Target == SerializationType.File)
            {
                using(var file = new StreamWriter(input.Path, append: false, encoding: Encoding.UTF8))
                {
                    file.Write(data);
                }

                return Path.GetFullPath(input.Path);
            }
            else
                return data;
        }
    }
}
