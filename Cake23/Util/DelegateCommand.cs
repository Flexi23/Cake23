using System;
using System.Windows.Input;

namespace Cake23.Util
{
	public class DelegateCommand : ICommand
	{
		public static Func<object, bool> CanExecuteTrue = (obj) => { return true; };

		private readonly Action<object> _executeDelegate;
		private readonly Func<object, bool> _canExecuteDelegate;

		public DelegateCommand(Action<object> executeDelegate)
			: this(executeDelegate, null)
		{

		}

		public DelegateCommand(Action<object> executeDelegate, Func<object, bool> canExecuteDelegate)
		{
			_executeDelegate = executeDelegate;
			_canExecuteDelegate = canExecuteDelegate;
		}

		public bool CanExecute(object parameter)
		{
			return _canExecuteDelegate == null ? true : _canExecuteDelegate(parameter);
		}

		public void Execute(object parameter)
		{
			_executeDelegate(parameter);
		}

		public event EventHandler CanExecuteChanged //see http://msdn.microsoft.com/de-de/magazine/dd419663.aspx#id0090030
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}
}
