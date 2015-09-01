﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Glimpse.Common.Extensions;

namespace Glimpse
{
    public class DefaultMessageConverter : IMessageConverter
    {
        private readonly JsonSerializer _jsonSerializer;
        private readonly IContextData<MessageContext> _contextData;

        private readonly static Type _objectType = typeof(object);
        private readonly static Type _dictionaryType = typeof(Dictionary<string, object>);
        private readonly static MethodInfo _addMethodInfo = _dictionaryType.GetMethod("Add", new[] { typeof(string), typeof(object) });
        private readonly static ConstructorInfo _constructorInfo = typeof(ReadOnlyDictionary<string, object>).GetConstructor(new [] { _dictionaryType });
        private readonly static IDictionary<Type, Func<object, IReadOnlyDictionary<string, object>>> _methodCache = new Dictionary<Type, Func<object, IReadOnlyDictionary<string, object>>>();

        private readonly static Type[] _exclusions = { typeof(object) };


        public DefaultMessageConverter(JsonSerializer jsonSerializer, IContextData<MessageContext> contextData)
        {
            jsonSerializer.ContractResolver = new CamelCasePropertyNamesContractResolver();

            _jsonSerializer = jsonSerializer;
            _contextData = contextData;
        }

        public IMessage ConvertMessage(object payload)
        {
            var message = new Message
            {
                Id = Guid.NewGuid(),
                Types = GetTypes(payload),
                Payload = _jsonSerializer.Serialize(payload),
                Context = _contextData.Value,
                Indices = GetIndices(payload)
            };

            return message;
        }

        private static IEnumerable<string> GetTypes(object payload)
        {
            var typeInfo = payload.GetType().GetTypeInfo();

            return typeInfo.BaseTypes(includeSelf: true)
                .Concat(typeInfo.ImplementedInterfaces)
                .Except(_exclusions)
                .Select(t => t.KebabCase());
        }

        private static IReadOnlyDictionary<string, object> GetIndices(object payload)
        {
            var payloadType = payload.GetType();
            Func<object, IReadOnlyDictionary<string, object>> indicesCreator;

            if (_methodCache.ContainsKey(payloadType))
            {
                indicesCreator = _methodCache[payloadType];
            }
            else
            {
                indicesCreator = GenerateIndicesCreator(payloadType);
                _methodCache.Add(payloadType, indicesCreator);
            }

            return indicesCreator(payload);
        }

        private static Func<object, IReadOnlyDictionary<string, object>> GenerateIndicesCreator(Type messageType)
        {
            var parameter = Expression.Parameter(_objectType, "message");
            var variable = Expression.Variable(messageType, "casted");
            var cast = Expression.Assign(variable, Expression.Convert(parameter, messageType));

            var items = new List<ElementInit>();

            foreach (var property in messageType.GetProperties())
            {
                var attribute =
                    property.GetCustomAttributes(typeof(PromoteToAttribute), true)
                        .Cast<PromoteToAttribute>()
                        .SingleOrDefault();
                if (attribute != null)
                    items.Add(Expression.ElementInit(_addMethodInfo, Expression.Constant(attribute.Key),
                        Expression.Convert(Expression.Property(variable, property.Name), _objectType)));
            }

            var ctor = Expression.New(_dictionaryType);
            var init = Expression.ListInit(ctor, items);
            var wrapped = Expression.New(_constructorInfo, init);

            var code = Expression.Block(new[] { variable }, cast, init, wrapped);

            var lambda = Expression.Lambda<Func<object, IReadOnlyDictionary<string, object>>>(code, parameter);
            return lambda.Compile();
        }
    }
}