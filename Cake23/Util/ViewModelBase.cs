using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Cake23.Util
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged<T>(Expression<Func<T>> exp)
		{
			MemberExpression memex = (MemberExpression)exp.Body;
			string propName = memex.Member.Name;
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propName));
			}
		}
	}
}