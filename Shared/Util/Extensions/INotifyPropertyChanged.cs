﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Linq.Expressions.Expression;

namespace ParseTreeVisualizer.Util {
    public static class INotifyPropertyChangedExtensions {
        public static void NotifyChanged<T>(this INotifyPropertyChanged inpc, ref T current, T newValue, Action<PropertyChangedEventArgs> eventRaiser, [CallerMemberName] string name = null) where T : IEquatable<T> {
            if (current.Equals(newValue)) { return; }
            current = newValue;
            eventRaiser(new PropertyChangedEventArgs(name));
        }
        public static void NotifyChanged<T>(this INotifyPropertyChanged inpc, Expression<Func<T>> property, T newValue, Action<PropertyChangedEventArgs> eventRaiser, [CallerMemberName] string name = null) where T : IEquatable<T> {
            var current = property.Compile().Invoke();
            if (current.Equals(newValue)) { return; }
            Lambda<Action>(Assign(property.Body, Constant(newValue))).Compile().Invoke();
            eventRaiser(new PropertyChangedEventArgs(name));
        }


        public static void NotifyChanged<T>(this INotifyPropertyChanged inpc, ref T? current, T? newValue, Action<PropertyChangedEventArgs> eventRaiser, [CallerMemberName] string name = null) where T : struct, IEquatable<T> {
            if (current == null && newValue==null) { return; }
            if (current != null && current.Equals(newValue)) { return; }
            current = newValue;
            eventRaiser(new PropertyChangedEventArgs(name));
        }

        public static void NotifyChangedElements<T, U>(this INotifyPropertyChanged inpc, ref T current, T newValue, Action<PropertyChangedEventArgs> eventRaiser, [CallerMemberName] string name = null) where T : IEnumerable<U> {
            if (current == null && newValue==null) { return; }
            if (current!= null && current.SequenceEqual(newValue)) { return; }
            current = newValue;
            eventRaiser(new PropertyChangedEventArgs(name));
        }
    }
}
