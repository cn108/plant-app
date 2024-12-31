using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FinalYearProject
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private PlantAppDatabase _database;
        public ObservableCollection<DBUsers> Users { get; set; }

        public AdminViewModel()
        {
            _database = new PlantAppDatabase();
            Users = new ObservableCollection<DBUsers>(_database.GetUser());

            OnPropertyChanged(nameof(Users));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
