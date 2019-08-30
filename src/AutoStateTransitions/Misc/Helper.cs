using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoStateTransitions.Misc
{
    public class Helper : IHelper
    {
        public Int32 GetWorkItemIdFromUrl(string url)
        {
            Int32 lastIndexOf = url.LastIndexOf("/");
            Int32 size = url.Length - (lastIndexOf + 1);

            string value = url.Substring(lastIndexOf + 1, size);

            return Convert.ToInt32(value);
        }

        public void GetRules()
        {
            JObject o1 = JObject.Parse(File.ReadAllText(@"c:\videogames.json"));

            // read JSON directly from a file
            using (StreamReader file = File.OpenText(@"c:\videogames.json"))

            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject o2 = (JObject)JToken.ReadFrom(reader);
            }
        }

    }

    public interface IHelper
    {
        Int32 GetWorkItemIdFromUrl(string url);
    }


}
