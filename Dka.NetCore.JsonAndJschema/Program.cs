using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Dka.NetCore.JsonAndJschema.GenericDeserialization;

namespace Dka.NetCore.JsonAndJschema
{
    class Program
    {
        private const string JsonFilename = "json.txt";
        private const string JschemaFilename = "jschema.txt";
        
        static void Main(string[] args)
        {
            var genericDeserializer = new GenericDeserializer();
            
            var originalJsonString = File.ReadAllText(JsonFilename);
            var originalJschemaString = File.ReadAllText(JschemaFilename);

            // Cases where JArray and JObjects will be still presented. 
            var genericClassFromJson1 = JsonConvert.DeserializeObject<IEnumerable<GenericDeserializedClass>>(originalJsonString);
            var genericClassFromJschema1 = JsonConvert.DeserializeObject<GenericDeserializedClass>(originalJschemaString);

            // Cases with full deserialization.
            var genericClassFromJson2 = (IEnumerable<GenericDeserializedClass>) genericDeserializer.DeserializeObjects(originalJsonString);
            var genericClassFromJschema2 = genericDeserializer.DeserializeObject(originalJschemaString);

            // Serializing back from generic classes.
            var newJsonString = JsonConvert.SerializeObject(genericClassFromJson2);
            var newJSchemaString = JsonConvert.SerializeObject(genericClassFromJschema2);
            
            // Flat properties.
            genericClassFromJson2 = genericClassFromJson2.FlatProperties();
            genericClassFromJschema2 = genericClassFromJschema2.FlatProperties();
            
            // Unflat properties.
            genericClassFromJson2 = genericClassFromJson2.UnFlatProperties();
            genericClassFromJschema2 = genericClassFromJschema2.UnFlatProperties();
        }
    }
}