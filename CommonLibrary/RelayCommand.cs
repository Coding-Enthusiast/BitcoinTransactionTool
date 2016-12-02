using System;
using System.Windows.Input;

namespace CommonLibrary
{
    public class BindableCommand : ICommand
    {
        public BindableCommand(Action<object> parameterizedAction)
        {
            ExecuteMethod = parameterizedAction;
        }
        public BindableCommand(Action<object> parameterizedAction, Func<bool> canExecute)
        {
            ExecuteMethod = parameterizedAction;
            CanExecuteMethod = canExecute;
        }


        private Action<object> ExecuteMethod;
        private Func<bool> CanExecuteMethod;


        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }


        public bool CanExecute(object parameter)
        {
            if (CanExecuteMethod != null)
            {
                return CanExecuteMethod();
            }
            if (ExecuteMethod != null)
            {
                return true;
            }
            return false;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            if (ExecuteMethod != null)
            {
                ExecuteMethod(parameter);
            }
        }
    }



    public class RelayCommand : ICommand
    {
        public RelayCommand(Action executeMethod)
        {
            ExecuteMethod = executeMethod;
        }
        public RelayCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            ExecuteMethod = executeMethod;
            CanExecuteMethod = canExecuteMethod;
        }


        Action ExecuteMethod;
        Func<bool> CanExecuteMethod;


        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }


        public bool CanExecute(object parameter)
        {
            if (CanExecuteMethod != null)
            {
                return CanExecuteMethod();
            }
            if (ExecuteMethod != null)
            {
                return true;
            }
            return false;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            if (ExecuteMethod != null)
            {
                ExecuteMethod();
            }
        }
    }
}
