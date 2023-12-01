using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace XploreAutomationIntegrations {

    [NonParallelizable]
    public class UnitTest2 {

        public static string CamelCasedJSON(string json) {
            dynamic expandedObject = JsonConvert.DeserializeObject<ExpandoObject>(json);
            var camelSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            string camelJson = JsonConvert.SerializeObject(expandedObject, camelSettings);
            return camelJson;
        }

        public static string CamelCasedJSON(dynamic someObject) {
            string json = JsonConvert.SerializeObject(someObject);
            dynamic expandedObject = JsonConvert.DeserializeObject<ExpandoObject>(json);
            var camelCaseSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            string camelCaseJson = JsonConvert.SerializeObject(expandedObject, camelCaseSettings);
            return camelCaseJson;
        }
        [Test]
        public void Test1() {
            TestCyclePayload testCyclePayload = new TestCyclePayload {
                ProjectId = 53530,
                Name = "Auto TC1",
                Owner = "RUPESHKUMARSOMALA",
                PlannedEndDate = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ"),
                PlannedStartDate = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ"),
                StatusId = 123
            };
            Console.WriteLine(JsonConvert.SerializeObject(testCyclePayload));
            string testcasesStatusJsonFileLoc = @"C:\Users\RUPESHKUMARSOMALA\source\repos\XploreAutomationIntegrations\XploreAutomationIntegrations\testcasesStatus.json";
            string testcasesInfo = File.ReadAllText(testcasesStatusJsonFileLoc);
            JObject testCasesObj = JObject.Parse(testcasesInfo);
            Console.WriteLine(CamelCasedJSON(testCasesObj));
            JToken jToken1 = testCasesObj.SelectToken("$.results");
            Console.WriteLine(JsonConvert.SerializeObject(jToken1));
            JArray jArray = new JArray(jToken1.Children<JObject>());
            int i = 0;
            List<AddedTestRunItem> testRunItems = new List<AddedTestRunItem>();

            foreach (JObject j in jArray) {
                AddedTestRunItem addedTestRunItem = new AddedTestRunItem();
                LastTestResult lastTestResult = new LastTestResult();
                lastTestResult.AssignedTo = j.GetValue("userKey").ToString();
                lastTestResult.TestCaseId = Convert.ToInt32(j.GetValue("testCaseId").ToString());
                addedTestRunItem.Index = i;
                addedTestRunItem.LastTestResult = lastTestResult;
                testRunItems.Add(addedTestRunItem);
                i++;
            }
            AddingTestcasesToTestcyclePayload addingTestcasesToTestcyclePayload = new AddingTestcasesToTestcyclePayload {
                AutoReorder = false,
                TestRunId = 123,
                DeletedTestRunItems = new List<object>(),
                UpdatedTestRunItems = new List<object>(),
                UpdatedTestRunItemsIndexes = new List<object>(),
                AddedTestRunItems = testRunItems
            };
            Console.WriteLine(CamelCasedJSON(addingTestcasesToTestcyclePayload));
        }

        [Test]
        public void Test2() {
            //Assembly assem1 = Assembly.Load(AssemblyName.GetAssemblyName("XploreAutomationIntegrations"));
            Assembly assem1 = Assembly.Load("XploreAutomationIntegrations");
            Type[] types = assem1.GetTypes();
            foreach (Type tc in types) {
                if (tc.IsAbstract) {
                    Console.WriteLine("Abstract Class : " + tc.Name);
                } else if (tc.IsPublic) {
                    Console.WriteLine("Public Class : " + tc.Name);
                } else if (tc.IsSealed) {
                    Console.WriteLine("Sealed Class : " + tc.Name);
                }
                MemberInfo[] methodName = tc.GetMethods();
                foreach (MemberInfo method in methodName) {
                    if (method.ReflectedType.IsPublic) {
                        Console.WriteLine("Public Method : " + method.Name.ToString());
                    } else {
                        Console.WriteLine("Non-Public Method : " + method.Name.ToString());
                    }
                }
            }
        }

        [Test]
        [Category("ABC")]
        public void SOMEPROJECTTest3() {
            var rootPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            var dlls = Directory.GetFiles(rootPath, "*.dll", SearchOption.AllDirectories).Where(row => row.Contains("\\bin\\")).Distinct();
            HashSet<string> set = new HashSet<string>();
            foreach (var dll in dlls) {
                var assembly = Assembly.LoadFrom(dll);
                var types = assembly.GetTypes();
                foreach (var type in types) {
                    var members = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static);
                    foreach (MemberInfo member in members) {
                        if (member.Name.Contains("PROJECTKEY")) {
                            set.Add(string.Format($"{assembly.GetName().Name}.{type.Name}.{member.Name}"));
                            //Console.WriteLine($"{assembly.GetName().Name}.{type.Name}.{member.Name}");
                        }
                    }
                    //var members2 = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Static);
                    //foreach (MemberInfo member in members2) {
                    //    if (member.Name.Contains("PROJECTKEY")) {
                    //        Console.WriteLine($"{assembly.GetName().Name}.{type.Name}.{member.Name}");
                    //    }
                    //}
                }

            }
            foreach (var i in set) {
                Console.WriteLine(i);
            }
        }
        [Test]
        [Category("ABC")]
        public void SOMEPROJECTTest4() {
            Assert.Multiple(() => {
                Assert.IsTrue(5 > 4, "Bool check failed");
                Assert.AreEqual("s", "a", "string check failed");
                Assert.Greater(3, 4, "Greater than failed");
            });

            Console.WriteLine("Print");
        }
        [Test]
        [Category("ABC")]
        public void SOMEPROJECTTest5() {
            string hostName = Dns.GetHostName();
            Console.WriteLine(hostName);
            string ip = Dns.GetHostByName(hostName).AddressList[0].ToString();
            Console.WriteLine(ip);
            Console.WriteLine("Print");
        }
        [TearDown]
        public void Teardown() {
            Console.WriteLine("StackTrace: " + TestContext.CurrentContext.Result.StackTrace);
            Console.WriteLine("Message:" + TestContext.CurrentContext.Result.Message);
            Console.WriteLine("TestDirectory:" + TestContext.CurrentContext.TestDirectory);
            Console.WriteLine("FullName:" + TestContext.CurrentContext.Test.FullName);
            Console.WriteLine("Name:" + TestContext.CurrentContext.Test.Name);
            Console.WriteLine("MethodName:" + TestContext.CurrentContext.Test.MethodName);
            Console.WriteLine("ClassName:" + TestContext.CurrentContext.Test.ClassName);
            Console.WriteLine("Message:" + TestContext.CurrentContext.Result.Message);
            Console.WriteLine("Message:" + TestContext.CurrentContext.Result.Message);

            foreach (var i in TestContext.CurrentContext.Result.Assertions) {
                Console.WriteLine("AssertionsStatus: " + i.Status);
                Console.WriteLine("AssertionsStackTrace: " + i.StackTrace);
                Console.WriteLine("AssertionsMessage: " + i.Message);
            }
        }

    }
}






