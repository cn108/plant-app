using System.Collections.ObjectModel;
using FinalYearProject.Models;
using SQLite;

namespace FinalYearProject
{
    public sealed class PlantAppDatabase
    {
        private static readonly object DatabaseLock = new();
        private readonly SQLiteConnection _connection;

        public PlantAppDatabase()
        {
            _connection = new SQLiteConnection(DBConnection.DatabasePath, DBConnection.Flags);
            _connection.CreateTable<DBUsers>();
            _connection.CreateTable<DBPlants>();
            _connection.CreateTable<ForumMessage>();
            _connection.CreateTable<CropBed>();
            _connection.CreateTable<PlantCareTask>();
        }

        public DBUsers? GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            lock (DatabaseLock)
            {
                var normalizedEmail = email.Trim().ToLowerInvariant();
                return _connection.Table<DBUsers>()
                    .ToList()
                    .FirstOrDefault(user => string.Equals(
                        user.Email?.Trim(),
                        normalizedEmail,
                        StringComparison.OrdinalIgnoreCase));
            }
        }

        public ObservableCollection<DBUsers> GetUser()
        {
            lock (DatabaseLock)
            {
                return new ObservableCollection<DBUsers>(
                    _connection.Table<DBUsers>().OrderBy(user => user.Username).ToList());
            }
        }

        public void AddUser(DBUsers user)
        {
            ArgumentNullException.ThrowIfNull(user);
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
            {
                throw new ArgumentException("A user email and password hash are required.", nameof(user));
            }

            lock (DatabaseLock)
            {
                if (_connection.Table<DBUsers>().ToList()
                    .Any(existing => string.Equals(
                        existing.Email?.Trim(),
                        user.Email.Trim(),
                        StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("An account with this email already exists.");
                }

                user.Email = user.Email.Trim().ToLowerInvariant();
                user.CreatedAt = DateTime.Now;
                _connection.Insert(user);
            }
        }

        public void DeleteUser(int userId)
        {
            lock (DatabaseLock)
            {
                _connection.Delete<DBUsers>(userId);
                _connection.Execute("DELETE FROM DBPlant WHERE UserId = ?", userId);
                _connection.Execute("DELETE FROM PlantCareTasks WHERE UserId = ?", userId);
            }
        }

        public void AddPlant(DBPlants plant)
        {
            ArgumentNullException.ThrowIfNull(plant);
            if (string.IsNullOrWhiteSpace(plant.PlantName))
            {
                throw new ArgumentException("A plant name is required.", nameof(plant));
            }

            lock (DatabaseLock)
            {
                _connection.Insert(plant);
                _connection.Insert(new PlantCareTask
                {
                    UserId = plant.UserId,
                    PlantId = plant.PId,
                    PlantName = plant.PlantName,
                    TaskDescription = $"Water the plant ({plant.WaterPerLiters} L).",
                    TaskDate = DateTime.Now,
                    DueDate = DateTime.Today.AddDays(1)
                });
            }
        }

        public ObservableCollection<DBPlants> GetPlantsByUser(int userId)
        {
            lock (DatabaseLock)
            {
                return new ObservableCollection<DBPlants>(
                    _connection.Table<DBPlants>().Where(plant => plant.UserId == userId).ToList());
            }
        }

        public ObservableCollection<DBPlants> SearchPlants(string searchText)
        {
            lock (DatabaseLock)
            {
                IEnumerable<DBPlants> plants = _connection.Table<DBPlants>()
                    .Where(plant => plant.UserId == App.CurrentUserId)
                    .ToList();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var normalizedQuery = searchText.Trim();
                    plants = plants.Where(plant => plant.PlantName?.Contains(
                        normalizedQuery,
                        StringComparison.OrdinalIgnoreCase) == true);
                }

                return new ObservableCollection<DBPlants>(plants);
            }
        }

        public void DeletePlant(int plantId)
        {
            lock (DatabaseLock)
            {
                _connection.Delete<DBPlants>(plantId);
                _connection.Execute("DELETE FROM PlantCareTasks WHERE PlantId = ?", plantId);
            }
        }

        public void AddForumMessage(ForumMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);
            lock (DatabaseLock)
            {
                _connection.Insert(message);
            }
        }

        public List<ForumMessage> GetAllMessages()
        {
            lock (DatabaseLock)
            {
                return _connection.Table<ForumMessage>()
                    .OrderByDescending(message => message.CreatedAt)
                    .ToList();
            }
        }

        public List<CropBed> GetAllBeds()
        {
            lock (DatabaseLock)
            {
                return _connection.Table<CropBed>().OrderBy(bed => bed.Position).ToList();
            }
        }

        public void SaveBedConfiguration(CropBed bed)
        {
            ArgumentNullException.ThrowIfNull(bed);
            lock (DatabaseLock)
            {
                var existingBed = _connection.Table<CropBed>()
                    .FirstOrDefault(item => item.Position == bed.Position);
                if (existingBed is null)
                {
                    _connection.Insert(bed);
                }
                else
                {
                    existingBed.CurrentCrop = bed.CurrentCrop;
                    _connection.Update(existingBed);
                }
            }
        }

        public List<PlantCareTask> GetPlantCareTasksByUser(int userId)
        {
            lock (DatabaseLock)
            {
                return _connection.Table<PlantCareTask>()
                    .Where(task => task.UserId == userId)
                    .OrderBy(task => task.DueDate)
                    .ToList();
            }
        }

        public void AddPlantCareTask(PlantCareTask task)
        {
            ArgumentNullException.ThrowIfNull(task);
            lock (DatabaseLock)
            {
                _connection.Insert(task);
            }
        }

        public void DeletePlantCareTask(int taskId)
        {
            lock (DatabaseLock)
            {
                _connection.Delete<PlantCareTask>(taskId);
            }
        }
    }
}
