using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;

namespace Classroom_Learning_Partner
{
    public static class ObservableCollectionExtensions
    {
        public static IObservable<ObservableCollectionOperation<T>> ToOperations<T>(this ObservableCollection<T> @this)
        {
            return Observable.Create<ObservableCollectionOperation<T>>(o =>
                                                                       {
                                                                           var local = new List<T>(@this);

                                                                           Func<NotifyCollectionChangedEventArgs, ObservableCollectionOperation<T>[]> getAdds = ea =>
                                                                                                                                                                {
                                                                                                                                                                    var xs = new T[] {};
                                                                                                                                                                    if(ea.Action ==
                                                                                                                                                                       NotifyCollectionChangedAction.Add ||
                                                                                                                                                                       ea.Action ==
                                                                                                                                                                       NotifyCollectionChangedAction
                                                                                                                                                                           .Replace)
                                                                                                                                                                    {
                                                                                                                                                                        xs =
                                                                                                                                                                            ea.NewItems.Cast<T>()
                                                                                                                                                                              .ToArray();
                                                                                                                                                                        local.AddRange(xs);
                                                                                                                                                                    }
                                                                                                                                                                    return
                                                                                                                                                                        xs.Select(
                                                                                                                                                                                  x =>
                                                                                                                                                                                  ObservableCollectionOperation
                                                                                                                                                                                      <T>.Add(x))
                                                                                                                                                                          .ToArray();
                                                                                                                                                                };

                                                                           Func<NotifyCollectionChangedEventArgs, ObservableCollectionOperation<T>[]> getRemoves = ea =>
                                                                                                                                                                   {
                                                                                                                                                                       var xs = new T[] {};
                                                                                                                                                                       if(ea.Action ==
                                                                                                                                                                          NotifyCollectionChangedAction
                                                                                                                                                                              .Remove ||
                                                                                                                                                                          ea.Action ==
                                                                                                                                                                          NotifyCollectionChangedAction
                                                                                                                                                                              .Replace)
                                                                                                                                                                       {
                                                                                                                                                                           xs =
                                                                                                                                                                               ea.OldItems.Cast<T>()
                                                                                                                                                                                 .ToArray();
                                                                                                                                                                           Array.ForEach(xs,
                                                                                                                                                                                         x =>
                                                                                                                                                                                         local.Remove(x));
                                                                                                                                                                       }
                                                                                                                                                                       return
                                                                                                                                                                           xs.Select(
                                                                                                                                                                                     x =>
                                                                                                                                                                                     ObservableCollectionOperation
                                                                                                                                                                                         <T>.Remove(x))
                                                                                                                                                                             .ToArray();
                                                                                                                                                                   };

                                                                           Func<NotifyCollectionChangedEventArgs, ObservableCollectionOperation<T>[]> getClears = ea =>
                                                                                                                                                                  {
                                                                                                                                                                      var xs = new T[] {};
                                                                                                                                                                      if(ea.Action ==
                                                                                                                                                                         NotifyCollectionChangedAction
                                                                                                                                                                             .Reset)
                                                                                                                                                                      {
                                                                                                                                                                          xs = local.ToArray();
                                                                                                                                                                          local.Clear();
                                                                                                                                                                      }
                                                                                                                                                                      return
                                                                                                                                                                          xs.Select(
                                                                                                                                                                                    x =>
                                                                                                                                                                                    ObservableCollectionOperation
                                                                                                                                                                                        <T>.Remove(x))
                                                                                                                                                                            .ToArray();
                                                                                                                                                                  };

                                                                           var changes =
                                                                               from ep in
                                                                                   Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                                                                                                                                                                                      h =>
                                                                                                                                                                                      @this
                                                                                                                                                                                          .CollectionChanged
                                                                                                                                                                                      += h,
                                                                                                                                                                                      h =>
                                                                                                                                                                                      @this
                                                                                                                                                                                          .CollectionChanged
                                                                                                                                                                                      -= h)
                                                                               let adds = getAdds(ep.EventArgs)
                                                                               let removes = getRemoves(ep.EventArgs)
                                                                               let clears = getClears(ep.EventArgs)
                                                                               from x in clears.Concat(removes).Concat(adds).ToObservable()
                                                                               select x;

                                                                           return changes.Subscribe(o);
                                                                       });
        }

        public static IObservable<ObservableCollectionOperation<T>> ToOperations<T>(this ObservableCollection<T> @this, Func<T, bool> filter)
        {
            return @this.ToOperations().Where(op => filter(op.Value));
        }

        public static IDisposable Subscribe<T>(this IObservable<ObservableCollectionOperation<T>> @this, ObservableCollection<T> observer)
        {
            return @this.Subscribe(op =>
                                   {
                                       switch(op.Operation)
                                       {
                                           case Operation.Add:
                                               observer.Add(op.Value);
                                               break;
                                           case Operation.Remove:
                                               observer.Remove(op.Value);
                                               break;
                                       }
                                   });
        }
    }
}