using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;


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
    }

    public interface IHelper
    {
        Int32 GetWorkItemIdFromUrl(string url);

    }


}
