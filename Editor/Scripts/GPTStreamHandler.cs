using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text;
using UnityEngine;

namespace UMLAutoGenerator
{
    // https://platform.openai.com/docs/api-reference/chat/create
    public class GPTStreamHandler
    {
        private const string API_URL = "https://api.openai.com/v1/chat/completions";
        public static string apiKey;
        private static HttpClient httpClient;

        public static async IAsyncEnumerable<ResponseChunkData> CreateCompletionRequestAsStream(RequestData requestData,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (httpClient == null)
                httpClient = new HttpClient();

            if (string.IsNullOrEmpty(apiKey))
                throw new AggregateException("apiKey is null");
            
            if (!requestData.stream)
            {
                throw new AggregateException("stream must be true.");
            }

            var json = JsonUtility.ToJson(requestData);

            using var request = new HttpRequestMessage(HttpMethod.Post, API_URL);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            using var response =
                await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                cancellationToken.ThrowIfCancellationRequested();

                using var reader = new StreamReader(stream, Encoding.UTF8);

                string line = null;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (string.IsNullOrEmpty(line)) continue;
                    var chunkString = line.Substring(6); // "data: "
                    if (chunkString == "[DONE]") break;
                    yield return JsonUtility.FromJson<ResponseChunkData>(chunkString);
                }
            }
            else
            {
                var message = await response.Content.ReadAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();
                throw new WebException($"request failed. {(int)response.StatusCode} {response.StatusCode}\n{message}");
            }
        }

        public static async Task<HttpStatusCode> CheckAPIStatus(RequestData requestData, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (httpClient == null)
                httpClient = new HttpClient();

            var json = JsonUtility.ToJson(requestData);

            using var request = new HttpRequestMessage(HttpMethod.Post, API_URL);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return response.StatusCode;
        }

        [System.Serializable]
        public class RequestData
        {
            public string model = "gpt-3.5-turbo-1106";
            public List<Message> messages;
            public float temperature; // [0.0 - 2.0]
            public float top_p;
            public bool stream = true;
            public List<string> stop = null;
            public int max_tokens = 4096;
            public float presence_penalty;
            public float frequency_penalty;
            public Dictionary<int, int> logit_bias = null;
            public string user = null;
        }

        [System.Serializable]
        public class Message
        {
            public string role;
            public string content;
        }

        [System.Serializable]
        public class ChunkChoice
        {
            public Message delta;
            public int index;
            public object finish_reason;
        }

        [System.Serializable]
        public class ResponseChunkData
        {
            public string id;
            public string @object;
            public int created;
            public string model;
            public List<ChunkChoice> choices;
        }
    }
}