﻿using System;

namespace CLP.Entities.Demo
{
    /// <summary>Executes a particular piece of code based on the type of the argument.</summary>
    /// <example>
    ///     Usage example:
    ///     <code>
    /// public string GetName(object value)
    /// {
    ///     string name = null;
    ///     TypeSwitch.On(operand)
    ///         .Case((C x) => name = x.FullName)
    ///         .Case((B x) => name = x.LongName)
    ///         .Case((A x) => name = x.Name)
    ///         .Case((X x) => name = x.ToString(CultureInfo.CurrentCulture))
    ///         .Case((Y x) => name = x.GetIdentifier())
    ///         .Default((x) => name = x.ToString());
    ///     return name;
    /// }
    /// </code>
    /// </example>
    /// <remarks>Created by Virtlink. Original source code on GitHub:
    ///     <see href="https://gist.github.com/Virtlink/8722649" />.</remarks>
    public static class TypeSwitch
    {
        /// <summary>Executes a particular piece of code based on the type of the argument.</summary>
        /// <typeparam name="TSource">The argument's type.</typeparam>
        /// <param name="value">The switch argument.</param>
        /// <returns>An object on which the switch cases can be specified.</returns>
        public static Switch<TSource> On<TSource>(TSource value) { return new Switch<TSource>(value); }

        /// <summary>Internal class used by the <see cref="TypeSwitch" /> static class.</summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        public sealed class Switch<TSource>
        {
            /// <summary>The source value.</summary>
            private readonly TSource _value;

            /// <summary>Whether a switch case handled the value.</summary>
            private bool _handled;

            /// <summary>Initializes a new instance of the <see cref="Switch{TSource}" /> class.</summary>
            /// <param name="value">The switch value.</param>
            internal Switch(TSource value) { _value = value; }

            /// <summary>Executes the specified piece of code when the type of the argument is assignable to the specified type.</summary>
            /// <typeparam name="TTarget">The target type.</typeparam>
            /// <param name="action">The action to execute.</param>
            /// <returns>An object on which further switch cases can be specified.</returns>
            public Switch<TSource> Case<TTarget>(Action action) where TTarget : TSource
            {
                if (action == null)
                {
                    throw new ArgumentNullException("action");
                }

                return Case<TTarget>(_ => action());
            }

            /// <summary>Executes the specified piece of code when the type of theargument is assignable to the specified type.</summary>
            /// <typeparam name="TTarget">The target type.</typeparam>
            /// <param name="action">The action to execute.</param>
            /// <returns>An object on which further switch cases can be specified.</returns>
            public Switch<TSource> Case<TTarget>(Action<TTarget> action) where TTarget : TSource
            {
                if (action == null)
                {
                    throw new ArgumentNullException("action");
                }

                if (!_handled &&
                    _value is TTarget)
                {
                    action((TTarget)_value);
                    _handled = true;
                }

                return this;
            }

            /// <summary>Executes the specified piece of code when none of the other cases handles the specified type.</summary>
            /// <param name="action">The action to execute.</param>
            public void Default(Action action)
            {
                if (action == null)
                {
                    throw new ArgumentNullException("action");
                }

                Default(_ => action());
            }

            /// <summary>Executes the specified piece of code when none of the other cases handles the specified type.</summary>
            /// <param name="action">The action to execute.</param>
            public void Default(Action<TSource> action)
            {
                if (action == null)
                {
                    throw new ArgumentNullException("action");
                }

                if (!_handled)
                {
                    action(_value);
                }
            }
        }
    }
}