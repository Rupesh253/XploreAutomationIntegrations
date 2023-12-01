using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AuxAdapterCLI {
    public class Postman {
        public static string MAGENTA = Console.IsOutputRedirected ? "" : "\x1b[95m";
        public void Main(string[] args) {
            string results = File.ReadAllText(@"C:\Users\git\source\XploreAutomationIntegrations\AuxAdapterCLI\.postman_test_run.json");
            JObject jobj = JObject.Parse(results);
            JToken token = jobj?.SelectToken("$.results");
            Dictionary<string, bool> statusPairs = new Dictionary<string, bool>();
            int k = 0;
            foreach (JObject j in token?.Children<JObject>()) {
                Console.WriteLine($"│ ├──name:{j["name"]}");
                Console.WriteLine($"  allTests:");
                int i = 1;
                foreach (JObject jj in j.SelectToken("$.allTests").Children<JObject>()) {
                    if (jj.HasValues) {
                        var pair = jj?.Properties()?.ToArray()[0];
                        if (pair.Value.ToString().ToLower() == "true") {
                            statusPairs.Add($"PK-T{k}{i}.{pair.Name}", true);
                        } else {
                            statusPairs.Add($"PK-T{k}{i}.{pair.Name}", false);
                        }
                        Console.WriteLine($"│ ├──({k}).({i})___:{pair.Name}:___{pair.Value.ToString().ToLower()}");
                        i++;
                    }
                }
                k++;
            }
            int m = 0;
            foreach (var item in statusPairs) {
                Console.WriteLine($"KEY:{item.Key.Split(".")[0]} \t Status: {item.Value} \t TestcaseID: {m}1234 \t ExecutionID: {m}4567");
                m++;
            }
        }
    }
}
