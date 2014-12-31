using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.N1QL;
using NUnit.Framework;

namespace Couchbase.Tests.N1QL
{
    [TestFixture]
    public class QueryRequestTests
    {
        [Test]
        public void Test_Statement()
        {
            var query = new QueryRequest().
                BaseUri(new Uri("http://192.168.30.101:8093/query")).
                Statement("SELECT * FROM default");

            var uri = query.GetQuery();
            Assert.IsTrue(uri.ToString().Contains(":8093/query?statement=SELECT * FROM default"));
            Console.WriteLine(uri);
        }

        [Test]
        public void Test_Statement_ClientContextId()
        {
            var query = new QueryRequest().
                BaseUri(new Uri("http://192.168.30.101:8093/query")).
                Statement("SELECT * FROM default").
                ClientContextId("somecontextlessthanorequalto64chars");

            var uri = query.GetQuery();
            Console.WriteLine(uri);
            Assert.IsTrue(uri.ToString().Contains(":8093/query?statement=SELECT * FROM default&client_context_id=somecontextlessthanorequalto64chars"));
        }

        [Test]
        public void Test_Statement_ClientContextId_Pretty()
        {
            var query = new QueryRequest().
                BaseUri(new Uri("http://192.168.30.101:8093/query")).
                Statement("SELECT * FROM default").
                ClientContextId("somecontextlessthanorequalto64chars").
                Pretty(true);

            var uri = query.GetQuery();
            Console.WriteLine(uri);
            Assert.IsTrue(uri.ToString().Contains(":8093/query?statement=SELECT * FROM default&pretty=true&client_context_id=somecontextlessthanorequalto64chars"));
        }

        [Test]
        public void Test_Positional_Parameters()
        {
            var query = new QueryRequest().
                BaseUri(new Uri("http://192.168.30.101:8093/query")).
                Statement("SELECT * FROM default WHERE type=$1").
                AddPositionalParameter("dog");

            var uri = query.GetQuery();
            Console.WriteLine(uri);
            Assert.IsTrue(uri.ToString().Contains(":8093/query?statement=SELECT * FROM default WHERE type%3D%241&args=[\"dog\"]"));
        }

        [Test]
        public void Test_Positional_Parameters_Two_Arguments()
        {
            var query = new QueryRequest().
                BaseUri(new Uri("http://192.168.30.101:8093/query")).
                Statement("SELECT * FROM default WHERE type=$1 OR type=$2").
                AddPositionalParameter("dog").
                AddPositionalParameter("cat");

            var uri = query.GetQuery();
            Console.WriteLine(uri);
            Assert.IsTrue(uri.ToString().Contains(":8093/query?statement=SELECT * FROM default WHERE type%3D%241 OR type%3D%242&args=[\"dog\"%2C\"cat\"]"));
        }

        [Test]
        public void Test_Named_Parameters_Two_Arguments()
        {
            var query = new QueryRequest().
                BaseUri(new Uri("http://192.168.30.101:8093/query")).
                Statement("SELECT * FROM default WHERE type=$canine OR type=$feline").
                AddNamedParameter("canine", "dog").
                AddNamedParameter("feline", "cat");

            var uri = query.GetQuery();
            Console.WriteLine(uri);
            Assert.IsTrue(uri.ToString().Contains(":8093/query?statement=SELECT * FROM default WHERE type%3D%24canine OR type%3D%24feline&$canine=\"dog\"&$feline=\"cat\""));
        }

        [Test]
        public void When_isAdmin_Is_True_Credentials_Contains_admin()
        {
            var query = new QueryRequest().
                BaseUri(new Uri("http://192.168.30.101:8093/query")).
                Statement("SELECT * FROM authenticated").
                AddCredentials("authenticated", "secret", true);

            var uri = query.GetQuery();
            Console.WriteLine(uri);
            Assert.IsTrue(uri.ToString().Contains(":8093/query?statement=SELECT * FROM authenticated&creds=[{\"user\":\"admin:authenticated\"%2C\"pass\":\"secret\"}]"));
        }

        [Test]
        public void When_isAdmin_Is_False_Credentials_Contains_local()
        {
            var query = new QueryRequest().
                BaseUri(new Uri("http://192.168.30.101:8093/query")).
                Statement("SELECT * FROM authenticated").
                AddCredentials("authenticated", "secret", false);

            var uri = query.GetQuery();
            Console.WriteLine(uri);
            Assert.IsTrue(uri.ToString().Contains(":8093/query?statement=SELECT * FROM authenticated&creds=[{\"user\":\"local:authenticated\"%2C\"pass\":\"secret\"}]"));
        }

        [Test]
        public void When_Username_Is_Empty_AddCredentials_Throws_AOOE()
        {
           var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new QueryRequest().
                BaseUri(new Uri("http://192.168.30.101:8093/query")).
                Statement("SELECT * FROM authenticated").
                AddCredentials("", "secret", false));

           Assert.That(ex.Message, Is.EqualTo("cannot be null, empty or whitespace.\r\nParameter name: username"));
        }

        [Test]
        public void When_Username_Is_Whitespace_AddCredentials_Throws_AOOE()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new QueryRequest().
                 BaseUri(new Uri("http://192.168.30.101:8093/query")).
                 Statement("SELECT * FROM authenticated").
                 AddCredentials(" ", "secret", false));

            Assert.That(ex.Message, Is.EqualTo("cannot be null, empty or whitespace.\r\nParameter name: username"));
        }

        [Test]
        public void When_Username_Is_Null_AddCredentials_Throws_AOOE()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new QueryRequest().
                 BaseUri(new Uri("http://192.168.30.101:8093/query")).
                 Statement("SELECT * FROM authenticated").
                 AddCredentials(null, "secret", false));

            Assert.That(ex.Message, Is.EqualTo("cannot be null, empty or whitespace.\r\nParameter name: username"));
        }
    }
}
