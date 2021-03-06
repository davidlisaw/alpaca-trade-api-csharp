﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace Alpaca.Markets.Extensions
{
    /// <summary>
    /// Helper extension method for creating special version of the <see cref="IAlpacaDataStreamingClient"/>
    /// implementation with automatic reconnection (with configurable delay and number of attempts) support.
    /// </summary>
    public static class AlpacaDataStreamingClientExtensions
    {
        private sealed class ClientWithReconnection :
            ClientWithReconnectBase<IAlpacaDataStreamingClient, IAlpacaDataSubscription>,
            IAlpacaDataStreamingClient
        {
            public ClientWithReconnection(
                IAlpacaDataStreamingClient client,
                ReconnectionParameters reconnectionParameters)
                : base (client, reconnectionParameters)
            {
            }

            public IAlpacaDataSubscription<IStreamAgg> GetMinuteAggSubscription() =>
                Client.GetMinuteAggSubscription();

            public IAlpacaDataSubscription<IStreamTrade> GetTradeSubscription(String symbol) =>
                Client.GetTradeSubscription(symbol);

            public IAlpacaDataSubscription<IStreamQuote> GetQuoteSubscription(String symbol) =>
                Client.GetQuoteSubscription(symbol);

            public IAlpacaDataSubscription<IStreamAgg> GetMinuteAggSubscription(String symbol) =>
                Client.GetMinuteAggSubscription(symbol);

            public void Subscribe(
                IAlpacaDataSubscription subscription)
            {
                Subscriptions.TryAdd(subscription.Stream, subscription);
                Client.Subscribe(subscription);
            }

            public void Subscribe(
                IEnumerable<IAlpacaDataSubscription> subscriptions) => 
                Subscribe(subscriptions.ToArray());

            public void Subscribe(
                params IAlpacaDataSubscription[] subscriptions)
            {
                foreach (var subscription in subscriptions)
                {
                    Subscriptions.TryAdd(subscription.Stream, subscription);
                }
                Client.Subscribe(subscriptions);
            }

            public void Unsubscribe(
                IAlpacaDataSubscription subscription)
            {
                Subscriptions.TryRemove(subscription.Stream, out _);
                Client.Unsubscribe(subscription);
            }

            public void Unsubscribe(
                IEnumerable<IAlpacaDataSubscription> subscriptions) =>
                Unsubscribe(subscriptions.ToArray());

            public void Unsubscribe(
                params IAlpacaDataSubscription[] subscriptions)
            {
                foreach (var subscription in subscriptions)
                {
                    Subscriptions.TryRemove(subscription.Stream, out _);
                }
                Client.Unsubscribe(subscriptions);
            }

            protected override void Resubscribe(
                String symbol, 
                IAlpacaDataSubscription subscription)
            {
                Client.Subscribe(subscription);
            }
        }

        /// <summary>
        /// Wraps instance of <see cref="IAlpacaDataStreamingClient"/> into the helper class
        /// with automatic reconnection support and provide optional reconnection parameters.
        /// </summary>
        /// <param name="client">Original streaming client for wrapping.</param>
        /// <param name="parameters">Reconnection parameters (or default if missing).</param>
        /// <returns>Wrapped version of the <paramref name="client"/> object with reconnect.</returns>
        public static IAlpacaDataStreamingClient WithReconnect(
            this IAlpacaDataStreamingClient client,
            ReconnectionParameters? parameters = null) =>
            new ClientWithReconnection(client, parameters ?? ReconnectionParameters.Default);
    }
}
