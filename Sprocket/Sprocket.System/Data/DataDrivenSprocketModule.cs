using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Data
{
	public abstract class DataDrivenSprocketModule<T> : ISprocketModule
		where T : IModuleDataProvider
	{
		private T dataProvider;

		public virtual void AttachEventHandlers(ModuleRegistry registry)
		{
			DatabaseManager.Instance.OnDatabaseHandlerLoaded += new NotificationEventHandler<IDatabaseHandler>(DatabaseManager_OnDatabaseHandlerLoaded);
		}

		private void DatabaseManager_OnDatabaseHandlerLoaded(IDatabaseHandler source)
		{
			source.OnInitialise += new InterruptableEventHandler(source_OnInitialise);
			foreach (Type t in Core.Modules.GetInterfaceImplementations(typeof(T)))
			{
				T provider = (T)Activator.CreateInstance(t);
				if (provider.DatabaseHandlerType == source.GetType())
				{
					dataProvider = provider;
					break;
				}
			}
		}

		private void source_OnInitialise(Result result)
		{
			if (!result.Succeeded)
				return;
			if (dataProvider == null)
				result.SetFailed(this.GetType().FullName + " has no implementation for " + DatabaseManager.DatabaseEngine.Title);
			else
				dataProvider.Initialise(result);
		}

		public T DataProvider
		{
			get { return dataProvider; }
		}
	}
}
