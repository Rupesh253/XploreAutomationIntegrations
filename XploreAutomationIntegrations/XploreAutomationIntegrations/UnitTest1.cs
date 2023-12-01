using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using RestSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace XploreAutomationIntegrations {
    [TestFixture]
    [NonParallelizable]
    public class UnitTest1 {
        public string currentClass, currentMethod, currentNamespace;
        public JObject testExeObject;
        public JArray testcasesStatus;
        string testcasesStatusJsonFileLoc;
        public RestClient client;
        public Dictionary<string, string> bulkDefectsLinking;
        public int totalCount, executedCount, notExecutedCount, passedCount, failedCount = 0;
        public int testCycleId;
        public string testCycleName;
        public bool isNew = true;
        public string currentDirectory;
        public string testDLLLoc;
        public string nunit3CommandLoc, dotnetTestCommandLoc, continuationLoc;

        //id, key,eid, status,class, Method
        public List<Tuple<int, string, int, string, string, string>> hardSoftList = new List<Tuple<int, string, int, string, string, string>>();
        public HashSet<string> omitClasses = new HashSet<string>();
        [OneTimeSetUp]
        public void SetUpFixture() {
            currentDirectory = new DirectoryInfo("../../../").FullName;
            currentNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            testDLLLoc = new DirectoryInfo("./").FullName + currentNamespace + ".dll";
            nunit3CommandLoc = currentDirectory + "nunit3Command.txt";
            dotnetTestCommandLoc = currentDirectory + "dotnetTestCommand.txt";
            continuationLoc = currentDirectory + "continuation.json";
            testExeObject = new JObject();
            testcasesStatus = new JArray();
            bulkDefectsLinking = new Dictionary<string, string>();
            client = new RestClient();
            client.BaseUrl = new Uri("https://jira.yourCOMPANYDOMAIN.com");
            client.AddDefaultHeader("Content-Type", "application/json");
            client.AddDefaultHeader("Authorization", "Bearer <token>");
            Console.WriteLine(currentMethod);
            Console.WriteLine("Fixture Setup");
            PrepareTestCases();
            CreateTestcycle();
            currentMethod = MethodBase.GetCurrentMethod().Name;
            currentClass = MethodBase.GetCurrentMethod().DeclaringType.FullName;

            Console.WriteLine(currentNamespace);
            Console.WriteLine(currentClass);
            Console.WriteLine(currentMethod);
            Console.WriteLine("testExeObject:" + testExeObject);
        }
        public static string FormattedJson(string json) {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            string formattedJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
            return formattedJson;
        }

        public void PrepareTestCases() {
            //TODO: filtered testcases smoke, sanity, regress-auto
            var decodedPath = HttpUtility.UrlDecode("rest/tests/1.0/testcase/search?fields=id,key,projectId,name,averageTime,estimatedTime,labels,folderId,componentId,status(id,name,i18nKey,color),priority(id,name,i18nKey,color),lastTestResultStatus(name,i18nKey,color),majorVersion,createdOn,createdBy,updatedOn,updatedBy,customFieldValues,owner,folderId&query=testCase.projectId+IN+(53530)+AND+testCase.folderTreeId+IN+(76425)+ORDER+BY+testCase.name+ASC&startAt=0&maxResults=40&archived=false");
            RestRequest restRequest = new RestRequest(decodedPath);
            var fullUrl = client.BuildUri(restRequest);
            Console.WriteLine(fullUrl);
            IRestResponse restResponse = client.Get(restRequest);
            string response = restResponse.Content;
            Console.WriteLine("restResponse" + response);
            string currentDirectory1 = Environment.CurrentDirectory + @"\testcases.json";
            Console.WriteLine(currentDirectory1);
            DirectoryInfo info = new DirectoryInfo(".");
            Console.WriteLine(info.FullName);
            Console.WriteLine(new DirectoryInfo("..").FullName);
            Console.WriteLine(new DirectoryInfo("../").FullName);
            Console.WriteLine(new DirectoryInfo("../..").FullName);
            Console.WriteLine(new DirectoryInfo("../../").FullName);

            string testcasesjsonFileLoc = currentDirectory + "testcases.json";
            File.WriteAllText(testcasesjsonFileLoc, FormattedJson(response));
            JObject jObject = JObject.Parse(response);
            //Console.WriteLine(jObject["results"][0]);
            //Console.WriteLine(jObject["results"][0]["key"]);
            //Console.WriteLine(jObject["results"][0]["labels"]);
            //Console.WriteLine(jObject.SelectToken("$.results[0].key"));
            //Console.WriteLine(jObject.SelectToken("$.results[0].labels[0]"));
            JToken match = jObject.SelectToken("$.results[?(@.key=='YOURPROJECTKEY-T1997')]");
            JToken results = jObject.SelectToken("$.results");
            Console.WriteLine(match?["id"]);
            JObject newObject = new JObject(
                new JProperty("results", new JArray(
                    results.Children<JObject>().Select(jp => new JObject(
                                new JProperty("userKey", "RUPESHKUMARSOMALA"),
                                new JProperty("projectId", jp["projectId"]),
                                new JProperty("testCaseId", jp["id"]),
                                new JProperty("executionId", ""),
                                new JProperty("key", jp["key"]),
                                new JProperty("executionDate", ""),
                                new JProperty("status", ""),
                                new JProperty("imageLocation", null),
                                new JProperty("exception", null)
                                )))));
            totalCount = newObject.Children().Count();
            string newJson = JsonConvert.SerializeObject(newObject);
            string newFormattedJson = FormattedJson(newJson);
            testcasesStatusJsonFileLoc = new DirectoryInfo("../../../").FullName + "testcasesStatus.json";
            File.WriteAllText(testcasesStatusJsonFileLoc, newFormattedJson);
        }

        public void CreateTestcycle() {
            string resetContinuation = File.ReadAllText(continuationLoc);
            JObject jobject = JObject.Parse(resetContinuation);
            bool isItNewTestCycle = Convert.ToBoolean(jobject.SelectToken("$.info")["isItNewTestCycle"]);
            testCycleId = Convert.ToInt32(jobject.SelectToken("$.info")["testCycleId"]);
            testCycleName = jobject.SelectToken("$.info")["testCycleName"]?.ToString();
            if (isItNewTestCycle) {
                var decodedUrl = HttpUtility.UrlDecode("/rest/tests/1.0/testrun");
                RestRequest request = new RestRequest(decodedUrl);
                request.RequestFormat = DataFormat.Json;
                testCycleName = string.Format("[{0}] Biweekly Regression Automated TestCycle", DateTime.UtcNow.ToString("F"));
                //TODO: Folder based on smoke, sanity, regress
                TestCyclePayload testCyclePayload = new TestCyclePayload {
                    ProjectId = 53530,
                    FolderId = 76433,
                    Name = testCycleName,
                    Owner = "RUPESHKUMARSOMALA",
                    PlannedEndDate = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ"),
                    PlannedStartDate = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ"),
                    StatusId = 123
                };
                string json = UnitTest2.CamelCasedJSON(testCyclePayload);
                request.AddBody(json);
                IRestResponse response = client.Post(request);

                JObject jObject4CreateTestcycle = JObject.Parse(response.Content);
                JToken jToken4Id = jObject4CreateTestcycle.SelectToken("id");
                testCycleId = Convert.ToInt16(jToken4Id.ToString());
                string testcasesInfo = File.ReadAllText(testcasesStatusJsonFileLoc);
                JObject testCasesObj = JObject.Parse(testcasesInfo);
                JToken jToken4Results = testCasesObj.SelectToken("$.results");
                JArray jArray = new JArray(jToken4Results.Children<JObject>());
                int i = 0;
                List<AddedTestRunItem> testRunItems = new List<AddedTestRunItem>();
                foreach (JObject singleObj in jArray) {
                    AddedTestRunItem testRunItem = new AddedTestRunItem();
                    LastTestResult lastTestResult = new LastTestResult();
                    lastTestResult.AssignedTo = singleObj.GetValue("userKey").ToString();
                    lastTestResult.TestCaseId = Convert.ToInt32(singleObj.GetValue("testCaseId").ToString());
                    testRunItem.Index = i;
                    testRunItem.LastTestResult = lastTestResult;
                    testRunItems.Add(testRunItem);
                    i++;
                }
                AddingTestcasesToTestcyclePayload addingTestcasesToTestcyclePayload = new AddingTestcasesToTestcyclePayload {
                    AutoReorder = false,
                    TestRunId = testCycleId,
                    DeletedTestRunItems = new List<object>(),
                    UpdatedTestRunItems = new List<object>(),
                    UpdatedTestRunItemsIndexes = new List<object>(),
                    AddedTestRunItems = testRunItems
                };

                var decodedUrl4AttachingTestCases2TestCycle = HttpUtility.UrlDecode("rest/tests/1.0/testrunitem/bulk/save");
                RestRequest request4Link = new RestRequest(decodedUrl4AttachingTestCases2TestCycle);
                request4Link.AddJsonBody(UnitTest2.CamelCasedJSON(addingTestcasesToTestcyclePayload));
                IRestResponse response4Link = client.Put(request4Link);
            } else {
                var decodedUrl = HttpUtility.UrlDecode($"/rest/tests/1.0/testrun/{testCycleId}");
                RestRequest request = new RestRequest(decodedUrl);
                request.RequestFormat = DataFormat.Json;
                testCycleName += " [Continued]";
                //TODO: Folder based on smoke, sanity, regress
                JObject updateObject = new JObject(
                     new JProperty("id", testCycleId),
                     new JProperty("projectId", 53530),
                     new JProperty("name", testCycleName)
                     );
                string updatePayload = UnitTest2.CamelCasedJSON(updateObject);
                request.AddBody(updatePayload);
                IRestResponse response = client.Put(request);
            }
            var decodedUrl4FetchingExeId = HttpUtility.UrlDecode(string.Format("rest/tests/1.0/testrun/{0}/testrunitems/lasttestresults", testCycleId));
            RestRequest request4FetchingExeId = new RestRequest(decodedUrl4FetchingExeId);
            IRestResponse response4FetchingExeId = client.Get(request4FetchingExeId);
            JArray job = JArray.Parse(response4FetchingExeId.Content);
            JToken lastTestResultsJToken = job.SelectToken("$");
            string testcasesStatusJson = File.ReadAllText(testcasesStatusJsonFileLoc);
            JObject testcasesStatusJObject = JObject.Parse(testcasesStatusJson);

            foreach (JObject lastTestResultJObject in lastTestResultsJToken.Children<JObject>()) {
                JToken lastTestResultJToken = lastTestResultJObject.SelectToken("$.lastTestResult");
                string testCaseId = lastTestResultJToken["testCaseId"].ToString();
                int executionId = Convert.ToInt32(lastTestResultJToken["id"].ToString());
                string query = string.Format("$.results[?(@.testCaseId=={0})]", testCaseId);
                JToken matchedTestcaseToken = testcasesStatusJObject.SelectToken(query);
                matchedTestcaseToken["executionId"] = executionId;
            }
            string testcasesStatusJsonwithExecID = JsonConvert.SerializeObject(testcasesStatusJObject);
            File.WriteAllText(testcasesStatusJsonFileLoc, FormattedJson(testcasesStatusJsonwithExecID));
        }

        public void CreateDefect(string bugTitle, string stackTrace, string imageFullPath, int executionId) {
            var decode = HttpUtility.UrlDecode("rest/api/2/issue");
            RestRequest request = new RestRequest();
            JObject root = new JObject(
                new JProperty("fields", new JObject(
                    new JProperty("summary", bugTitle),
                    new JProperty("description", stackTrace),
                    new JProperty("environment", "environment details"),
                    new JProperty("customfield_14900", "repro steps"),
                    new JProperty("issuetype", new JObject(
                        new JProperty("id", "9"))),
                   new JProperty("project", new JObject(
                        new JProperty("id", "53530"))),
                   new JProperty("priority", new JObject(
                        new JProperty("id", "3"))),
                   new JProperty("customfield_10046", new JObject(
                        new JProperty("id", "10060"))),
                   new JProperty("customfield_10028", new JObject(
                        new JProperty("id", "50127"))),
                   new JProperty("customfield_15401", new JObject(
                        new JProperty("id", "-1"))),
                    new JProperty("customfield_10051", new JObject(
                        new JProperty("inputValues", new JArray("36323")))),
                     new JProperty("customfield_16001", new JObject(
                        new JProperty("inputValues", new JArray("1001150000")))),
                     new JProperty("labels", new JArray("AutomatedRegression")),
                     new JProperty("components", new JArray(
                         new JObject(
                             new JProperty("id", "31206")))),
                     new JProperty("reporter", new JObject(
                        new JProperty("id", "RUPESHKUMARSOMALA")))
                    )));
            string defectPayload = JsonConvert.SerializeObject(root);
            request.AddJsonBody(defectPayload);
            IRestResponse response = client.Execute(request, Method.POST);
            int issueId = 2569380;
            var decode1 = HttpUtility.UrlDecode(string.Format("rest/api/2/issue/{0}/attachments", issueId));
            RestRequest request1 = new RestRequest(decode1);
            request1.AddHeader("X-Atlassian-Token", "no-check");
            request1.AddHeader("Content-Type", "multipart/form-data");
            request1.AddFile("file", imageFullPath);
            IRestResponse restResponse = client.Post(request1);
            if (!bulkDefectsLinking.ContainsKey(executionId.ToString())) {
                bulkDefectsLinking.Add(executionId.ToString(), issueId.ToString());
            } else {
                bulkDefectsLinking[executionId.ToString()] = issueId.ToString();
            }
        }
        [SetUp]
        public void Setup() {
            Console.WriteLine("Test Setup");
            Console.WriteLine("Start video recording");
        }

        [Test]
        [Author("RKS"), Category("UI"), Category("Sanity")]
        [Property("Test", "PROJECTKEY-T275"), Description("Element will be hidden")]
        public void SOMEPROJECTT275_Test1() {
            Console.Beep();
            Thread.Sleep(2000);
            Console.WriteLine("Test1");
            Assert.Pass();
        }
        [Test]
        [Author("RKS"), Category("UI"), Category("Sanity")]
        [Property("Test", "PROJECTKEY-T276"), Description("Element will be hidden")]
        public void SOMEPROJECTT276_Test2() {
            Console.Beep();
            Console.WriteLine("Test2");
            Thread.Sleep(2000);
            Assert.Pass();
        }
        [Test]
        [Author("RKS"), Category("UI"), Category("Sanity")]
        [Property("Test", "PROJECTKEY-T277"), Description("Element will be hidden")]
        public void SOMEPROJECTT277_Test3() {
            Console.Beep();
            Console.WriteLine("Test3");
            Thread.Sleep(2000);
            // Assert.AreEqual("some expected desc", "some actual desc", "Description doesn't matched");
            Assert.Multiple(() => {
                //Assert.IsTrue(1 > 4, "Bool check failed");
                //Assert.AreEqual("s", "a", "string check failed");
                //Assert.Greater(3, 4, "Greater than failed");
                Assert.Greater(5, 4, "Greater than failed");
            });
        }
        [Test]
        [Author("RKS"), Category("UI"), Category("Sanity")]
        [Property("Test", "PROJECTKEY-T278"), Description("Element will be hidden")]
        public void SOMEPROJECTT278_Test4() {
            Console.Beep();
            Console.WriteLine("Test4");
            Thread.Sleep(2000);
            Assert.Pass();
        }

        [Test]
        [Author("RKS"), Category("UI")]
        [Property("Test", "YOURPROJECTKEY-T1037"), Description("Element will be hidden")]
        [Ignore("")]
        public void Test5() {
            JArray ja = new JArray();
            JValue name = new JValue("rupesh");
            JValue lastname = new JValue("somala");
            ja.Add(name);
            ja[0].AddBeforeSelf("before");
            Console.WriteLine(ja);
            ja[0].AddAfterSelf("after");
            Console.WriteLine(ja);
            JArray ja2 = new JArray(
                new JObject(
                    new JProperty("prop1", "prop1Value"),
                     new JProperty("prop2", "prop2Value")
                    ),
                new JObject(
                    new JProperty("prop1", "prop11Value"),
                     new JProperty("prop2", "prop22Value")
                    )
                );
            ja2.Add(new JValue("123"));
            Console.WriteLine(ja2);

            JObject ja3 = new JObject(
                new JProperty("results", new JArray(
                    ja2.Children<JObject>().Select(jo => new JObject(
                        new JProperty("key1", jo["prop1"]),
                        new JProperty("key2", jo["prop2"])
                        )))));
            Console.WriteLine(ja3);

            JObject ja4 = new JObject(
                new JProperty("results", new JArray(
                    ja2.Children()
                    ))
                );
            DescriptionAttribute some = (DescriptionAttribute)Attribute.GetCustomAttribute(typeof(UnitTest1), typeof(DescriptionAttribute));
            Console.WriteLine(TestContext.CurrentContext.Test.Properties.Get("Author"));
            Console.WriteLine(TestContext.CurrentContext.Test.Properties.Get("Category"));
            Console.WriteLine(TestContext.CurrentContext.Test.Properties.Get("Description"));
            Console.WriteLine(TestContext.CurrentContext.Test.Properties.Get("Test"));
            Console.WriteLine(ja4);
            Console.WriteLine("Test5");
            Assert.Fail();
        }
        [TearDown]
        public void Teardown() {
            string testCaseJiraId = TestContext.CurrentContext.Test.Properties.Get("Test").ToString();
            string bugTitle = TestContext.CurrentContext.Test.MethodName;
            int testCaseStatusEnum;
            string testCaseStatus;
            string expMessage = null;
            string imageLocation = null;

            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed) {
                testCaseStatusEnum = 2804;
                testCaseStatus = "P";
                bugTitle = null;
                passedCount++;
                Console.WriteLine("Stopped video recording and immediately deleted the execution video since it's already passed and due to space constraints");
            } else {
                testCaseStatusEnum = 2805;
                testCaseStatus = "F";
                StringBuilder prepareErrorMessage = new StringBuilder();
                prepareErrorMessage.Append(string.Format("Top Stacktrace:\n{0}", TestContext.CurrentContext.Result.StackTrace));
                prepareErrorMessage.AppendLine(string.Format("Top Message:\n{0}", TestContext.CurrentContext.Result.Message));
                if (TestContext.CurrentContext.Result.Assertions.ToList().Count > 1) {
                    foreach (var item in TestContext.CurrentContext.Result.Assertions) {
                        prepareErrorMessage.AppendLine($"Assertion Status: {item.Status}");
                        prepareErrorMessage.AppendLine($"Assertion Message: {item.Message}");
                        prepareErrorMessage.AppendLine($"Assertion StackTrace: {item.StackTrace}");
                    }
                }
                expMessage = prepareErrorMessage.ToString();
                imageLocation = @"C:\Users\RUPESHKUMARSOMALA\source\repos\XploreAutomationIntegrations\XploreAutomationIntegrations\video.mp4";
                bugTitle = bugTitle.Replace(testCaseJiraId.Replace("-", ""), "") + " failed";
                failedCount++;
                Console.WriteLine("Stopped video recording");
            }
            executedCount++;
            notExecutedCount = totalCount - executedCount;
            //Console.WriteLine(query);
            //JToken token = jObj.SelectToken(query);
            string testcasesStatusJson = File.ReadAllText(testcasesStatusJsonFileLoc);
            JObject testcasesStatusJObject = JObject.Parse(testcasesStatusJson);
            string query = string.Format("$.results[?(@.key=='{0}')]", testCaseJiraId);
            JToken matchedTestcaseToken = testcasesStatusJObject.SelectToken(query);
            int testCaseId = Convert.ToInt32(matchedTestcaseToken["testCaseId"].ToString());
            int executionId = Convert.ToInt32(matchedTestcaseToken["executionId"].ToString());
            JObject testcaseStatus = new JObject(
                    new JProperty("userKey", "RUPESHKUMARSOMALA"),
                    new JProperty("key", testCaseJiraId),
                    new JProperty("testCaseId", testCaseId),
                    new JProperty("executionId", executionId),
                    new JProperty("executionDate", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ")),
                    new JProperty("status", Convert.ToInt32(testCaseStatusEnum)),
                    new JProperty("bugTitle", bugTitle),
                    new JProperty("exception", expMessage),
                    new JProperty("imageLocation", imageLocation));
            string className = TestContext.CurrentContext.Test.ClassName;
            var testTuple = Tuple.Create(testCaseId, testCaseJiraId, executionId, testCaseStatus, className, TestContext.CurrentContext.Test.MethodName);
            omitClasses.Add(className);
            hardSoftList.Add(testTuple);
            testcasesStatus.Add(testcaseStatus);
            if (bugTitle != null) {
                CreateDefect(bugTitle, expMessage, imageLocation, executionId);
            }
            if (hardSoftList.Count > 2) {
                int lastIndex = hardSoftList.Count - 1;
                StringBuilder sequence = new StringBuilder();
                int counter = 0;
                for (int i = lastIndex; i > 0; i--) {
                    sequence.Append(hardSoftList[i].Item4);
                    counter++;
                    if (counter == 2) {
                        break;
                    }
                }
                if (sequence.ToString() == "FF") {
                    Console.WriteLine("Hard stop");
                    StringBuilder nunit3ExpressionBuilder = new StringBuilder();
                    StringBuilder dotnetTestExpressionBuilder = new StringBuilder();
                    HashSet<string> nunit3AtomicExpressions = new HashSet<string>();
                    HashSet<string> dotnetTestAtomicExpressions = new HashSet<string>();
                    HashSet<string> uniqueExcludeClasses = new HashSet<string>();
                    string currentNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace + ".";
                    foreach (var t in hardSoftList) {
                        omitClasses.Add(t.Item5.Replace(currentNamespace, ""));
                        uniqueExcludeClasses.Add(t.Item5.Replace(currentNamespace, ""));
                        nunit3AtomicExpressions.Add($"(class!={t.Item5.Replace(currentNamespace, "")})");
                        dotnetTestAtomicExpressions.Add($"(FullyQualifiedName!~{t.Item5.Replace(currentNamespace, "")})");
                    }
                    JArray excludeClasses = new JArray();
                    foreach (var item in uniqueExcludeClasses) {
                        excludeClasses.Add(item);
                    }
                    foreach (var item in nunit3AtomicExpressions) {
                        nunit3ExpressionBuilder.Append($"{item}&&");
                    }
                    foreach (var item in dotnetTestAtomicExpressions) {
                        dotnetTestExpressionBuilder.Append($"{item}&");
                    }
                    string nunit3Expression = nunit3ExpressionBuilder.ToString();
                    nunit3Expression = nunit3Expression.Remove(nunit3Expression.Length - 2);
                    string dotnetTestExpression = dotnetTestExpressionBuilder.ToString();
                    dotnetTestExpression = dotnetTestExpression.Remove(dotnetTestExpression.Length - 1);
                    string nunit3Command = $"nunit3-console \"{testDLLLoc}\" --where \"cat==Smoke && ({nunit3Expression})\"";
                    string dotnetTestCommand = $"dotnet test \"{testDLLLoc}\" --filter \"TestCategory=Smoke&({dotnetTestExpression})\" -l \"console;verbosity=detailed\"";
                    JObject stop = new JObject(
                       new JProperty("info", new JObject(
                           new JProperty("testCycleId", testCycleId),
                           new JProperty("testCycleName", testCycleName),
                           new JProperty("isItNewTestCycle", false),
                           new JProperty("excludeClasses", excludeClasses)
                    )));
                    File.WriteAllText(nunit3CommandLoc, nunit3Command);
                    File.WriteAllText(dotnetTestCommandLoc, dotnetTestCommand);
                    File.WriteAllText(continuationLoc, FormattedJson(JsonConvert.SerializeObject(stop)));
                    string nunitStopCommand = $"nunit3-console \"{testDLLLoc}\" --stoponerror";
                    string dotnetTestStopCommand = $"dotnet test \"{testDLLLoc}\" --stoponerror";
                    JArray jArray = new JArray();
                    foreach (var item in hardSoftList) {
                        int status = (item.Item4 == "P") ? 2804 : 2805;
                        //id, key,eid, status,class, Method
                        JObject j = new JObject(
                                        new JProperty("id", item.Item3),
                                        new JProperty("testResultStatusId", status),
                                        new JProperty("userKey", "RUPESHKUMARSOMALA"),
                                        new JProperty("executionDate", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ")),
                                        new JProperty("plannedStartDate", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ")),
                                        new JProperty("plannedEndDate", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ")),
                                        new JProperty("environmentId", null),
                                        new JProperty("iterationId", null),
                                        new JProperty("jiraVersionId", null)
                                 );
                        jArray.Add(j);
                    }
                    string decoded = HttpUtility.UrlDecode("rest/tests/1.0/testresult");
                    RestRequest request = new RestRequest(decoded);
                    string testResultsPayload = JsonConvert.SerializeObject(jArray);
                    request.AddJsonBody(testResultsPayload);
                    IRestResponse response = client.Put(request);
                    LinkDefect2Testcase();
                    Process.GetCurrentProcess().Kill();
                }
            }

            Console.WriteLine($"Execution status:{executedCount}/{totalCount}");
            Console.WriteLine($"Passed status:{passedCount}/{executedCount}");
            Console.WriteLine($"Failed status:{failedCount}/{executedCount}");
            Console.WriteLine("Test Teardown");
        }
        public void LinkDefect2Testcase() {
            var decode2 = HttpUtility.UrlDecode("rest/tests/1.0/tracelink/testresult/bulk/create");
            RestRequest request2 = new RestRequest(decode2);
            JArray jArray = new JArray();
            foreach (var item in bulkDefectsLinking) {
                JObject jobj = new JObject(
                    new JProperty("testResultId", item.Key),
                    new JProperty("issueId", item.Value),
                    new JProperty("typeId", 3));
                jArray.Add(jobj);
            }
            string bulkDefectsLinkingpayload = JsonConvert.SerializeObject(jArray);
            request2.AddJsonBody(bulkDefectsLinkingpayload);
            IRestResponse restResponse = client.Post(request2);
        }

        [OneTimeTearDown]
        public void TeardownFixture() {
            LinkDefect2Testcase();

            string hostName = Dns.GetHostName();
            string ip = Dns.GetHostByName(hostName).AddressList[0].ToString();
            testExeObject = new JObject(
                new JProperty("info", new JObject(
                    new JProperty("testCycleId", testCycleId),
                    new JProperty("testCycleName", testCycleName),
                    new JProperty("environment", "QA"),
                    new JProperty("category", "Smoke"),
                    new JProperty("passedStatus", $"{passedCount}/{executedCount}"),
                    new JProperty("failedStatus", $"{failedCount}/{executedCount}"),
                    new JProperty("platformName", "Android"),
                    new JProperty("platformVersion", "11.0"),
                    new JProperty("device", "Oneplus 7T"),
                    new JProperty("hostName", hostName),
                    new JProperty("ip", ip)
                    )),
               new JProperty("results", testcasesStatus));
            string platformName = "Android";
            string platformVersion = "11.0";
            string exeStatusLoc = string.Format($"{new DirectoryInfo("../../../").FullName}Results/testExe_{platformName}{platformVersion}_{DateTime.Now.ToString("yyyyMMdd'T'HHmmssfff")}.json");
            File.WriteAllText(exeStatusLoc, FormattedJson(JsonConvert.SerializeObject(testExeObject)));

            JToken jToken = testExeObject.SelectToken("$.results");
            JArray jarray4Payload = new JArray(
                jToken.Children<JObject>().Select((j) => new JObject {
                new JProperty("id",j["executionId"]),
                new JProperty("testResultStatusId", j["status"]),
                new JProperty("userKey", j["userKey"]),
                new JProperty("executionDate", j["executionDate"]),
                new JProperty("plannedStartDate", j["executionDate"]),
                new JProperty("plannedEndDate", j["executionDate"]),
                new JProperty("environmentId",null),
                new JProperty("iterationId", null),
                new JProperty("jiraVersionId",null)
                }));

            string decoded = HttpUtility.UrlDecode("rest/tests/1.0/testresult");
            RestRequest request = new RestRequest(decoded);
            string testResultsPayload = JsonConvert.SerializeObject(jarray4Payload);
            request.AddJsonBody(testResultsPayload);
            IRestResponse response = client.Put(request);

            string nunitResetCommand = $"nunit3-console \"{testDLLLoc}\" --where \"cat=={0}\"";
            File.WriteAllText(nunit3CommandLoc, nunitResetCommand);
            string dotnetTestResetCommand = $"dotnet test \"{testDLLLoc}\" --filter \"TestCategory={0}\" -l \"console;verbosity=detailed\"";
            File.WriteAllText(dotnetTestCommandLoc, dotnetTestResetCommand);
            string resetContinuation = File.ReadAllText(continuationLoc);
            JObject jobject = JObject.Parse(resetContinuation);
            jobject.SelectToken("$.info")["testCycleId"] = 0;
            jobject.SelectToken("$.info")["testCycleName"] = "";
            jobject.SelectToken("$.info")["isItNewTestCycle"] = true;
            jobject.SelectToken("$.info")["excludeClasses"] = new JArray();
            File.WriteAllText(continuationLoc, FormattedJson(JsonConvert.SerializeObject(jobject)));
            Console.WriteLine("Fixture Teardown");
        }
    }
}