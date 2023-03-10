// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Steeltoe.Discovery.Eureka.Transport
{
    public class JsonInstanceInfoConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IList<JsonInstanceInfo>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<JsonInstanceInfo> result = null;
            try
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    result = (List<JsonInstanceInfo>)serializer.Deserialize(reader, typeof(List<JsonInstanceInfo>));
                }
                else
                {
                    JsonInstanceInfo singleInst = (JsonInstanceInfo)serializer.Deserialize(reader, typeof(JsonInstanceInfo));
                    if (singleInst != null)
                    {
                        result = new List<JsonInstanceInfo>
                        {
                            singleInst
                        };
                    }
                }
            }
            catch (Exception)
            {
                result = new List<JsonInstanceInfo>();
            }

            if (result == null)
            {
                result = new List<JsonInstanceInfo>();
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
