using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Couchbase.N1QL
{
    public class QueryRequest : IQueryRequest
    {
        private HttpMethod _method;
        private string _statement;
        private string _preparedStatement;
        private TimeSpan? _timeOut = TimeSpan.Zero;
        private bool? _readOnly;
        private bool? _includeMetrics;
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private readonly List<object> _arguments = new List<object>();
        private Format? _format;
        private Encoding? _encoding;
        private Compression? _compression;
        private ScanConsistency? _scanConsistency;
        private bool? _includeSignature;
        private dynamic _scanVector;
        private TimeSpan? _scanWait;
        private bool _pretty = false;
        private readonly Dictionary<string, string> _credentials = new Dictionary<string, string>();
        private string _clientContextId;
        private Uri _baseUri;

        public const string ForwardSlash = "/";
        public const string QueryOperator = "?";
        private const string QueryArgPattern = "{0}={1}&";
        private const string ParameterIdentifier = "$";
        private const string LowerCaseTrue = "true";
        private const string LowerCaseFalse = "false";

        private struct QueryParameters
        {
            public const string Statement = "statement";
            public const string Prepared = "prepared";
            public const string Timeout = "timeout";
            public const string Readonly = "readonly";
            public const string Metrics = "metrics";
            public const string Args = "args";
            public const string BatchArgs = "batch_args";
            public const string BatchNamedArgs = "batch_named_args";
            public const string Format = "format";
            public const string Encoding = "encoding";
            public const string Compression = "compression";
            public const string Signature = "signature";
            public const string ScanConsistency = "scan_consistency";
            public const string ScanVector = "scan_vector";
            public const string ScanWait = "scan_wait";
            public const string Pretty = "pretty";
            public const string Creds = "creds";
            public const string ClientContextId = "client_context_id";
        }

        public bool IsPost
        {
            get { return _method == N1QL.HttpMethod.Post; }
        }

        public IQueryRequest HttpMethod(HttpMethod method)
        {
            _method = method;
            return this;
        }

        public IQueryRequest Statement(string statement)
        {
            if (!string.IsNullOrWhiteSpace(_preparedStatement))
            {
                throw new ArgumentException("A prepared statement has already been provided.");
            }
            _statement = statement;
            return this;
        }

        public IQueryRequest PreparedStatement(string preparedStatement)
        {
            if (!string.IsNullOrWhiteSpace(_statement))
            {
                throw new ArgumentException("A statement has already been provided.");
            }
            _preparedStatement = preparedStatement;
            return this;
        }

        public IQueryRequest Timeout(TimeSpan timeOut)
        {
            _timeOut = timeOut;
            return this;
        }

        public IQueryRequest ReadOnly(bool readOnly)
        {
            _readOnly = readOnly;
            return this;
        }

        public IQueryRequest Metrics(bool includeMetrics)
        {
            _includeMetrics = includeMetrics;
            return this;
        }

        public IQueryRequest AddNamedParameter(string name, object value)
        {
            _parameters.Add(name, value);
            return this;
        }

        public IQueryRequest AddPositionalParameter(object value)
        {
            _arguments.Add(value);
            return this;
        }

        public IQueryRequest AddNamedParameter(params KeyValuePair<string, object>[] parameters)
        {
            foreach (var parameter in parameters)
            {
                _parameters.Add(parameter.Key, parameter.Value);
            }
            return this;
        }

        public IQueryRequest AddPositionalParameter(params object[] parameters)
        {
            foreach (var parameter in parameters)
            {
                _arguments.Add(parameter);
            }
            return this;
        }

        public IQueryRequest Format(Format format)
        {
            _format = format;
            return this;
        }

        public IQueryRequest Encoding(Encoding encoding)
        {
            _encoding = encoding;
            return this;
        }

        public IQueryRequest Compression(Compression compression)
        {
            _compression = compression;
            return this;
        }

        public IQueryRequest Signature(bool includeSignature)
        {
            _includeSignature = includeSignature;
            return this;
        }

        public IQueryRequest ScanConsistency(ScanConsistency scanConsistency)
        {
            _scanConsistency = scanConsistency;
            return this;
        }

        public IQueryRequest ScanVector(dynamic scanVector)
        {
            _scanVector = scanVector;
            return this;
        }

        public IQueryRequest ScanWait(TimeSpan scanWait)
        {
            _scanWait = scanWait;
            return this;
        }

        public IQueryRequest Pretty(bool pretty)
        {
            _pretty = pretty;
            return this;
        }

        public IQueryRequest AddCredentials(string username, string password, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentOutOfRangeException("username", "cannot be null, empty or whitespace.");
            }
            if (isAdmin && !username.StartsWith("admin:"))
            {
                username = "admin:" + username;
            }
            else if(!username.StartsWith("local:"))
            {
                username = "local:" + username;
            }
            _credentials.Add(username, password);
            return this;
        }

        public IQueryRequest ClientContextId(string clientContextId)
        {
            _clientContextId = clientContextId;
            return this;
        }

        public IQueryRequest BaseUri(Uri baseUri)
        {
            _baseUri = baseUri;
            return this;
        }

        void CheckMethod()
        {
            if (_method == N1QL.HttpMethod.None)
            {
                if (!string.IsNullOrWhiteSpace(_statement))
                {
                    var statement = _statement.ToLower();
                    _method = statement.Contains("SELECT") || statement.Contains("select")
                        ? N1QL.HttpMethod.Get
                        : N1QL.HttpMethod.Post;
                }
                else
                {
                    var preparedStatement = _preparedStatement.ToLower();
                    _method = preparedStatement.Contains("SELECT") || preparedStatement.Contains("select")
                        ? N1QL.HttpMethod.Get
                        : N1QL.HttpMethod.Post;
                }
            }
        }

        public Uri GetQuery()
        {
            if (string.IsNullOrWhiteSpace(_statement) && string.IsNullOrWhiteSpace(_preparedStatement))
            {
                throw new ArgumentException("A statement or prepared statement must be provided, but not both.");
            }
            CheckMethod();

            //build the request query starting with the base uri- e.g. http://localhost:8093/query
            var sb = new StringBuilder();
            sb.Append(_baseUri + "?");

            if (!string.IsNullOrEmpty(_statement))
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Statement, Uri.EscapeDataString(_statement));
            }
            if (!string.IsNullOrEmpty(_preparedStatement))
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Prepared, _preparedStatement);
            }
            if (_timeOut.HasValue && _timeOut.Value > TimeSpan.Zero)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Timeout, _timeOut);
            }
            if (_readOnly.HasValue && _readOnly.Value)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Readonly, _readOnly.Value ? LowerCaseTrue : LowerCaseFalse);
            }
            if (_includeMetrics.HasValue && _includeMetrics.Value)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Metrics, _includeMetrics.Value ? LowerCaseTrue : LowerCaseFalse);
            }
            if (_parameters.Count > 0)
            {
                foreach (var parameter in _parameters)
                {
                    sb.AppendFormat(QueryArgPattern,
                       parameter.Key.Contains("$") ? parameter.Key : "$" + parameter.Key,
                       EncodeParameter( parameter.Value));
                }
            }
            if (_arguments.Count > 0)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Args, EncodeParameter(_arguments));
            }
            if (_format.HasValue)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Format, _format);
            }
            if (_encoding.HasValue)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Encoding, _encoding);
            }
            if (_compression.HasValue)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Compression, _compression);
            }
            if(_includeSignature.HasValue && _includeSignature.Value)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Signature, _includeSignature.Value ? LowerCaseTrue : LowerCaseFalse);
            }
            if (_scanConsistency.HasValue)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.ScanConsistency, _scanConsistency);
            }
            if (_scanVector != null)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.ScanVector, _scanVector);
            }
            if (_scanWait.HasValue)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.ScanWait, _scanWait);
            }
            if (_pretty)
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.Pretty, _pretty ? LowerCaseTrue : LowerCaseFalse);
            }
            if (_credentials.Count > 0)
            {
                var creds = new List<dynamic>();
                foreach (var credential in _credentials)
                {
                    creds.Add(new {user=credential.Key, pass=credential.Value});
                }
                sb.AppendFormat(QueryArgPattern, QueryParameters.Creds, EncodeParameter(creds));
            }
            if (!string.IsNullOrEmpty(_clientContextId))
            {
                sb.AppendFormat(QueryArgPattern, QueryParameters.ClientContextId, _clientContextId);
            }
            return new Uri(sb.ToString().TrimEnd('&'));
        }

        /// <summary>
        /// JSON encodes the parameter and URI escapes the input parameter.
        /// </summary>
        /// <param name="parameter">The parameter to encode.</param>
        /// <returns>A JSON and URI escaped copy of the parameter.</returns>
        static string EncodeParameter(object parameter)
        {
            return Uri.EscapeDataString(JsonConvert.SerializeObject(parameter));
        }

        public static IQueryRequest Create()
        {
            return new QueryRequest();
        }

        public static IQueryRequest Create(string statement, bool isPrepared)
        {
            return isPrepared ? new QueryRequest().PreparedStatement(statement)
                : new QueryRequest().Statement(statement);
        }

        public override string ToString()
        {
            string request;
            try
            {
                request = GetQuery().ToString();
            }
            catch
            {
                request = string.Empty;
            }
            return request;
        }
    }
}
