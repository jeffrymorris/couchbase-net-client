using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Configuration.Client;
using Couchbase.N1QL;
using Couchbase.Tests.Documents;
using Couchbase.Views;
using NUnit.Framework;

namespace Couchbase.Tests.N1QL
{
    [TestFixture]
    public class QueryClientTests
    {
        private string server = "192.168.30.101";
        [Test]
        public void TestQuery_HelloWorld()
        {
            var config = new ClientConfiguration();
            var client = new QueryClient(new HttpClient(), new JsonDataMapper(config), config);
            var uri = new Uri(string.Format("http://{0}:8093/query", server));
            const string query = "SELECT 'Hello World' AS Greeting";

            var result = client.Query<dynamic>(uri, query);
            Assert.IsNotNull(result);
            Assert.AreEqual("Hello World", result.Rows.First().Greeting.ToString());
        }

        [Test]
        public void TestQuery_Incorrect_Syntax()
        {
            var config = new ClientConfiguration();
            var client = new QueryClient(new HttpClient(), new JsonDataMapper(config), config);
            var uri = new Uri(string.Format("http://{0}:8093/query", server));
            const string query = "SELECT 'Hello World' ASB Greeting";

            var result = client.Query<dynamic>(uri, query);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsEmpty(result.Rows);
        }

        //SELECT c.children FROM tutorial as c
        [Test]
        public void TestQuery_Select_Children_Dynamic()
        {
            var config = new ClientConfiguration();
            var client = new QueryClient(new HttpClient(), new JsonDataMapper(config), config);
            var uri = new Uri(string.Format("http://{0}:8093/query", server));
            const string query = "SELECT c.children FROM tutorial as c";

            var result = client.Query<dynamic>(uri, query);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Rows);
        }

        //SELECT c.children FROM tutorial as c
        [Test]
        public void TestQuery_Select_Children_Poco()
        {
            var config = new ClientConfiguration();
            var client = new QueryClient(new HttpClient(), new JsonDataMapper(config), config);
            var uri = new Uri(string.Format("http://{0}:8093/query", server));
            const string query = "SELECT c.children FROM tutorial as c";

            var result = client.Query<Contact>(uri, query);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Rows);
        }

        [Test]
        public void TestQuery_Select_All()
        {
            var config = new ClientConfiguration();
            var client = new QueryClient(new HttpClient(), new JsonDataMapper(config), config);
            var uri = new Uri(string.Format("http://{0}:8093/query", server));
            const string query = "SELECT * FROM default as d";

            var result = client.Query<dynamic>(uri, query);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Rows);
        }

        [Test]
        public void TestQuery_Select_All_Where_Cat()
        {
            var config = new ClientConfiguration();
            var client = new QueryClient(new HttpClient(), new JsonDataMapper(config), config);
            var uri = new Uri(string.Format("http://{0}:8093/query", server));
            const string query = "SELECT type, meta FROM default as d WHERE d.type='cat'";

            var result = client.Query<dynamic>(uri, query);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Rows);
        }

        [Test]
        public void Test_Create_Index()
        {
            var config = new ClientConfiguration();
            var client = new QueryClient(new HttpClient(), new JsonDataMapper(config), config);
            var uri = new Uri(string.Format("http://{0}:8093/query", server));
            const string query = "CREATE PRIMARY INDEX ON `authenticated`";

            var result = client.Query<dynamic>(uri, query);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Rows);
        }

        [Test]
        public void Test_All()
        {
            var config = new ClientConfiguration();
            var client = new QueryClient(new HttpClient(), new JsonDataMapper(config), config);
            var request = QueryRequest.Create("SELECT * from `beer-sample` WHERE type=$1 LIMIT 10", false).
                BaseUri(new Uri(string.Format("http://{0}:8093/query", server))).
                Pretty(false).
                AddPositionalParameter("beer");

            var result = client.Query<dynamic>(request);
            foreach (var row in result.Rows)
            {
                Console.WriteLine(row);
            }
        }
    }
}

#region [ License information          ]

/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2014 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/

#endregion