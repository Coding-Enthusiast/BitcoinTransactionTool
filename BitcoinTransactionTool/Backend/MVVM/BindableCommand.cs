// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Windows.Input;

namespace BitcoinTransactionTool.Backend.MVVM
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


        private readonly Action<object> ExecuteMethod;
        private readonly Func<bool> CanExecuteMethod;


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
            ExecuteMethod?.Invoke(parameter);
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


        private readonly Action ExecuteMethod;
        private readonly Func<bool> CanExecuteMethod;


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
            ExecuteMethod?.Invoke();
        }
    }
}
