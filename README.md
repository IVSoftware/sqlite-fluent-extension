As an  alternative to my first answer, if you _do_ like the LINQ-like fluent syntax, there is nothing preventing you from making your own fluent extension that would allow a call to `ClearEmployee()` in this format:

~~~
/// <summary>
/// Clear the id selected on the UI and display the modified records.
/// </summary>
 private async void OnTestFluentClicked(object sender, EventArgs e)
{
    if (OldId.HasValue)
    {
        Jobs.Clear();
        (await 
            myDatabase.Table<Job>()
            .ClearEmployee(OldId.Value))
            .ForEach(_ => Jobs.Add(_));
    }
}
public IList Jobs { get; } = new ObservableCollection<Job>();
public int? OldId { get; set; }
~~~

___

Your extension could be coded like this for example:

~~~
static partial class Extensions
{
    public static async Task<List<Job>> ClearEmployee(this AsyncTableQuery<Job> query, int oldId)
    {
        var jobs = await query.Where(_=>_.EmployeeId == oldId).ToListAsync();
        jobs.ForEach(_ => _.EmployeeId = 0);
        // Visible because of:
        // using static sqlite_fluent_extension.MainPage;
        await myDatabase.UpdateAllAsync(jobs); 
        return jobs;
    }
}
~~~

___

**Minimal Example**

[![before and after clear employee][2]][2]

###### Code-behind

~~~
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

    /// <summary>
    /// Clear the id selected on the UI and display the modified records.
    /// </summary>
    private async void OnTestFluentClicked(object sender, EventArgs e)
    {
        if (OldId.HasValue)
        {
            Jobs.Clear();
            (await 
                myDatabase.Table<Job>()
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
        var jobs = await query.Where(_=>_.EmployeeId == oldId).ToListAsync();
        jobs.ForEach(_ => _.EmployeeId = 0);
        // Visible because of:
        // using static sqlite_fluent_extension.MainPage;
        await myDatabase.UpdateAllAsync(jobs); 
        return jobs;
    }
}
~~~

**XAML**

~~~
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="sqlite_fluent_extension.MainPage"
             BackgroundColor="White">
    <ScrollView>
        <VerticalStackLayout
            Padding="30,0"
            Spacing="25">
            <Image
                Source="dotnet_bot.png"
                HeightRequest="185"
                Aspect="AspectFit"
                SemanticProperties.Description="dot net bot in a race car number eight" />
            <Label
                Text="Welcome to &#10;Fluent Extension"
                Style="{StaticResource SubHeadline}"
                SemanticProperties.HeadingLevel="Level2" />
            <Entry
                Placeholder="Enter Employee ID"
                Keyboard="Numeric"
                Text="{Binding OldId, Mode=OneWayToSource}"
                HorizontalOptions="Fill"
                FontSize="18"
                Margin="0,10,0,0" />
            <Button
                x:Name="buttonTest"
                Text="Test Fluent" 
                Clicked="OnTestFluentClicked"
                HorizontalOptions="Fill"
                IsEnabled="False"/>            
            <CollectionView 
                x:Name="JobsCollection"
                ItemsSource="{Binding Jobs}"
                Margin="0,20,0,0"
                VerticalOptions="FillAndExpand">
                <CollectionView.ItemsLayout>                    
                    <LinearItemsLayout
                        Orientation="Vertical"
                        ItemSpacing="1" />
                </CollectionView.ItemsLayout>
                <CollectionView.ItemTemplate>
                    <DataTemplate>
                        <Frame Padding="10"
                               Margin="5,5,5,0"
                               CornerRadius="8"
                               BackgroundColor="#F0F0F0"
                               HasShadow="True">
                            <VerticalStackLayout>
                                <Label Text="{Binding Name}"
                                       FontSize="18"
                                       FontAttributes="Bold"
                                       TextColor="Black" />
                                <Label Text="{Binding EmployeeId}"
                                       FontSize="14"
                                       TextColor="Gray" />
                            </VerticalStackLayout>
                        </Frame>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
~~~


  [1]: https://i.sstatic.net/Z43poFcm.png
  [2]: https://i.sstatic.net/Cbej8ccr.png