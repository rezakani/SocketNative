// Author: Mohammad Reza Kani 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System.Collections.Generic;
using System.Net.WebSockets;

namespace SocketNative.Models
{
    /// <summary>
    /// A container instance of an object created from a socket
    /// </summary>
    public sealed class ModuleSocket
    {
        private object _model;
        /// <summary>
        /// An instance is from the System.Net.WebSockets.WebSocket class
        /// </summary>
        public WebSocket Socket { get; set; }

        /// <summary>
        /// The QueryString set contains the values of the HTTP query string variables.
        /// </summary>
        public Dictionary<string, string> RequestQueryString { get; set; }

        /// <summary>
        /// With the key, it returns the value of the querystring corresponding to it
        /// </summary>
        /// <param name="key">It is of string type</param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            string returnValue = null;

            if (RequestQueryString.ContainsKey(key))
                RequestQueryString.TryGetValue(key, out returnValue);

            return returnValue;
        }

        /// <summary>
        /// Stores an instance of your model in the socket container
        /// </summary>
        /// <typeparam name="T">The value of T is of class type</typeparam>
        /// <param name="model">The value of model can be an example of any class</param>
        public void SetModel<T>(T model) where T : class
        {
            _model = model;
        }

        /// <summary>
        /// Get your model
        /// </summary>
        /// <typeparam name="T">The value of T is of class type</typeparam>
        /// <returns></returns>
        public T GetModel<T>()
        {
            return (T)_model;
        }

    }
}
