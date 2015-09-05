using System.ComponentModel;
using System.Windows.Data;

namespace Forklift
{
	class MainWindowDataContext
	{
		public ICollectionView Notifications { get; private set; }

		public MainWindowDataContext(Database database)
		{
			Notifications = CollectionViewSource.GetDefaultView(database.Notifications);
		}
	}
}
