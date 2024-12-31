using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject
{
    public class DBUsersListViewModel: INotifyPropertyChanged
    {
        // Create a connection to the trip database
        PlantAppDatabase plantappDatabase = new PlantAppDatabase();

        public ObservableCollection<DBUsers> FilteredUsers { get; set; }

        // Constructor for the view model
        public DBUsersListViewModel()
        {
            // Initialize the trip database
            AllUsers = plantappDatabase.GetUser();
        }

        // The Trips property that holds the details of all trips
        private ObservableCollection<DBUsers> allusers;
        public ObservableCollection<DBUsers> AllUsers
        {
            get { return allusers; }
            set
            {
                if (value != null)
                {
                    allusers = value;
                    OnPropertyChanged("AllUsers");
                }
            }
        }

        private string searchTerm;
        public string SearchTerm
        {
            get
            {
                return searchTerm;
            }
            set
            {
                if (value != null)
                {
                    searchTerm = value;
                    OnPropertyChanged("SearchTerm");

                    //AllUsers = plantappDatabase.SearchUsers(searchTerm);
                }
            }
        }

        public void FilterUsers(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                FilteredUsers = new ObservableCollection<DBUsers>(AllUsers);
            }
            else
            {
                FilteredUsers = new ObservableCollection<DBUsers>(
                    AllUsers.Where(user => user.Username.ToLower().Contains(searchText.ToLower())));
            }
            OnPropertyChanged(nameof(FilteredUsers));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}