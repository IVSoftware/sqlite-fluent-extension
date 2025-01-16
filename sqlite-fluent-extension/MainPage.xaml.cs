using SQLite;
using System.Collections;
using System.Collections.ObjectModel;
using static sqlite_fluent_extension.MainPage;

namespace sqlite_fluent_extension
{
    public partial class MainPage : ContentPage
    {
        internal static SQLiteAsyncConnection myDatabase = new (":memory:");
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await myDatabase.CreateTableAsync<Job>();
            await myDatabase.InsertAllAsync(new[]
            {
                new Job { EmployeeId = 1, Name = "Job A" },
                new Job { EmployeeId = 2, Name = "Job B" },
                new Job { EmployeeId = 3, Name = "Job C" },
                new Job { EmployeeId = 4, Name = "Job D" },
                new Job { EmployeeId = 5, Name = "Job E" },
                new Job { EmployeeId = 1, Name = "Job F" },
                new Job { EmployeeId = 2, Name = "Job G" },
                new Job { EmployeeId = 3, Name = "Job H" },
                new Job { EmployeeId = 4, Name = "Job I" },
                new Job { EmployeeId = 5, Name = "Job J" },
            });
            buttonTest.IsEnabled = true;
        }

        private async void OnTestFluentClicked(object sender, EventArgs e)
        {
            if (OldId.HasValue)
            {
                Jobs.Clear();
                (await myDatabase.Table<Job>()
                    .Where(_ => _.EmployeeId == OldId)
                    .ClearEmployee(OldId.Value))
                    .ForEach(_ => Jobs.Add(_));
            }
        }
        public IList Jobs { get; } = new ObservableCollection<Job>();
        public int? OldId { get; set; }
    }
    class Job
    {
        [PrimaryKey]
        public string PK { get; set; } = Guid.NewGuid().ToString();
        public int EmployeeId { get; set; }
        public string? Name { get; set; }
    }
    static partial class Extensions
    {
        public static async Task<List<Job>> ClearEmployee(this AsyncTableQuery<Job> query, int oldId)
        {
            var jobs = await query.ToListAsync();
            jobs.ForEach(_ => _.EmployeeId = 0);
            await myDatabase.UpdateAllAsync(jobs);
            return jobs;
        }
    }
}
