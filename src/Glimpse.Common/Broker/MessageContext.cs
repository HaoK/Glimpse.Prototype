using System;
using Newtonsoft.Json;

namespace Glimpse
{
    public class MessageContext
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        [JsonIgnore]
        public OperationStack Operations { get; } = new OperationStack();
    }
}