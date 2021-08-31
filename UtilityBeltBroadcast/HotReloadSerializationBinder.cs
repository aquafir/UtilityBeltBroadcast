using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilityBeltBroadcast
{
    public class HotReloadSerializationBinder : SerializationBinder
    {
        Regex genericRe = new Regex(@"^(?<gen>[^\[]+)\[\[(?<type>[^\]]*)\](,\[(?<type>[^\]]*)\])*\]$", RegexOptions.Compiled);
        Regex subtypeRe = new Regex(@"^(?<tname>.*)(?<aname>(,[^,]+){4})$", RegexOptions.Compiled);

        public override Type BindToType(string assemblyName, string typeName)
        {
            var m = genericRe.Match(typeName);
            if (m.Success)
            { // generic type
                var gen = GetFlatTypeMapping(m.Groups["gen"].Value);
                var genArgs = m.Groups["type"]
                    .Captures
                    .Cast<Capture>()
                    .Select(c => {
                        var m2 = subtypeRe.Match(c.Value);
                        return BindToType(m2.Groups["aname"].Value.Substring(1).Trim(), m2.Groups["tname"].Value.Trim());
                    })
                    .ToArray();
                return gen.MakeGenericType(genArgs);
            }
            return GetFlatTypeMapping(typeName);
        }

        private Type GetFlatTypeMapping(string typeName)
        {
            var res = typeof(UBClient).Assembly.GetType(typeName);
            res = res == null ? GetType().Assembly.GetType(typeName) : res;
            res = res == null ? Type.GetType(typeName) : res;
            return res;
        }
    }
}
