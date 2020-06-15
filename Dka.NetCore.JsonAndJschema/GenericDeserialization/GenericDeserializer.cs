using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dka.NetCore.JsonAndJschema.GenericDeserialization
{
    public class GenericDeserializer
    {
        public GenericDeserializedClass DeserializeObject(string objectAsString)
        {
            var genericObject = JsonConvert.DeserializeObject<GenericDeserializedClass>(objectAsString);

            for (var i = 0; i < genericObject.AllProperties.Keys.Count; i++)
            {
                var propertyKey = genericObject.AllProperties.Keys.ElementAt(i);
                
                switch (genericObject.AllProperties[propertyKey])
                {
                    case JObject propertyValue:
                        genericObject.AllProperties[propertyKey] = DeserializeObject(JsonConvert.SerializeObject(propertyValue));
                        break;
                        
                    case JArray propertyValue:
                        genericObject.AllProperties[propertyKey] = DeserializeObjects(JsonConvert.SerializeObject(propertyValue));
                        break;
                }
            }

            return genericObject;
        }        
        
        public IEnumerable<object> DeserializeObjects(string objectsAsString)
        {
            IEnumerable<object> genericObjects;

            try
            {
                genericObjects = JsonConvert.DeserializeObject<IEnumerable<GenericDeserializedClass>>(objectsAsString);
            }
            catch (JsonSerializationException)
            {
                genericObjects = JsonConvert.DeserializeObject<IEnumerable<object>>(objectsAsString);
                
                return genericObjects;
            }
            
            for (var i = 0; i < genericObjects.Count(); i++)
            {
                var genericObject = (GenericDeserializedClass) genericObjects.ElementAt(i);
                
                for (var j = 0; j < genericObject.AllProperties.Keys.Count; j++)
                {
                    var propertyKey = genericObject.AllProperties.Keys.ElementAt(j);

                    switch (genericObject.AllProperties[propertyKey])
                    {
                        case JObject propertyValue:
                            genericObject.AllProperties[propertyKey] = DeserializeObject(JsonConvert.SerializeObject(propertyValue));
                            break;
                        
                        case JArray propertyValue:
                            genericObject.AllProperties[propertyKey] = DeserializeObjects(JsonConvert.SerializeObject(propertyValue));
                            break;
                    }
                }
            }

            return genericObjects;
        }
    }
}