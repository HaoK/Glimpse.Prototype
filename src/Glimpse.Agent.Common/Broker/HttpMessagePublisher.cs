﻿using System;
using System.Net.Http;

namespace Glimpse.Agent
{
    public class HttpMessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClientHandler _httpHandler;

        public HttpMessagePublisher()
        {
            _httpHandler = new HttpClientHandler();
            _httpClient = new HttpClient(_httpHandler);
        }

        public async void PublishMessage(IMessage message)
        {
            // TODO: Needs error handelling
            // TODO: Find out what happened to System.Net.Http.Formmating - PostAsJsonAsync
            try
            {
                var response = await _httpClient.PostAsJsonAsync("http://localhost:5210/Glimpse/AgentMessage", message);

                // Check that response was successful or throw exception
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                // TODO: Bad thing happened
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _httpHandler.Dispose();
        }
    }
}