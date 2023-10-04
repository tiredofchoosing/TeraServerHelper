using System.Net;
using System.Text;
using System.Text.Json;

namespace TeraPartyMonitor.DataSender
{
    internal class Requester
    {
        private readonly string _url;
        private readonly HttpClient _client;
        private delegate Task<HttpResponseMessage> Request(StringContent content);

        public event Action<StringContent> RequestSending;
        public event Action<bool, string?> ResponseReceived;

        public Requester(string url)
        {
            _client = new();
            _url = url;
        }

        /// <summary>
        /// Отправляет Http запрос к api на добавление записи в таблицу БД.
        /// </summary>
        /// <returns>true при успешно выполненном запросе, иначе false.</returns>
        public async Task<bool> CreateAsync<T>(T entity)
        {
            Request request = async (content) => await _client.PostAsync(_url, content);

            string jsonString = JsonSerializer.Serialize(entity);
            StringContent content = new(jsonString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await HandleRequest(content, request);
            return response != null && response.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Выполняет отправку запроса и проверку результата.
        /// Показывает сообщение об ошибке в случае обработки исключения.
        /// </summary>
        /// <param name="url">Url адрес.</param>
        /// <param name="content">Json тело запроса.</param>
        /// <param name="request">Делегат, содержащий Http запрос.</param>
        /// <returns>Http ответ сервера при успешно выполненном запросе, иначе null.</returns>
        private async Task<HttpResponseMessage> HandleRequest(StringContent content, Request request)
        {
            RequestSending?.Invoke(content);

            try
            {
                HttpResponseMessage response = (await request(content)).EnsureSuccessStatusCode();
                ResponseReceived?.Invoke(true, null);
                return response;
            }
            catch (Exception e)
            {
                ResponseReceived?.Invoke(false, e.Message);
                return null;
            }
        }
    }
}
