using System;
using System.Collections.Generic;
using System.Reflection;

namespace SiteMapUrlChecker.Misc
{
    
    /// <summary>
    /// Provides loosely-coupled messaging between
    /// various colleague objects. All references to objects
    /// are stored weakly, to prevent memory leaks.
    /// </summary>
    public class Messenger
    {
        public Messenger()
        {
        }



        /// <summary>
        /// Registers a callback method to be invoked when a specific message is broadcasted.
        /// </summary>
        /// <param name="message">The message to register for.</param>
        /// <param name="callback">The callback to be called when this message is broadcasted.</param>
        public void Register(string message, Delegate callback)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("'message' cannot be null or empty.");



            if (callback == null)
                throw new ArgumentNullException("callback");



            ParameterInfo[] parameters = callback.Method.GetParameters();



            if (parameters != null && parameters.Length > 1)
                throw new InvalidOperationException("The registered delegate can have no more than one parameter.");



            Type parameterType = (parameters == null || parameters.Length == 0) ? null : parameters[0].ParameterType;

            _messageToActionsMap.AddAction(message, callback.Target, callback.Method, parameterType);
        }



        /// <summary>
        /// Notifies all registered parties that a message is being broadcasted.
        /// </summary>
        /// <param name="message">The message to broadcast.</param>
        public void NotifyColleagues(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("'message' cannot be null or empty.");



            var actions = _messageToActionsMap.GetActions(message);



            if (actions != null)
                actions.ForEach(action => action.DynamicInvoke());
        }



        /// <summary>
        /// Notifies all registered parties that a message is being broadcasted.
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        /// <param name="parameter">The parameter to pass together with the message</param>
        public void NotifyColleagues(string message, object parameter)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("'message' cannot be null or empty.");



            var actions = _messageToActionsMap.GetActions(message);



            if (actions != null)
                actions.ForEach(action => action.DynamicInvoke(parameter));
        }



        /// <summary>
        /// This class is an implementation detail of the Messenger class.
        /// </summary>
        private class MessageToActionsMap
        {

            // Stores a hash where the key is the message and the value is the list of callbacks to invoke.

            private readonly Dictionary<string, List<WeakAction>> _map = new Dictionary<string, List<WeakAction>>();



            internal MessageToActionsMap()
            {
            }



            /// <summary>
            /// Adds an action to the list.
            /// </summary>
            /// <param name="message">The message to register.</param>
            /// <param name="target">The target object to invoke, or null.</param>
            /// <param name="method">The method to invoke.</param>
            /// <param name="actionType">The type of the Action delegate.</param>
            internal void AddAction(string message, object target, MethodInfo method, Type actionType)
            {
                if (message == null)
                    throw new ArgumentNullException("message");



                if (method == null)
                    throw new ArgumentNullException("method");



                lock (_map)
                {
                    if (!_map.ContainsKey(message))
                        _map[message] = new List<WeakAction>();



                    _map[message].Add(new WeakAction(target, method, actionType));
                }
            }



            /// <summary>
            /// Gets the list of actions to be invoked for the specified message
            /// </summary>
            /// <param name="message">The message to get the actions for</param>
            /// <returns>Returns a list of actions that are registered to the specified message</returns>
            internal List<Delegate> GetActions(string message)
            {
                if (message == null)
                    throw new ArgumentNullException("message");



                List<Delegate> actions;

                lock (_map)
                {
                    if (!_map.ContainsKey(message))
                        return null;



                    List<WeakAction> weakActions = _map[message];

                    actions = new List<Delegate>(weakActions.Count);



                    for (int i = weakActions.Count - 1; i >= -1 + 1; i += -1)
                    {
                        WeakAction weakAction = weakActions[i];



                        if (weakAction == null)
                            continue;



                        Delegate action = weakAction.CreateAction();



                        if (action != null)
                            actions.Add(action);
                        else

                            // The target object is dead, so get rid of the weak action.

                            weakActions.Remove(weakAction);
                    }



                    // Delete the list from the map if it is now empty.

                    if (weakActions.Count == 0)
                        _map.Remove(message);
                }

                return actions;
            }
        }



        /// <summary>
        /// This class is an implementation detail of the MessageToActionsMap class.
        /// </summary>
        private class WeakAction
        {
            private readonly Type _delegateType;

            private readonly MethodInfo _method;

            private readonly WeakReference _targetRef;



            /// <summary>
            /// Constructs a WeakAction.
            /// </summary>
            /// <param name="target">The object on which the target method is invoked, or null if the method is static.</param>
            /// <param name="method">The MethodInfo used to create the Action.</param>
            /// <param name="parameterType">The type of parameter to be passed to the action. Pass null if there is no parameter.</param>
            internal WeakAction(object target, MethodInfo method, Type parameterType)
            {
                if (target == null)
                    _targetRef = null;
                else
                    _targetRef = new WeakReference(target);



                _method = method;



                if (parameterType == null)
                    _delegateType = typeof(Action);
                else
                    _delegateType = typeof(Action<>).MakeGenericType(parameterType);
            }



            /// <summary>
            /// Creates a "throw away" delegate to invoke the method on the target, or null if the target object is dead.
            /// </summary>
            internal Delegate CreateAction()
            {



                // Rehydrate into a real Action object, so that the method can be invoked.

                if (_targetRef == null)
                    return Delegate.CreateDelegate(_delegateType, _method);
                else
                    try
                    {
                        object target = _targetRef.Target;



                        if (target != null)
                            return Delegate.CreateDelegate(_delegateType, target, _method);
                    }


                    catch
                    {
                    }



                return null;
            }
        }



        private readonly MessageToActionsMap _messageToActionsMap = new MessageToActionsMap();
    }

}
