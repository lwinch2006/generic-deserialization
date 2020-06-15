using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dka.NetCore.JsonAndJschema.GenericDeserialization
{
    public class GenericDeserializedClass
    {
        [JsonExtensionData]
        public Dictionary<string, object> AllProperties { get; set; }
    }
}