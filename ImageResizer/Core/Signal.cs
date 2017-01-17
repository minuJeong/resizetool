using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer.Core
{
    public enum SignalKey
    {
        UPDATE_SELECTED_DIRECTORY = 0,

        TOOLS_RESIZE = 10,
        TOOLS_DBBUILD,
    }

    public class InvalidSignalDataException : Exception
    {
        public InvalidSignalDataException() : base() { }
        public InvalidSignalDataException(string message) : base(message) { }
        public InvalidSignalDataException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Static utility class for broadcast callback chain to entire application.
    /// </summary>
    public static class Signal
    {
        private static readonly Dictionary<SignalKey, List<Action<object>>>
            s_CallbackChain = new Dictionary<SignalKey, List<Action<object>>>();

        /// <summary>
        /// Add Signal Listener
        /// </summary>
        public static void Add(SignalKey key, Action<object> callback)
        {
            if (!s_CallbackChain.ContainsKey(key))
            {
                s_CallbackChain.Add(key, new List<Action<object>>());
            }

            s_CallbackChain[key].Add(callback);
        }

        /// <summary>
        /// Unlink Signal Listener
        /// </summary>
        public static void Remove(SignalKey key, Action<object> callback)
        {
            if (!s_CallbackChain.ContainsKey(key))
            {
                return;
            }

            if (s_CallbackChain[key].Contains(callback))
            {
                s_CallbackChain[key].Remove(callback);
            }
        }

        /// <summary>
        /// Invoke Signal Listeners
        /// </summary>
        public static void Dispatch(SignalKey key, object data)
        {
            // escape if signal key not added
            if (!s_CallbackChain.ContainsKey(key))
            {
                return;
            }

            // call callbacks
            s_CallbackChain[key].ForEach((action) =>
            {
                action(data);
            });
        }
    }
}
