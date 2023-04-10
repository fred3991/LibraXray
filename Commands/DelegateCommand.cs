using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibraXray.Commands
{
    public class DelegateCommand : DelegateCommandBase
    {
        public DelegateCommand(Action executeMethod)
            : this(executeMethod, () => true)
        {
        }

        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
            : base(delegate
            {
                executeMethod();
            }, (object o) => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException("executeMethod", "DelegateCommandDelegatesCannotBeNull");
            }
        }

        public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod)
        {
            return new DelegateCommand(executeMethod);
        }

        public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod)
        {
            return new DelegateCommand(executeMethod, canExecuteMethod);
        }

        public virtual async Task Execute()
        {
            await Execute(null);
        }

        public virtual bool CanExecute()
        {
            return CanExecute(null);
        }

        private DelegateCommand(Func<Task> executeMethod)
            : this(executeMethod, () => true)
        {
        }

        private DelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
            : base((object o) => executeMethod(), (object o) => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException("executeMethod", "DelegateCommandDelegatesCannotBeNull");
            }
        }
    }

    public interface IActiveAware
    {
        bool IsActive { get; set; }

        event EventHandler IsActiveChanged;
    }

    public static class WeakEventHandlerManager
    {
        private static readonly SynchronizationContext syncContext = SynchronizationContext.Current;

        public static void CallWeakReferenceHandlers(object sender, List<WeakReference> handlers)
        {
            if (handlers != null)
            {
                EventHandler[] array = new EventHandler[handlers.Count];
                int count = 0;
                count = CleanupOldHandlers(handlers, array, count);
                for (int i = 0; i < count; i++)
                {
                    CallHandler(sender, array[i]);
                }
            }
        }

        private static void CallHandler(object sender, EventHandler eventHandler)
        {
            if (eventHandler == null)
            {
                return;
            }

            if (syncContext != null)
            {
                syncContext.Post(delegate
                {
                    eventHandler(sender, EventArgs.Empty);
                }, null);
            }
            else
            {
                eventHandler(sender, EventArgs.Empty);
            }
        }

        private static int CleanupOldHandlers(List<WeakReference> handlers, EventHandler[] callees, int count)
        {
            for (int num = handlers.Count - 1; num >= 0; num--)
            {
                WeakReference weakReference = handlers[num];
                EventHandler eventHandler = weakReference.Target as EventHandler;
                if (eventHandler == null)
                {
                    handlers.RemoveAt(num);
                }
                else
                {
                    callees[count] = eventHandler;
                    count++;
                }
            }

            return count;
        }

        public static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler, int defaultListSize)
        {
            if (handlers == null)
            {
                handlers = ((defaultListSize > 0) ? new List<WeakReference>(defaultListSize) : new List<WeakReference>());
            }

            handlers.Add(new WeakReference(handler));
        }

        public static void RemoveWeakReferenceHandler(List<WeakReference> handlers, EventHandler handler)
        {
            if (handlers == null)
            {
                return;
            }

            for (int num = handlers.Count - 1; num >= 0; num--)
            {
                WeakReference weakReference = handlers[num];
                EventHandler eventHandler = weakReference.Target as EventHandler;
                if (eventHandler == null || eventHandler == handler)
                {
                    handlers.RemoveAt(num);
                }
            }
        }
    }

    public abstract class DelegateCommandBase : ICommand, IActiveAware
    {
        private bool _isActive;

        private List<WeakReference> _canExecuteChangedHandlers;

        protected readonly Func<object, Task> _executeMethod;

        protected readonly Func<object, bool> _canExecuteMethod;

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnIsActiveChanged();
                }
            }
        }


        public virtual event EventHandler CanExecuteChanged
        {
            add
            {
                WeakEventHandlerManager.AddWeakReferenceHandler(ref _canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                WeakEventHandlerManager.RemoveWeakReferenceHandler(_canExecuteChangedHandlers, value);
            }
        }

        public virtual event EventHandler IsActiveChanged;

        protected DelegateCommandBase(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException("executeMethod", "DelegateCommandDelegatesCannotBeNull");
            }

            _executeMethod = delegate (object arg)
            {
                executeMethod(arg);
                return Task.Delay(0);
            };
            _canExecuteMethod = canExecuteMethod;
        }

        protected DelegateCommandBase(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException("executeMethod", "DelegateCommandDelegatesCannotBeNull");
            }

            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        protected virtual void OnCanExecuteChanged()
        {
            WeakEventHandlerManager.CallWeakReferenceHandlers(this, _canExecuteChangedHandlers);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        async void ICommand.Execute(object parameter)
        {
            await Execute(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        protected async Task Execute(object parameter)
        {
            await _executeMethod(parameter);
        }

        protected bool CanExecute(object parameter)
        {
            if (_canExecuteMethod != null)
            {
                return _canExecuteMethod(parameter);
            }

            return true;
        }

        protected virtual void OnIsActiveChanged()
        {
            this.IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
